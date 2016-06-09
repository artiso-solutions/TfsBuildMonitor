namespace BuildMonitor.Logic.Contracts
{
   using System;
   using System.Collections.Generic;

   public class RootObject
   {
      #region Public Properties

      public string controller { get; set; }

      public List<object> customSummarySections { get; set; }

      public string definition { get; set; }

      public string definitionUri { get; set; }

      public bool finished { get; set; }

      public DateTime finishTime { get; set; }

      public bool hasDiagnostics { get; set; }

      public List<Information> information { get; set; }

      public string lastChangedBy { get; set; }

      public DateTime lastChangedOn { get; set; }

      public string name { get; set; }

      public string project { get; set; }

      public object quality { get; set; }

      public int reason { get; set; }

      public string requestedFor { get; set; }

      public List<int> requests { get; set; }

      public bool retain { get; set; }

      public DateTime startTime { get; set; }

      public int status { get; set; }

      public string uri { get; set; }

      #endregion
   }
}