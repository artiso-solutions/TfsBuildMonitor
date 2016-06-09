namespace BuildMonitor.Logic.Contracts
{
   using System;
   using System.Collections.Generic;

   /// <summary>The build server.</summary>
   [Serializable]
   public class BuildServer
   {
      #region Constructors and Destructors

      /// <summary>Initializes a new instance of the <see cref="BuildServer"/> class.</summary>
      public BuildServer()
      {
         BuildDefinitions = new List<BuildDefinition>();
      }

      #endregion

      #region Public Properties

      /// <summary>Gets or sets the build definitions.</summary>
      public List<BuildDefinition> BuildDefinitions { get; set; }

      /// <summary>Gets or sets the detail build URL.</summary>
      public string DetailBuildUrl { get; set; }

      /// <summary>Gets or sets the name of domain.</summary>
      public string DomainName { get; set; }

      /// <summary>Gets or sets the login.</summary>
      public string Login { get; set; }

      /// <summary>Gets or sets the password.</summary>
      public string PasswordBytes { get; set; }

      /// <summary>Gets or sets the name of the server.</summary>
      public string ServerName { get; set; }

      /// <summary>Gets or sets the TFS version.</summary>
      public TfsVersion TfsVersion { get; set; }

      /// <summary>Gets or sets the URL.</summary>
      public string Url { get; set; }

      #endregion
   }
}