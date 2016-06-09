namespace BuildMonitor.Logic.Contracts
{
   /// <summary>The build definition result.</summary>
   public class BuildDefinitionResult
   {
      #region Public Properties

      /// <summary>Gets or sets the identifier.</summary>
      public int Id { get; set; }

      /// <summary>Gets or sets the name.</summary>
      public string Name { get; set; }

      /// <summary>Gets or sets the project identifier.</summary>
      public string ProjectId { get; set; }

      /// <summary>Gets or sets the name of the project.</summary>
      public string ProjectName { get; set; }

      /// <summary>Gets or sets a value indicating whether this <see cref="BuildDefinitionResult"/> is selected.</summary>
      public bool Selected { get; set; }

      /// <summary>Gets or sets the URI.</summary>
      public string Uri { get; set; }

      /// <summary>Gets or sets the URL.</summary>
      public string Url { get; set; }

      #endregion
   }
}