

namespace BuildMonitor.Logic.BuildExplorer
{
   using System;
   using System.Collections.Generic;
   using System.Globalization;
   using System.Net;
   using System.Net.Http;
   using System.Net.Http.Headers;
   using System.Text;
   using System.Threading.Tasks;

   using BuildMonitor.Logic.Contracts;
   using BuildMonitor.Logic.Interfaces;

   using Newtonsoft.Json;

   /// <summary>Implementation of the <see cref="IBuildExplorer"/> for TFS version 2015.</summary>
   internal class Tfs2015Explorer : IBuildExplorer
   {
      private readonly Dictionary<string, string> buildStatusReplacement = new Dictionary<string, string>
      {
         { "notStarted", "Waiting" }
      };

      private readonly Dictionary<string, string> statusReplacementsTfs2015 = new Dictionary<string, string>
      {
         { "canceled", "stopped" }
      };

      private readonly List<string> inProgressStatesTfs2015 = new List<string> { "inProgress", "cancelling", "postponed" };

      /// <summary>Gets the build result.</summary>
      /// <param name="buildInformation">The build information.</param>
      /// <returns>The <see cref="BuildResult" />.</returns>
      public async Task<BuildResult> GetBuildResult(BuildInformation buildInformation)
      {
         string response;
         using (var httpClient = WebClientFactory.CreateHttpClient(buildInformation.DomainName, buildInformation.Login, buildInformation.CryptedPassword))
         {
            HttpResponseMessage responseMessage;
            try
            {
               responseMessage = await httpClient.GetAsync(buildInformation.BuildDefinitionUrl);
            }
            catch (Exception ex)
            {
               return new BuildResult { Name = ex.InnerException != null ? ex.InnerException.Message : ex.Message, Status = BuildStatus.Unknown };
            }

            if (!responseMessage.IsSuccessStatusCode)
            {
               return new BuildResult
               {
                  Name = string.Format("Error {0}:{1} - {2}", (int)responseMessage.StatusCode, responseMessage.StatusCode, responseMessage.ReasonPhrase),
                  Status = BuildStatus.Unknown
               };
            }

            response = await responseMessage.Content.ReadAsStringAsync();
         }

         try
         {
            dynamic jsonObject = JsonConvert.DeserializeObject(response);

            var result = new BuildResult();
            var firstNotInprogressFound = false;
            var firstInprogressFound = false;

            foreach (var item in jsonObject.value)
            {
               BuildStatus buildStatus = ReadBuildStatus(item);

               string webLinkUri = item._links.web.href;
               var uri = new Uri(webLinkUri);
               if (string.Equals(uri.Host, "tfs", StringComparison.InvariantCultureIgnoreCase))
               {
                  var host = new Uri(buildInformation.BuildDefinitionUrl).Host + ":" + uri.Port;
                  webLinkUri = string.Format("{0}:/{1}{2}{3}", uri.Scheme, host, string.Join(string.Empty, uri.Segments), uri.Query);
               }

               if (buildStatus == BuildStatus.NotStarted)
               {
                  if (firstInprogressFound)
                  {
                     continue;
                  }
                  
                  var buildReason = string.Format("{0}", item.reason);
                  result.IsGatedCheckin = string.Equals(buildReason, "checkInShelveset", StringComparison.InvariantCultureIgnoreCase);
                  result.WaitingTfsUri = webLinkUri;
                  result.WaitingBuildId = item.id;
                  result.WaitingBuildRequestedBy = item.requestedFor.displayName;
                  result.WaitingBuildNumber = item.buildNumber;
                  result.WaitingBuildTfsUri = webLinkUri;
                  result.Waiting = true;
                  continue;
               }

               if (buildStatus == BuildStatus.InProgress)
               {
                  if (firstInprogressFound)
                  {
                     continue;
                  }

                  try
                  {
                     string response2;
                     using (var httpClient = WebClientFactory.CreateHttpClient(buildInformation.DomainName, buildInformation.Login, buildInformation.CryptedPassword))
                     {
                        string buildIdUri = item._links.details.href;
                        var responseMsgs2 = await httpClient.GetAsync(buildIdUri);
                        response2 = await responseMsgs2.Content.ReadAsStringAsync();
                     }

                     UpdateProperties(response2, result);
                  }
                  catch
                  {
                  }

                  result.IsRunning = true;
                  var buildReason = string.Format("{0}", item.reason);
                  result.IsGatedCheckin = string.Equals(buildReason, "checkInShelveset", StringComparison.InvariantCultureIgnoreCase);
                  result.RunningTfsUri = webLinkUri;
                  result.RunningBuildId = item.id;
                  result.RunningBuildRequestedBy = item.requestedFor.displayName;
                  result.RunningBuildNumber = item.buildNumber;
                  result.RunningBuildTfsUri = webLinkUri;
                  var runningStartTimeString = string.Format("{0}", item.startTime);
                  if (!string.IsNullOrEmpty(runningStartTimeString))
                  {
                     result.RunningStartTime = DateTime.Parse(runningStartTimeString, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal);
                  }

                  firstInprogressFound = true;
                  continue;
               }

               if (!firstNotInprogressFound)
               {
                  result.Name = item.definition.name;
                  result.Number = item.buildNumber;
                  result.Status = buildStatus;
                  result.TfsUri = webLinkUri;
                  result.RequestedBy = item.requestedFor.displayName;
                  var finishTimeString = string.Format("{0}", item.finishTime);
                  DateTime finishDate = DateTime.Parse(finishTimeString, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal);
                  result.FinishTime = finishDate.ToString("g", CultureInfo.CurrentCulture);
                  firstNotInprogressFound = true;
               }

               if (buildStatus == BuildStatus.PartiallySucceeded || buildStatus == BuildStatus.Succeeded)
               {
                  var startTimeString = string.Format("{0}", item.startTime);
                  DateTime startTime = DateTime.Parse(startTimeString, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal);

                  var finishTimeString = string.Format("{0}", item.finishTime);
                  DateTime finishTime = DateTime.Parse(finishTimeString, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal);

                  var delta = finishTime - startTime;
                  result.BuildTimes.Add(delta.TotalMinutes);
               }
            }

            return result;
         }
         catch (Exception ex)
         {
            return new BuildResult { Status = BuildStatus.Error, Name = ex.Message };
         }
      }

