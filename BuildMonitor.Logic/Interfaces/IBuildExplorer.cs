
namespace BuildMonitor.Logic.Interfaces
{
   using System.Threading.Tasks;

   using BuildMonitor.Logic.Contracts;

   /// <summary>Defines an interface to explore TFS builds.</summary>
   public interface IBuildExplorer
   {
      /// <summary>Gets the build result.</summary>
      /// <param name="buildInformation">The build information.</param>
      /// <returns>The <see cref="BuildResult"/>.</returns>
      Task<BuildResultCollection> GetBuildResultCollection(BuildInformation buildInformation);

      /// <summary>Gets the build definitions.</summary>
      /// <param name="buildServer">The build server.</param>
      /// <returns>The <see cref="TfsConnectResult"/>.</returns>
      Task<TfsConnectResult> GetBuildDefinitions(BuildServer buildServer);

      /// <summary>Stops the build.</summary>
      /// <param name="buildInformation">The build information.</param>
      /// <param name="buildId">The build identifier.</param>
      /// <returns>The <see cref="BuildResult"/>.</returns>
      Task<BuildResult> StopBuild(BuildInformation buildInformation, string buildId);

      /// <summary>Requests the build.</summary>
      /// <param name="buildInformation">The build information.</param>
      /// <returns>The <see cref="BuildResult"/>.</returns>
      Task<BuildResult> RequestBuild(BuildInformation buildInformation);

      /// <summary>
      /// Gets the test result asynchronous.
      /// </summary>
      /// <param name="buildInformation">The build information.</param>
      /// <param name="result">The result.</param>
      /// <returns>The <see cref="BuildResult"/>.</returns>
      Task GetTestResultAsync(BuildInformation buildInformation, BuildResult result);

      /// <summary>
      /// Gets the change-set asynchronous.
      /// </summary>
      /// <param name="buildInformation">The build information.</param>
      /// <param name="sourceVersion">The source version.</param>
      /// <returns>The <see cref="Changeset"/></returns>
      Task<Changeset> GetChangesetAsync(BuildInformation buildInformation, int sourceVersion);
   }
}
