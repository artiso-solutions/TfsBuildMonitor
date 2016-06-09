namespace BuildMonitor.Logic.Contracts
{
   using System;

   /// <summary>The build definition.</summary>
   [Serializable]
   public class BuildDefinition
   {
      #region Public Properties

      /// <summary>Gets or sets the identifier.</summary>
      public int Id { get; set; }

      /// <summary>Gets or sets a value indicating whether this instance is pined.</summary>
      public bool IsPined { get; set; }

      /// <summary>Gets or sets the name.</summary>
      public string Name { get; set; }

      /// <summary>Gets or sets the pin left.</summary>
      public int PinLeft { get; set; }

      /// <summary>Gets or sets the pin top.</summary>
      public int PinTop { get; set; }

      /// <summary>Gets or sets the project identifier.</summary>
      public string ProjectId { get; set; }

      /// <summary>Gets or sets the URI.</summary>
      public string Uri { get; set; }

      /// <summary>Gets or sets the URL.</summary>
      public string Url { get; set; }

      #endregion
   }
}