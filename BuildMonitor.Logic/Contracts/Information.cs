namespace BuildMonitor.Logic.Contracts
{
   using System.Collections.Generic;

   public class Information
   {
      #region Public Properties

      public List<Node> nodes { get; set; }

      public string status { get; set; }

      public string text { get; set; }

      public string type { get; set; }

      #endregion
   }
}