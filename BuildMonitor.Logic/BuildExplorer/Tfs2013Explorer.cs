
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

   /// <summary>
   /// Implementation of the <see cref="IBuildExplorer"/> for TFS version 2013.
   /// </summary>
   internal class Tfs2013Explorer : IBuildExplorer
   {
      private readonly List<string> validTestTexts = new List<string> { "Run MSTest for Test Assemblies", "Run Tests on Environment" };
      
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
            catch (HttpRequestException ex)
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
               var buildstatusString = string.Format("{0}", item.status);
               var buildStatus = (BuildStatus)Enum.Parse(typeof(BuildStatus), buildstatusString, true);
               if (buildStatus == BuildStatus.InProgress)
               {
                  if (firstInprogressFound)
                  {
                     continue;
                  }

                  string response2;
                  using (var httpClient = WebClientFactory.CreateHttpClient(buildInformation.DomainName, buildInformation.Login, buildInformation.CryptedPassword))
                  {
                     var buildIdUri = string.Format(buildInformation.TestResultUrl, item.id);
                     var responseMsgs2 = await httpClient.GetAsync(buildIdUri);
                     response2 = await responseMsgs2.Content.ReadAsStringAsync();
                  }

                  UpdateProperties(response2, result);
                  result.IsRunning = true;
                  var buildReason = string.Format("{0}", item.reason);
                  result.IsGatedCheckin = string.Equals(buildReason, "checkInShelveset", StringComparison.InvariantCultureIgnoreCase);
                  result.RunningTfsUri = item.uri;
                  result.RunningBuildId = item.id;
                  result.RunningBuildRequestedBy = item.requests[0].requestedFor.displayName;
                  result.RunningBuildNumber = item.buildNumber;
                  result.RunningBuildTfsUri = item.uri;
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
                  result.TfsUri = item.uri;
                  result.RequestedBy = item.requests[0].requestedFor.displayName;
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

      /// <summary>Gets the build definitions.</summary>
      /// <param name="buildServer">The build server.</param>
      /// <returns>The <see cref="TfsConnectResult" />.</returns>
      public async Task<TfsConnectResult> GetBuildDefinitions(BuildServer buildServer)
      {
         string response;
         using (var httpClient = WebClientFactory.CreateHttpClient(buildServer.DomainName, buildServer.Login, buildServer.PasswordBytes))
         {
            HttpResponseMessage responseMessage;
            try
            {
               responseMessage = await httpClient.GetAsync(buildServer.Url);
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

         dynamic jsonObject = JsonConvert.DeserializeObject(response);
         var result = new TfsConnectResult();
         try
         {
            foreach (var item in jsonObject.value)
            {
               var buildResult = new BuildDefinitionResult { Id = item.id, Name = item.name, Uri = item.uri, Url = item.url };
               if (buildServer.TfsVersion == TfsVersion.Version2015)
               {
                  buildResult.ProjectName = item.project.name;
               }

               result.BuildDefinitions.Add(buildResult);
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
      /// <returns>True if stopping the build succeed; otherwise false.</returns>
      public async Task<BuildResult> StopBuild(BuildInformation buildInformation, string buildId)
      {
         string response;
         try
         {
            using (var httpClient = WebClientFactory.CreateHttpClient(buildInformation.DomainName, buildInformation.Login, buildInformation.CryptedPassword))
            {
               httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
               var requestUri = string.Format(buildInformation.StopBuildUrl, buildId);
               var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri) { Content = new StringContent("{ \"status\": \"Stopped\" }", Encoding.UTF8, "application/json") };

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
         var buildStatus = (BuildStatus)Enum.Parse(typeof(BuildStatus), buildstatusString, true);
         return new BuildResult { Name = jsonObject.buildNumber, Status = buildStatus, RequestedBy = jsonObject.lastChangedBy.displayName, TfsUri = jsonObject.uri };
      }

      /// <summary>Requests the build.</summary>
      /// <param name="buildInformation">The build information.</param>
      /// <returns>True if requesting the build succeed; otherwise false.</returns>
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
         var buildStatus = (BuildStatus)Enum.Parse(typeof(BuildStatus), buildstatusString, true);
         string buildId = string.Concat("vstfs:///Build/Build/", jsonObject.builds[0].id);
         return new BuildResult
         {
            Name = jsonObject.definition.name,
            Status = buildStatus,
            RequestedBy = jsonObject.requestedBy.displayName,
            TfsUri = buildId
         };
      }

      private void UpdateProperties(string response, BuildResult buildResult)
      {
         var rootObject = JsonConvert.DeserializeObject<RootObject>(response);
         foreach (var node in rootObject.information[0].nodes)
         {
            RecursivelyCheckBuildWaiting(buildResult, node);
         }

         foreach (var node in rootObject.information[0].nodes)
         {
            RecursivelyCheckTestsFailed(buildResult, node);
         }

         foreach (var node in rootObject.information[0].nodes)
         {
            RecursivelyCheckTestsRunning(buildResult, node);
         }
      }

      private void RecursivelyCheckBuildWaiting(BuildResult buildResult, Node node)
      {
         if (buildResult.Waiting || string.IsNullOrEmpty(node.text))
         {
            return;
         }

         if (node.text.Equals("Run On Agent (waiting for build agent)", StringComparison.InvariantCultureIgnoreCase))
         {
            buildResult.Waiting = true;
            return;
         }

         if (node.nodes == null)
         {
            return;
         }

         foreach (var subNode in node.nodes)
         {
            RecursivelyCheckBuildWaiting(buildResult, subNode);
         }
      }

      private void RecursivelyCheckTestsFailed(BuildResult buildResult, Node node)
      {
         if (buildResult.TestsFailed)
         {
            return;
         }

         AreTestsFailed(buildResult, node);
         if (node.nodes == null)
         {
            return;
         }

         foreach (var subNode in node.nodes)
         {
            RecursivelyCheckTestsFailed(buildResult, subNode);
         }
      }

      private void RecursivelyCheckTestsRunning(BuildResult buildResult, Node node)
      {
         if (buildResult.TestsRunning || buildResult.TestsFailed)
         {
            return;
         }

         AreTestsRunning(buildResult, node);
         if (node.nodes == null)
         {
            return;
         }

         foreach (var subNode in node.nodes)
         {
            RecursivelyCheckTestsRunning(buildResult, subNode);
         }
      }

      private static void AreTestsFailed(BuildResult buildResult, Node node)
      {
         var areTestsRunning = string.Format("{0}", node.status);
         if (areTestsRunning.Equals("error", StringComparison.InvariantCultureIgnoreCase) || node.errors > 0)
         {
            buildResult.TestsFailed = true;
         }
      }

      private void AreTestsRunning(BuildResult buildResult, Node node)
      {
         var text = string.Format("{0}", node.text);
         if (!validTestTexts.Any(x => x.Equals(text, StringComparison.InvariantCultureIgnoreCase)))
         {
            return;
         }

         var areTestsRunning = string.Format("{0}", node.status);
         if (areTestsRunning.Equals("inprogress", StringComparison.InvariantCultureIgnoreCase))
         {
            buildResult.TestsRunning = true;
         }

         if (areTestsRunning.Equals(string.Empty, StringComparison.InvariantCultureIgnoreCase))
         {
            buildResult.TestsFinished = true;
         }
      }
   }
}
