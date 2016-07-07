using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;

namespace BuildMonitorWpf.Adapter
{
   using System;
   using System.ComponentModel;
   using System.Diagnostics;
   using System.Linq;
   using System.Runtime.CompilerServices;
   using System.Windows.Input;
   using System.Windows.Media;

   using BuildMonitor.Logic.BuildExplorer;
   using BuildMonitor.Logic.Contracts;
   using BuildMonitor.Logic.Interfaces;

   using BuildMonitorWpf.Commands;
   using BuildMonitorWpf.Contracts;
   using BuildMonitorWpf.Properties;
   using BuildMonitorWpf.ViewModel;

   /// <summary>The build adapter.</summary>
   public class BuildAdapter : INotifyPropertyChanged
   {
      #region Constants and Fields

      private readonly Brush blueBrush = new SolidColorBrush(Color.FromRgb(27, 161, 226));

      private readonly IBuildExplorer buildExplorer;

      private readonly bool isPinedView;

      private readonly Brush redBrush = new SolidColorBrush(Color.FromRgb(229, 20, 0));

      private int averageMinutes;

      private bool errorsInBuild;

      private string finishDateTime;

      private bool isGatedCheckin;

      private bool isRunning;

      private string name;

      private string number;

      private string requestedBy;

      private string runningBuildId;

      private string runningBuildImagePath;

      private string runningBuildImageToolTip;

      private string runningBuildNumber;

      private Brush runningBuildProgressBarColor;

      private string runningBuildRequestedBy;

      private int runningMinutes;

      private int runningProgress;

      private string runningTfsUri;

      private BuildStatus status;

      private string tfsUri;

      private int passedTests;

      private int failedTests;

      private int totalTests;

      private readonly TempBuildContainer tempBuildContainer = new TempBuildContainer();

      private Changeset changeset;

      private Changeset runningBuildChangeset;

      #endregion

      #region Constructors and Destructors

      /// <summary>Initializes a new instance of the <see cref="BuildAdapter"/> class.</summary>
      /// <param name="mainWindowViewModel">The main window view model.</param>
      /// <param name="buildInformation">The build information.</param>
      /// <param name="isPinedView">if set to <c>true</c> [is pined view].</param>
      internal BuildAdapter(MainWindowViewModel mainWindowViewModel, BuildInformation buildInformation, bool isPinedView)
      {
         BuildInformation = buildInformation;
         this.isPinedView = isPinedView;
         buildExplorer = BuildExplorerFactory.CreateBuildExplorer(buildInformation.TfsVersion);

         OpenBrowserCommand = new RelayCommand(OpenBrowser);
         StopBuildCommand = new StopBuildCommand(this, buildInformation);
         RequestBuildCommand = new RequestBuildCommand(this, buildInformation);
         RunningBuildErrorDetails = new ObservableCollection<string>();
         Tags = new ObservableCollection<string>(buildInformation.Tags ?? new string[0]);

         if (!isPinedView)
         {
            PinBuildCommand = new PinBuildCommand(mainWindowViewModel, buildInformation);
         }

         var buildDefinition = Settings.Default.BuildServers.BuildServers.SelectMany(x => x.BuildDefinitions).FirstOrDefault(x => x.Id == buildInformation.BuildDefinitionId);
         if (PinBuildCommand != null && buildDefinition != null && buildDefinition.IsPined)
         {
            PinBuildCommand.Execute(null);
         }
      }

      private void OpenBrowser(object parameter)
      {
         var uri = string.Format("{0}", parameter);
         if (!string.IsNullOrEmpty(uri))
         {
            Process.Start(uri);
         }
      }

      #endregion

      #region INotifyPropertyChanged Members

      /// <summary>Occurs when a property value changes.</summary>
      public event PropertyChangedEventHandler PropertyChanged;

      #endregion

      #region Public Properties