      private void UpdateProperties(string response, BuildResult result)
      {
         dynamic jsonObject = JsonConvert.DeserializeObject(response);

         foreach (var item in jsonObject.value)
         {
            if (item.type == "BuildError")
            {
               result.TestsFailed = true;
            }

            if (item.type == "ActivityTracking")
            {
               string displayText = item.fields.DisplayText;
               if (displayText.StartsWith("Run MSTest"))
               {
                  result.TestsRunning = true;
               }

               if (displayText == "Copy Files to Drop Location")
               {
                  result.TestsFinished = true;
               }
            }
         }
      }

      private BuildStatus ReadBuildStatus(dynamic item)
      {
         var buildstatusString = string.Format("{0}", item.status);
         if (buildstatusString == "notStarted")
         {
            return BuildStatus.NotStarted;
         }

         if (inProgressStatesTfs2015.Contains(buildstatusString))
         {
            return BuildStatus.InProgress;
         }

         if (buildstatusString == "completed")
         {
            var buildResultString = string.Format("{0}", item.result);

            if (statusReplacementsTfs2015.ContainsKey(buildResultString))
            {
               buildResultString = statusReplacementsTfs2015[buildResultString];
            }

            return (BuildStatus)Enum.Parse(typeof(BuildStatus), buildResultString, true);
         }

         return BuildStatus.Unknown;
      }

