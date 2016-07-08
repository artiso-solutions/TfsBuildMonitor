

namespace BuildMonitor.Logic.BuildExplorer
{
   using System;
   using System.Collections.Generic;
   using System.Globalization;
   using System.Linq;
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
      public async Task<BuildResultCollection> GetBuildResultCollection(BuildInformation buildInformation)
      {
         var collectionResult = new BuildResultCollection();

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
               collectionResult.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
               return collectionResult;
            }

            if (!responseMessage.IsSuccessStatusCode)
            {
               collectionResult.ErrorMessage = string.Format("Error {0}:{1} - {2}", (int)responseMessage.StatusCode,
                  responseMessage.StatusCode, responseMessage.ReasonPhrase);
               return collectionResult;
            }

            response = await responseMessage.Content.ReadAsStringAsync();
         }

         try
         {
            dynamic jsonObject = JsonConvert.DeserializeObject(response);

            foreach (var jsonItem in jsonObject.value)
            {
               var buildResult = new BuildResult();
               collectionResult.BuildResults.Add(buildResult);

               buildResult.Status = ReadBuildStatus(jsonItem);

               string webLinkUri = jsonItem._links.web.href;
               var uri = new Uri(webLinkUri);
               if (string.Equals(uri.Host, "tfs", StringComparison.InvariantCultureIgnoreCase))
               {
                  var host = new Uri(buildInformation.BuildDefinitionUrl).Host + ":" + uri.Port;
                  webLinkUri = string.Format("{0}://{1}{2}{3}", uri.Scheme, host, string.Join(string.Empty, uri.Segments), uri.Query);
               }

               buildResult.Id = jsonItem.id;
               buildResult.Name = jsonItem.definition.name;
               buildResult.Number = jsonItem.buildNumber;
               buildResult.TfsUri = webLinkUri;
               buildResult.RequestedBy = jsonItem.requestedFor.displayName;

               string sourceVersion = jsonItem.sourceVersion;
               int changeset;
               if (!string.IsNullOrEmpty(sourceVersion) && int.TryParse(sourceVersion.Replace("C", string.Empty), out changeset))
               {
                  buildResult.SourceVersion = changeset;
               }

               string finishTimeString = jsonItem.finishTime == null ? string.Empty : string.Format("{0}", jsonItem.finishTime);
               var finishDate = DateTime.MinValue;
               if (!string.IsNullOrEmpty(finishTimeString))
               {
                  finishDate = DateTime.Parse(finishTimeString, CultureInfo.CurrentCulture,
                     DateTimeStyles.AssumeUniversal);
                  buildResult.FinishTime = finishDate.ToString("g", CultureInfo.CurrentCulture);
               }

               string startTimeString = jsonItem.startTime == null ? string.Empty : string.Format("{0}", jsonItem.startTime);
               if (!string.IsNullOrEmpty(startTimeString))
               {
                  var startTime = DateTime.Parse(startTimeString, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal);
                  if (finishDate != DateTime.MinValue)
                  {
                     var delta = finishDate - startTime;
                     buildResult.Duration = delta.TotalMinutes;
                  }
               }

               if (buildResult.Status == BuildStatus.NotStarted)
               {
                  FillNotStartedProperties(jsonItem, buildResult, webLinkUri);
                  continue;
               }

               if (buildResult.Status == BuildStatus.InProgress)
               {
                  await FillInProgressProperties(buildInformation, jsonItem, buildResult, webLinkUri);
               }
            }

            return collectionResult;
         }
         catch (Exception ex)
         {
            collectionResult.ErrorMessage = ex.Message;
            return collectionResult;
         }
      }

      public async Task GetTestResultAsync(BuildInformation buildInformation, BuildResult result)
      {
         string response;
         using (var httpClient = WebClientFactory.CreateHttpClient(buildInformation.DomainName, buildInformation.Login, buildInformation.CryptedPassword))
         {
            var postString = string.Format("{{ \"query\":\"SELECT * FROM TestRun WHERE buildUri='vstfs:///Build/Build/{0}'\"}}", result.Id);
            HttpResponseMessage responseMessage;
            try
            {
               responseMessage = await httpClient.PostAsync(buildInformation.TestRunUrl,
                  new StringContent(postString, Encoding.UTF8, "application/json"));
            }
            catch
            {
               return;
            }

            if (!responseMessage.IsSuccessStatusCode)
            {
               return;
            }

            response = await responseMessage.Content.ReadAsStringAsync();
         }

         var testRuns = new List<TestRun>();
         dynamic jsonObject = JsonConvert.DeserializeObject(response);
         foreach (var item in jsonObject.value)
         {
            var testRun = new TestRun();
            int incompletTests = item.incompleteTests;
            int unanalyzedTests = item.unanalyzedTests;
            testRun.Id = item.id;
            testRun.TotalTests = item.totalTests;
            testRun.PassedTests = item.passedTests;
            testRun.FailedTests = incompletTests + unanalyzedTests;
            testRuns.Add(testRun);
         }

         result.TotalTests = testRuns.Sum(x => x.TotalTests);
         result.PassedTests = testRuns.Sum(x => x.PassedTests);
         result.FailedTests = testRuns.Sum(x => x.FailedTests);
      }

      public async Task<Changeset> GetChangesetAsync(BuildInformation buildInformation, int sourceVersion)
      {
         string response;
         using (var httpClient = WebClientFactory.CreateHttpClient(buildInformation.DomainName, buildInformation.Login, buildInformation.CryptedPassword))
         {
            HttpResponseMessage responseMessage;
            try
            {
               responseMessage = await httpClient.GetAsync(string.Format(buildInformation.ChangesetUrl, sourceVersion));
            }
            catch (Exception ex)
            {
               return null;
            }

            if (!responseMessage.IsSuccessStatusCode)
            {
               return null;
            }

            response = await responseMessage.Content.ReadAsStringAsync();
         }

         var changeset = JsonConvert.DeserializeObject<Changeset>(response);
         return changeset;
      }

      private async Task FillInProgressProperties(BuildInformation buildInformation, dynamic jsonItem, BuildResult buildResult,
         string webLinkUri)
      {
         await GetInProgressBuildDetails(buildInformation, jsonItem, buildResult);

         buildResult.IsRunning = true;
         var buildReason = string.Format("{0}", jsonItem.reason);
         buildResult.IsGatedCheckin = string.Equals(buildReason, "checkInShelveset",
            StringComparison.InvariantCultureIgnoreCase);
         buildResult.RunningTfsUri = webLinkUri;
         buildResult.RunningBuildId = jsonItem.id;
         buildResult.RunningBuildRequestedBy = jsonItem.requestedFor.displayName;
         buildResult.RunningBuildNumber = jsonItem.buildNumber;
         buildResult.RunningBuildTfsUri = webLinkUri;
         string sourceVersion = jsonItem.sourceVersion;
         buildResult.RunningBuildSourceVersion = Convert.ToInt32(sourceVersion.Replace("C", string.Empty));
         var runningStartTimeString = string.Format("{0}", jsonItem.startTime);
         if (!string.IsNullOrEmpty(runningStartTimeString))
         {
            buildResult.RunningStartTime = DateTime.Parse(runningStartTimeString, CultureInfo.CurrentCulture,
               DateTimeStyles.AssumeUniversal);
         }
      }

      private static async Task GetInProgressBuildDetails(BuildInformation buildInformation, dynamic jsonItem,
         BuildResult buildResult)
      {
         string response;
         try
         {
            using (
               var httpClient = WebClientFactory.CreateHttpClient(buildInformation.DomainName, buildInformation.Login,
                  buildInformation.CryptedPassword))
            {
               string buildIdUri = jsonItem._links.details.href;
               var responseMsgs2 = await httpClient.GetAsync(buildIdUri);
               response = await responseMsgs2.Content.ReadAsStringAsync();
            }
         }
         catch
         {
            return;
         }

         dynamic jsonObject = JsonConvert.DeserializeObject(response);

         foreach (var item in jsonObject.value)
         {
            if (item.type == "BuildError")
            {
               buildResult.TestsFailed = true;
               string message = item.fields.Message;
               buildResult.FailingDetails.Add(message);
            }

            if (item.type == "ActivityTracking")
            {
               string displayText = item.fields.DisplayText;
               if (displayText.StartsWith("Run MSTest"))
               {
                  buildResult.TestsRunning = true;
               }

               if (displayText == "Copy Files to Drop Location")
               {
                  buildResult.TestsFinished = true;
               }
            }
         }
      }

      private static void FillNotStartedProperties(dynamic jsonItem, BuildResult buildResult, string webLinkUri)
      {
         var buildReason = string.Format("{0}", jsonItem.reason);
         buildResult.IsGatedCheckin = string.Equals(buildReason, "checkInShelveset",
            StringComparison.InvariantCultureIgnoreCase);
         buildResult.WaitingTfsUri = webLinkUri;
         buildResult.WaitingBuildId = jsonItem.id;
         buildResult.WaitingBuildRequestedBy = jsonItem.requestedFor.displayName;
         buildResult.WaitingBuildNumber = jsonItem.buildNumber;
         buildResult.WaitingBuildTfsUri = webLinkUri;
         buildResult.Waiting = true;
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

            if (!responseMessage.IsSuccessStatusCode || responseMessage.StatusCode == HttpStatusCode.NonAuthoritativeInformation)
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