      /// <summary>Gets or sets the average minutes.</summary>
      public int AverageMinutes
      {
         get
         {
            return averageMinutes;
         }

         set
         {
            if (averageMinutes == value)
            {
               return;
            }

            averageMinutes = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets or sets a value indicating whether [errors in build].</summary>
      public bool ErrorsInBuild
      {
         get
         {
            return errorsInBuild;
         }

         set
         {
            if (errorsInBuild == value)
            {
               return;
            }

            errorsInBuild = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets or sets the finish date time.</summary>
      public string FinishDateTime
      {
         get
         {
            return finishDateTime;
         }

         set
         {
            if (finishDateTime == value)
            {
               return;
            }

            finishDateTime = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets or sets a value indicating whether [ignore status].</summary>
      public bool IgnoreStatus { get; set; }

      /// <summary>Gets or sets a value indicating whether this instance is gated check-in.</summary>
      public bool IsGatedCheckin
      {
         get
         {
            return isGatedCheckin;
         }

         set
         {
            if (isGatedCheckin == value)
            {
               return;
            }

            isGatedCheckin = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets or sets a value indicating whether this instance is running.</summary>
      public bool IsRunning
      {
         get
         {
            return isRunning;
         }

         set
         {
            if (isRunning == value)
            {
               return;
            }

            isRunning = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets or sets the name.</summary>
      public string Name
      {
         get
         {
            return name;
         }

         set
         {
            if (name == value)
            {
               return;
            }

            name = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets or sets the number.</summary>
      public string Number
      {
         get
         {
            return number;
         }

         set
         {
            if (number == value)
            {
               return;
            }

            number = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets the open browser command.</summary>
      public ICommand OpenBrowserCommand { get; private set; }

      /// <summary>Gets the pin build command.</summary>
      public ICommand PinBuildCommand { get; private set; }

      /// <summary>Gets or sets the previous build number.</summary>
      public string PreviousBuildNumber { get; set; }

      /// <summary>Gets or sets the previous running build number.</summary>
      public string PreviousRunningBuildNumber { get; set; }

      /// <summary>Gets the request build command.</summary>
      public ICommand RequestBuildCommand { get; private set; }

      /// <summary>Gets or sets the requested by.</summary>
      public string RequestedBy
      {
         get
         {
            return requestedBy;
         }

         set
         {
            if (requestedBy == value)
            {
               return;
            }

            requestedBy = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets the running build error details.</summary>
      public ObservableCollection<string> RunningBuildErrorDetails { get; private set; }

      /// <summary>Gets or sets the running build identifier.</summary>
      public string RunningBuildId
      {
         get
         {
            return runningBuildId;
         }

         set
         {
            if (runningBuildId == value)
            {
               return;
            }

            runningBuildId = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets or sets the running build image path.</summary>
      public string RunningBuildImagePath
      {
         get
         {
            return runningBuildImagePath;
         }

         set
         {
            if (runningBuildImagePath == value)
            {
               return;
            }

            runningBuildImagePath = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets or sets the running build image tool tip.</summary>
      public string RunningBuildImageToolTip
      {
         get
         {
            return runningBuildImageToolTip;
         }

         set
         {
            if (runningBuildImageToolTip == value)
            {
               return;
            }

            runningBuildImageToolTip = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets or sets the running build number.</summary>
      public string RunningBuildNumber
      {
         get
         {
            return runningBuildNumber;
         }

         set
         {
            if (runningBuildNumber == value)
            {
               return;
            }

            runningBuildNumber = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets or sets the color of the running build progress bar.</summary>
      public Brush RunningBuildProgressBarColor
      {
         get
         {
            return runningBuildProgressBarColor;
         }

         set
         {
            if (runningBuildProgressBarColor == value)
            {
               return;
            }

            runningBuildProgressBarColor = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets or sets the running build requested by.</summary>
      public string RunningBuildRequestedBy
      {
         get
         {
            return runningBuildRequestedBy;
         }

         set
         {
            if (runningBuildRequestedBy == value)
            {
               return;
            }

            runningBuildRequestedBy = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets or sets the running minutes.</summary>
      public int RunningMinutes
      {
         get
         {
            return runningMinutes;
         }

         set
         {
            if (runningMinutes == value)
            {
               return;
            }

            runningMinutes = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets or sets the running progress.</summary>
      public int RunningProgress
      {
         get
         {
            return runningProgress;
         }

         set
         {
            if (runningProgress == value)
            {
               return;
            }

            runningProgress = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets or sets the running TFS URI.</summary>
      public string RunningTfsUri
      {
         get
         {
            return runningTfsUri;
         }

         set
         {
            if (value == runningTfsUri)
            {
               return;
            }

            runningTfsUri = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets or sets the status.</summary>
      public BuildStatus Status
      {
         get
         {
            return status;
         }

         set
         {
            if (status == value)
            {
               return;
            }

            status = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets the stop build command.</summary>
      public ICommand StopBuildCommand { get; private set; }

      /// <summary>Gets the tags.</summary>
      public ObservableCollection<string> Tags { get; private set; }

      /// <summary>Gets or sets the TFS URI.</summary>
      public string TfsUri
      {
         get
         {
            return tfsUri;
         }

         set
         {
            if (tfsUri == value)
            {
               return;
            }

            tfsUri = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets or sets the passed tests.</summary>
      public int PassedTests
      {
         get
         {
            return passedTests;
         }

         set
         {
            if (passedTests == value)
            {
               return;
            }

            passedTests = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets or sets the failed tests.</summary>
      public int FailedTests
      {
         get
         {
            return failedTests;
         }

         set
         {
            if (failedTests == value)
            {
               return;
            }

            failedTests = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets or sets the total tests.</summary>
      public int TotalTests
      {
         get
         {
            return totalTests;
         }

         set
         {
            if (totalTests == value)
            {
               return;
            }

            totalTests = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets or sets the changeset.</summary>
      public Changeset Changeset
      {
         get
         {
            return changeset;
         }

         set
         {
            if (changeset == value)
            {
               return;
            }

            changeset = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets or sets the changeset.</summary>
      public Changeset RunningBuildChangeset
      {
         get
         {
            return runningBuildChangeset;
         }

         set
         {
            if (runningBuildChangeset == value)
            {
               return;
            }

            runningBuildChangeset = value;
            OnPropertyChanged();
         }
      }

      #endregion

      #region Properties

      /// <summary>Gets the build information.</summary>
      internal BuildInformation BuildInformation { get; private set; }

      #endregion

      #region Methods

      /// <summary>Refreshes this instance.</summary>
      internal async void Refresh()
      {
         PreviousBuildNumber = Number;
         PreviousRunningBuildNumber = RunningBuildNumber;
         Status = BuildStatus.Waiting;

         var resultCollection = await buildExplorer.GetBuildResultCollection(BuildInformation);
         if (!string.IsNullOrEmpty(resultCollection.ErrorMessage))
         {
            Name = resultCollection.ErrorMessage;
            Number = string.Empty;
            RequestedBy = string.Empty;
            FinishDateTime = string.Empty;
            Status = BuildStatus.Unknown;
            IsRunning = false;
            TfsUri = string.Empty;
            RunningTfsUri = string.Empty;
            return;
         }

         var firstNotWaiting = resultCollection.BuildResults.FirstOrDefault(x => !x.Waiting);
         if (firstNotWaiting == null)
         {
            return;
         }

         var firstFinished =
            resultCollection.BuildResults.FirstOrDefault(
               x => x.Status != BuildStatus.InProgress && x.Status != BuildStatus.Waiting);

         if (!isPinedView && !string.IsNullOrEmpty(PreviousBuildNumber) && !string.Equals(PreviousBuildNumber, firstNotWaiting.Number)
             && (firstNotWaiting.Status == BuildStatus.PartiallySucceeded || firstNotWaiting.Status == BuildStatus.Failed))
         {
            ToastNotifications.CreateToastNotification(firstNotWaiting, false, ToastActivated);
         }

         var doNotGetTests = !Settings.Default.UseFullWidth && Settings.Default.BigSize;
         var buildChanged = string.IsNullOrEmpty(PreviousBuildNumber) || !string.Equals(PreviousBuildNumber, firstNotWaiting.Number);
         if (buildChanged
            && (firstNotWaiting.Status == BuildStatus.PartiallySucceeded || firstNotWaiting.Status == BuildStatus.Succeeded) && !doNotGetTests)
         {
            await buildExplorer.GetTestResultAsync(BuildInformation, firstNotWaiting);
            tempBuildContainer.PassedTests = firstNotWaiting.PassedTests;
            tempBuildContainer.FailedTests = firstNotWaiting.FailedTests;
            tempBuildContainer.TotalTests = firstNotWaiting.TotalTests;
         }

         if (buildChanged)
         {
            Changeset = await buildExplorer.GetChangesetAsync(BuildInformation, firstFinished.SourceVersion);
         }

         Name = firstNotWaiting.Name;
         Number = GetValue(firstNotWaiting.IsRunning, x => x.Number, firstNotWaiting, firstFinished);
         RequestedBy = GetValue(firstNotWaiting.IsRunning, x => x.RequestedBy, firstNotWaiting, firstFinished);
         FinishDateTime = GetValue(firstNotWaiting.IsRunning, x => x.FinishTime, firstNotWaiting, firstFinished);
         Status = GetValue(firstNotWaiting.IsRunning, x => x.Status, firstNotWaiting, firstFinished);
         IsRunning = firstNotWaiting.IsRunning;
         IsGatedCheckin = firstNotWaiting.IsGatedCheckin;
         TfsUri = InsertDetailUrl(GetValue(firstNotWaiting.IsRunning, x => x.TfsUri, firstNotWaiting, firstFinished));
         RunningTfsUri = InsertDetailUrl(firstNotWaiting.RunningTfsUri);
         PassedTests = tempBuildContainer.PassedTests;
         FailedTests = tempBuildContainer.FailedTests;
         TotalTests = tempBuildContainer.TotalTests;
         RunningBuildErrorDetails.Clear();
         if (!firstNotWaiting.IsRunning)
         {
            return;
         }

         var runningBuildChanged = string.IsNullOrEmpty(PreviousRunningBuildNumber) || !string.Equals(PreviousRunningBuildNumber, firstNotWaiting.RunningBuildNumber);
         if (runningBuildChanged && !firstNotWaiting.IsGatedCheckin)
         {
            RunningBuildChangeset = await buildExplorer.GetChangesetAsync(BuildInformation, firstNotWaiting.RunningBuildSourceVersion);
         }

         var span = DateTime.Now - firstNotWaiting.RunningStartTime;
         RunningMinutes = firstNotWaiting.Waiting ? 0 : (int)span.TotalMinutes;
         RunningBuildRequestedBy = firstNotWaiting.RunningBuildRequestedBy;
         RunningBuildNumber = firstNotWaiting.RunningBuildNumber;
         RunningBuildId = firstNotWaiting.RunningBuildId;
         ErrorsInBuild = firstNotWaiting.TestsFailed;
         HandleRunnningBuildDisplayInformations(firstNotWaiting);
         foreach (var error in firstNotWaiting.FailingDetails.Take(5))
         {
            RunningBuildErrorDetails.Add(error);
         }

         if (RunningBuildErrorDetails.Count > 5)
         {
            RunningBuildErrorDetails.Add("(...)");
         }

         var buildTimes =
            resultCollection.BuildResults.Where(
               x => x.Status == BuildStatus.PartiallySucceeded || x.Status == BuildStatus.Succeeded)
               .Select(x => x.Duration).ToList();
         if (!buildTimes.Any())
         {
            return;
         }

         var average = buildTimes.Average();
         AverageMinutes = (int)average;
         var progress = (int)(span.TotalMinutes * 100 / average);
         RunningProgress = progress > 100 ? 100 : progress;
      }

      private static T2 GetValue<T1, T2>(bool isRunning, Expression<Func<T1, T2>> property, T1 firstNotWaiting, T1 firstFinished)
      {
         var propertyInfo = (PropertyInfo)((MemberExpression)property.Body).Member;
         var getMethod = propertyInfo.GetGetMethod();

         if (!isRunning)
         {
            return (T2)getMethod.Invoke(firstNotWaiting, new object[0]);
         }

         if (firstFinished == null)
         {
            return default(T2);
         }

         return (T2)getMethod.Invoke(firstFinished, new object[0]);
      }

      protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
      {
         var hander = PropertyChanged;
         if (hander != null)
         {
            hander(this, new PropertyChangedEventArgs(propertyName));
         }
      }

      private void HandleRunnningBuildDisplayInformations(BuildResult result)
      {
         RunningBuildProgressBarColor = blueBrush;
         if (result.Waiting)
         {
            RunningBuildImagePath = @"pack://application:,,,/Images/wait.png";
            RunningBuildImageToolTip = "Waiting for build agent";
         }
         else if (result.TestsFailed)
         {
            RunningBuildImagePath = @"pack://application:,,,/Images/test_failed.png";
            RunningBuildImageToolTip = "Unit-test failed";
            RunningBuildProgressBarColor = redBrush;
            if (!isPinedView && !string.Equals(PreviousRunningBuildNumber, result.RunningBuildNumber))
            {
               ToastNotifications.CreateToastNotification(result, true, ToastActivated);
            }
         }
         else if (result.TestsRunning)
         {
            RunningBuildImagePath = @"pack://application:,,,/Images/test-waiting.png";
            RunningBuildImageToolTip = "Tests are running";
         }
         else if (result.TestsFinished)
         {
            RunningBuildImagePath = @"pack://application:,,,/Images/finishDisc.png";
            RunningBuildImageToolTip = "Final steps (WIX setup)";
         }
         else
         {
            RunningBuildImagePath = @"pack://application:,,,/Images/building.png";
            RunningBuildImageToolTip = "Building DLLs";
         }
      }

      private string InsertDetailUrl(string url)
      {
         if (string.IsNullOrEmpty(url))
         {
            return string.Empty;
         }

         return url.StartsWith("http") ? url : string.Concat(BuildInformation.BuildDefinitionUrl, url);
      }

      private void ToastActivated(object sender, EventArgs args)
      {
         var uri = IsRunning ? RunningTfsUri : TfsUri;
         OpenBrowserCommand.Execute(uri);
      }

      #endregion
   }
}