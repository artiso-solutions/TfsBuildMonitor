
namespace BuildMonitor.Logic.BuildExplorer
{
   using System.ComponentModel;

   using BuildMonitor.Logic.Contracts;
   using BuildMonitor.Logic.Interfaces;

   /// <summary>The build explorer factory.</summary>
   public static class BuildExplorerFactory
   {
      /// <summary>Creates the build explorer.</summary>
      /// <param name="tfsVersion">The TFS version.</param>
      /// <returns>The <see cref="IBuildExplorer"/>.</returns>
      /// <exception cref="InvalidEnumArgumentException">If <paramref name="tfsVersion"/> is invalid.</exception>
      public static IBuildExplorer CreateBuildExplorer(TfsVersion tfsVersion)
      {
         switch (tfsVersion)
         {
            case TfsVersion.Version2013:
               return new Tfs2013Explorer();
            case TfsVersion.Version2015:
               return new Tfs2015Explorer();
            default:
               throw new InvalidEnumArgumentException("tfsVersion", (int)tfsVersion, typeof(TfsVersion));
         }
      }
   }
}