      /// <summary>Gets the build definitions.</summary>
      /// <param name="buildServer">The build server.</param>
      /// <returns>The <see cref="TfsConnectResult"/>.</returns>
      public async Task<TfsConnectResult> GetBuildDefinitions(BuildServer buildServer)
      {
         string response;
         using (var httpClient = WebClientFactory.CreateHttpClient(buildServer.DomainName, buildServer.Login, buildServer.PasswordBytes))
         {
            HttpResponseMessage responseMessage;
            try
            {
               responseMessage = await httpClient.GetAsync(string.Concat(buildServer.Url, "/_apis/projects"));
            }
            catch (Exception exception)
            {
               return new TfsConnectResult { Message = exception.InnerException != null ? exception.InnerException.Message : exception.Message };
            }

            if (!responseMessage.IsSuccessStatusCode)
            {
               return new TfsConnectResult
               {
                  Message = string.Format("Error {0}:{1} - {2}", (int)responseMessage.StatusCode, responseMessage.StatusCode, responseMessage.ReasonPhrase)
               };
            }

            response = await responseMessage.Content.ReadAsStringAsync();
         }

         dynamic projectCollectionJson = JsonConvert.DeserializeObject(response);
         var result = new TfsConnectResult();
         try
         {
            foreach (var project in projectCollectionJson.value)
            {
               string projectName = project.name;
               string projectId = project.id;

               using (var httpClient = WebClientFactory.CreateHttpClient(buildServer.DomainName, buildServer.Login, buildServer.PasswordBytes))
               {
                  HttpResponseMessage responseMessage;
                  try
                  {
                     var requestUri = string.Concat(buildServer.Url, "/", projectId, "/_apis/build/definitions?api-version=2.0");
                     responseMessage = await httpClient.GetAsync(requestUri);
                  }
                  catch (HttpRequestException exception)
                  {
                     return new TfsConnectResult { Message = exception.InnerException != null ? exception.InnerException.Message : exception.Message };
                  }

                  if (!responseMessage.IsSuccessStatusCode)
                  {
                     return new TfsConnectResult
                     {
                        Message = string.Format("Error {0}:{1} - {2}", (int)responseMessage.StatusCode, responseMessage.StatusCode, responseMessage.ReasonPhrase)
                     };
                  }

                  response = await responseMessage.Content.ReadAsStringAsync();
               }

               dynamic definitionCollectionJson = JsonConvert.DeserializeObject(response);

               foreach (var definition in definitionCollectionJson.value)
               {
                  var buildResult = new BuildDefinitionResult { Id = definition.id, Name = definition.name, Uri = definition.uri, Url = definition.url, ProjectName = projectName, ProjectId = projectId };

                  result.BuildDefinitions.Add(buildResult);
               }
            }
         }
         catch (Exception ex)
         {
            result.Message = "Error on getting list of builds - May be wrong TFS type - " + ex.Message;
         }

         return result;
      }

      /// <summary>Stops the build.</summary>
      /// <param name="buildInformation">The build information.</param>
      /// <param name="buildId">The build identifier.</param>
      /// <returns>The <see cref="BuildResult"/>.</returns>
      public async Task<BuildResult> StopBuild(BuildInformation buildInformation, string buildId)
      {
         return await new Tfs2013Explorer().StopBuild(buildInformation, buildId);
      }

      /// <summary>Requests the build.</summary>
      /// <param name="buildInformation">The build information.</param>
      /// <returns>The <see cref="BuildResult"/>.</returns>
      public async Task<BuildResult> RequestBuild(BuildInformation buildInformation)
      {
         string response;
         try
         {
            using (var httpClient = WebClientFactory.CreateHttpClient(buildInformation.DomainName, buildInformation.Login, buildInformation.CryptedPassword))
            {
               httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
               var json = string.Format("{{ \"definition\": {{ \"id\": {0} }}, \"reason\": \"Manual\", \"priority\": \"Normal\" }}", buildInformation.BuildDefinitionId);
               var request = new HttpRequestMessage(HttpMethod.Post, buildInformation.RequestBuildUrl) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

               var responseMessage = await httpClient.SendAsync(request);
               if (responseMessage.StatusCode != HttpStatusCode.OK)
               {
                  return new BuildResult { Name = responseMessage.StatusCode.ToString("G") };
               }

               response = await responseMessage.Content.ReadAsStringAsync();
            }
         }
         catch (Exception ex)
         {
            return new BuildResult { Name = ex.Message };
         }

         dynamic jsonObject = JsonConvert.DeserializeObject(response);
         var buildstatusString = string.Format("{0}", jsonObject.status);
         if (buildStatusReplacement.ContainsKey(buildstatusString))
         {
            buildstatusString = buildStatusReplacement[buildstatusString];
         }

         var buildStatus = (BuildStatus)Enum.Parse(typeof(BuildStatus), buildstatusString, true);
         string buildId = jsonObject.uri;
         return new BuildResult
         {
            Name = jsonObject.definition.name,
            Status = buildStatus,
            RequestedBy = jsonObject.requestedBy.displayName,
            TfsUri = buildId
         };
      }
   }
}
