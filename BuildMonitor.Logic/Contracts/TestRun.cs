
namespace BuildMonitor.Logic.Contracts
{
   public class TestRun
   {
      public int Id { get; set; }

      public int TotalTests { get; set; }

      public int PassedTests { get; set; }

      public int FailedTests { get; set; }
   }
}