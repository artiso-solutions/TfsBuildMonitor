using System.Collections.Generic;

namespace BuildMonitor.Logic.Contracts
{
   using System;

   /// <summary>The Build result.</summary>
   public class BuildResult
   {
      /// <summary>Initializes a new instance of the <see cref="BuildResult"/> class.</summary>
      public BuildResult()
      {
         FailingDetails=new List<string>();
      }

      #region Public Properties

      /// <summary>Gets or sets the id.</summary>
      public int Id { get; set; }

      /// <summary>Gets or sets the duration in minutes.</summary>
      public double Duration { get; set; }

      /// <summary>Gets or sets the finish time.</summary>
      public string FinishTime { get; set; }

      /// <summary>Gets or sets a value indicating whether this instance is gated check-in.</summary>
      public bool IsGatedCheckin { get; set; }

      /// <summary>Gets or sets a value indicating whether this instance is running.</summary>
      public bool IsRunning { get; set; }

      /// <summary>Gets the failing details.</summary>
      public IList<string> FailingDetails { get; private set; }

      /// <summary>Gets or sets the name.</summary>
      public string Name { get; set; }

      /// <summary>Gets or sets the number.</summary>
      public string Number { get; set; }

      /// <summary>Gets or sets the requested by.</summary>
      public string RequestedBy { get; set; }

      /// <summary>Gets or sets the running build identifier.</summary>
      public string RunningBuildId { get; set; }

      /// <summary>Gets or sets the running build number.</summary>
      public string RunningBuildNumber { get; set; }

      /// <summary>Gets or sets the running build requested by.</summary>
      public string RunningBuildRequestedBy { get; set; }

      /// <summary>Gets or sets the running build TFS URI.</summary>
      public string RunningBuildTfsUri { get; set; }

      /// <summary>Gets or sets the running start time.</summary>
      public DateTime RunningStartTime { get; set; }

      /// <summary>Gets or sets the running TFS URI.</summary>
      public string RunningTfsUri { get; set; }

      /// <summary>Gets or sets the status.</summary>
      public BuildStatus Status { get; set; }

      /// <summary>Gets or sets a value indicating whether [tests failed].</summary>
      public bool TestsFailed { get; set; }

      /// <summary>Gets or sets a value indicating whether [tests finished].</summary>
      public bool TestsFinished { get; set; }

      /// <summary>Gets or sets a value indicating whether [tests running].</summary>
      public bool TestsRunning { get; set; }

      /// <summary>Gets or sets the TFS URI.</summary>
      public string TfsUri { get; set; }

      /// <summary>Gets or sets a value indicating whether this <see cref="BuildResult"/> is waiting.</summary>
      public bool Waiting { get; set; }

      /// <summary>Gets or sets the waiting build identifier.</summary>
      public string WaitingBuildId { get; set; }

      /// <summary>Gets or sets the waiting build number.</summary>
      public string WaitingBuildNumber { get; set; }

      /// <summary>Gets or sets the waiting build requested by.</summary>
      public string WaitingBuildRequestedBy { get; set; }

      /// <summary>Gets or sets the waiting build TFS URI.</summary>
      public string WaitingBuildTfsUri { get; set; }

      /// <summary>Gets or sets the waiting TFS URI.</summary>
      public string WaitingTfsUri { get; set; }

      /// <summary>Gets or sets the total tests.</summary>
      public int TotalTests { get; set; }

      /// <summary>Gets or sets the failed tests.</summary>
      public int FailedTests { get; set; }

      /// <summary>Gets or sets the passed tests.</summary>
      public int PassedTests { get; set; }

      #endregion
   }
}