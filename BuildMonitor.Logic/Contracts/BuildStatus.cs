
namespace BuildMonitor.Logic.Contracts
{
   public enum BuildStatus
   {
      Unknown,

      Error,

      NotStarted,

      InProgress,

      Succeeded,

      Failed,

      PartiallySucceeded,

      Stopped,

      Waiting
   }
}
