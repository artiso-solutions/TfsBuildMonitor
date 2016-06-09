namespace BuildMonitor.Logic.Contracts
{
   using System.Collections.Generic;

   public class Node
   {
      #region Public Properties

      public int errors { get; set; }

      public string flavor { get; set; }

      public string localPath { get; set; }

      public List<Node> nodes { get; set; }

      public string platform { get; set; }

      public string serverPath { get; set; }

      public string status { get; set; }

      public string targetNames { get; set; }

      public string text { get; set; }

      public int total { get; set; }

      public string type { get; set; }

      public int warnings { get; set; }

      #endregion
   }
}