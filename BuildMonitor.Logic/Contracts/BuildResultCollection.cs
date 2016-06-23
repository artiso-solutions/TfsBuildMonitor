using System.Collections.Generic;

namespace BuildMonitor.Logic.Contracts
{
   /// <summary>
   ///    The build result collection.
   /// </summary>
   public class BuildResultCollection
   {
      /// <summary>
      ///    Initializes a new instance of the <see cref="BuildResultCollection" /> class.
      /// </summary>
      public BuildResultCollection()
      {
         BuildResults = new List<BuildResult>();
      }

      /// <summary>
      ///    Gets or sets the error message.
      /// </summary>
      public string ErrorMessage { get; set; }

      /// <summary>
      ///    Gets the build results.
      /// </summary>
      public IList<BuildResult> BuildResults { get; private set; }
   }
}