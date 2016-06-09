
namespace BuildMonitorWpf.Contracts
{
   using System;
   using System.Collections.Generic;

   using BuildMonitor.Logic.Contracts;

   [Serializable]
   public class BuildServerCollection
   {
      /// <summary>
      /// Initializes a new instance of the <see cref="BuildServerCollection"/> class.
      /// </summary>
      public BuildServerCollection()
      {
         BuildServers = new List<BuildServer>();
      }

      /// <summary>
      /// Gets or sets the build servers.
      /// </summary>
      public List<BuildServer> BuildServers { get; set; }
   }
}
