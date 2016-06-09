
namespace BuildMonitor.Logic.Contracts
{
   using System.Collections.Generic;

   /// <summary>The TFS connect result.</summary>
   public class TfsConnectResult
   {
      /// <summary>Initializes a new instance of the <see cref="TfsConnectResult"/> class.</summary>
      public TfsConnectResult()
      {
         BuildDefinitions = new List<BuildDefinitionResult>();
      }

      /// <summary>Gets or sets the message.</summary>
      public string Message { get; set; }

      /// <summary>Gets the build definitions.</summary>
      public IList<BuildDefinitionResult> BuildDefinitions { get; private set; }
   }
}
