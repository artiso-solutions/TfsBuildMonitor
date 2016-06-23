namespace BuildMonitorWpf.Contracts
{
   public class TempBuildContainer
   {
      public int PassedTests { get; set; }

      public int FailedTests { get; set; }

      public int TotalTests { get; set; }
   }
}