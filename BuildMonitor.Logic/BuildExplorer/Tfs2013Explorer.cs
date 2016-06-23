
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
            catch (HttpRequestException ex)
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

               buildResult.Name = jsonItem.definition.name;
               buildResult.Number = jsonItem.buildNumber;
               buildResult.TfsUri = jsonItem.uri;
               buildResult.RequestedBy = jsonItem.requests[0].requestedFor.displayName;

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

               var buildstatusString = string.Format("{0}", jsonItem.status);
               buildResult.Status = (BuildStatus)Enum.Parse(typeof(BuildStatus), buildstatusString, true);
               if (buildResult.Status == BuildStatus.InProgress)
               {
                  await FillInProgressBuild(buildInformation, jsonItem, buildResult);
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

      public Task GetTestResultAsync(BuildInformation buildInformation, BuildResult result)
      {
         // Todo: Implement me
         return null;
      }

      private async Task FillInProgressBuild(BuildInformation buildInformation, dynamic jsonItem, BuildResult buildResult)
      {
         string response2;
         using (
            var httpClient = WebClientFactory.CreateHttpClient(buildInformation.DomainName, buildInformation.Login,
               buildInformation.CryptedPassword))
         {
            var buildIdUri = string.Format(buildInformation.TestResultUrl, jsonItem.id);
            var responseMsgs2 = await httpClient.GetAsync(buildIdUri);
            response2 = await responseMsgs2.Content.ReadAsStringAsync();
         }

         UpdateProperties(response2, buildResult);
         buildResult.IsRunning = true;
         var buildReason = string.Format("{0}", jsonItem.reason);
         buildResult.IsGatedCheckin = string.Equals(buildReason, "checkInShelveset",
            StringComparison.InvariantCultureIgnoreCase);
         buildResult.RunningTfsUri = jsonItem.uri;
         buildResult.RunningBuildId = jsonItem.id;
         buildResult.RunningBuildRequestedBy = jsonItem.requests[0].requestedFor.displayName;
         buildResult.RunningBuildNumber = jsonItem.buildNumber;
         buildResult.RunningBuildTfsUri = jsonItem.uri;
         var runningStartTimeString = string.Format("{0}", jsonItem.startTime);
         if (!string.IsNullOrEmpty(runningStartTimeString))
         {
            buildResult.RunningStartTime = DateTime.Parse(runningStartTimeString, CultureInfo.CurrentCulture,
               DateTimeStyles.AssumeUniversal);
         }
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
