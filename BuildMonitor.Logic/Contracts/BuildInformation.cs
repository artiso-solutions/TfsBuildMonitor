namespace BuildMonitor.Logic.Contracts
{
   /// <summary>The build information.</summary>
   public class BuildInformation
   {
      #region Public Properties

      /// <summary>Gets or sets the build definition identifier.</summary>
      public int BuildDefinitionId { get; set; }

      /// <summary>Gets or sets the build definition URL.</summary>
      public string BuildDefinitionUrl { get; set; }

      /// <summary>Gets or sets the build detail URL.</summary>
      public string BuildDetailUrl { get; set; }

      /// <summary>Gets or sets the credential password.</summary>
      public string CryptedPassword { get; set; }

      /// <summary>Gets or sets the name of the credential domain.</summary>
      public string DomainName { get; set; }

      /// <summary>Gets or sets the credential login.</summary>
      public string Login { get; set; }

      /// <summary>Gets or sets the request build URL.</summary>
      public string RequestBuildUrl { get; set; }

      /// <summary>Gets or sets the stop build URL.</summary>
      public string StopBuildUrl { get; set; }

      /// <summary>Gets or sets the test result URL.</summary>
      public string TestResultUrl { get; set; }

      /// <summary>Gets or sets the version of the TFS this build belongs to. This is used for correctly parsing the results depending on the API version.</summary>
      public TfsVersion TfsVersion { get; set; }

      public string TestRunUrl { get; set; }

      #endregion
   }
}