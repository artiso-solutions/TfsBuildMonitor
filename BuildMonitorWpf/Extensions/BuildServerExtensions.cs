
namespace BuildMonitorWpf.Extensions
{
   using System;
   using System.Collections.Generic;
   using System.ComponentModel;
   using System.Linq;

   using BuildMonitor.Logic.Contracts;

   internal static class BuildServerExtensions
   {
      internal static IEnumerable<BuildInformation> GetBuilds(this BuildServer buildServer)
      {
         if (string.IsNullOrEmpty(buildServer.Url))
         {
            return Enumerable.Empty<BuildInformation>();
         }

         switch (buildServer.TfsVersion)
         {
            case TfsVersion.Version2013:
               return GetBuildsTfs2013(buildServer);
            case TfsVersion.Version2015:
               return GetBuildsTfs2015(buildServer);
            default:
               throw new InvalidEnumArgumentException("buildServer.TfsVersion", (int)buildServer.TfsVersion, typeof(TfsVersion));
         }
      }

      private static IEnumerable<BuildInformation> GetBuildsTfs2013(BuildServer buildServer)
      {
         var apiIndex = buildServer.Url.IndexOf("_apis", StringComparison.OrdinalIgnoreCase);
         var urlNoApi = buildServer.Url.Substring(0, apiIndex);
         var buildDefinitionUrl = string.Concat(urlNoApi, "_apis/build/Builds?api-version=1.0&definition=");
         var stopBuildUrl = string.Concat(urlNoApi, "_apis/build/builds/{0}?api-version=1.0");
         var requestBuildUrl = string.Concat(urlNoApi, "_apis/build/requests?api-version=1.0");
         var testResutlUri = string.Concat(urlNoApi, "_api/_build/build?__v=5&uri=vstfs:///Build/Build/{0}&includeTestRuns=true&includeCoverage=true");

         foreach (var buildDefinition in buildServer.BuildDefinitions)
         {
            var build = new BuildInformation
            {
               DomainName = buildServer.DomainName,
               Login = buildServer.Login,
               CryptedPassword = buildServer.PasswordBytes,
               BuildDefinitionUrl = string.Concat(buildDefinitionUrl, buildDefinition.Uri),
               BuildDetailUrl = buildServer.DetailBuildUrl,
               BuildDefinitionId = buildDefinition.Id,
               StopBuildUrl = stopBuildUrl,
               RequestBuildUrl = requestBuildUrl,
               TestResultUrl = testResutlUri,
               TfsVersion = buildServer.TfsVersion,
               Tags = buildDefinition.Tags
            };
            yield return build;
         }
      }

      private static IEnumerable<BuildInformation> GetBuildsTfs2015(BuildServer buildServer)
      {
         var apiIndex = buildServer.Url.IndexOf("_apis", StringComparison.OrdinalIgnoreCase);

         foreach (var buildDefinition in buildServer.BuildDefinitions)
         {
            var urlNoApi = apiIndex < 0 ? string.Concat(buildServer.Url, "/", buildDefinition.ProjectId, "/") : buildServer.Url.Substring(0, apiIndex);
            var buildDefinitionUrl = string.Concat(urlNoApi, "_apis/build/Builds?api-version=2.0&definitions=");
            var stopBuildUrl = string.Concat(urlNoApi, "_apis/build/builds/{0}?api-version=2.0");
            var requestBuildUrl = string.Concat(urlNoApi, "_apis/build/builds?api-version=2.0");
            var testRunUrl = string.Concat(urlNoApi, "_apis/test/Runs/Query?includeRunDetails=true&api-version=2.2-preview.2");
            var changesetUrl = string.Concat(urlNoApi, "_apis/tfvc/changesets/{0}?api-version=1.0");
            var build = new BuildInformation
            {
               DomainName = buildServer.DomainName,
               Login = buildServer.Login,
               CryptedPassword = buildServer.PasswordBytes,
               BuildDefinitionUrl = string.Concat(buildDefinitionUrl, buildDefinition.Id),
               BuildDefinitionId = buildDefinition.Id,
               StopBuildUrl = stopBuildUrl,
               RequestBuildUrl = requestBuildUrl,
               TfsVersion = buildServer.TfsVersion,
               TestRunUrl = testRunUrl,
               ChangesetUrl = changesetUrl,
               Tags = buildDefinition.Tags
            };
            yield return build;
         }
      }
   }
}
