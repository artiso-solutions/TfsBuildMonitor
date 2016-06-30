using BuildMonitorWpf.View;

namespace BuildMonitorWpf.ViewModel
{
   using System;
   using System.Collections.Generic;
   using System.Collections.ObjectModel;
   using System.ComponentModel;
   using System.Linq;
   using System.Runtime.CompilerServices;
   using System.Windows;
   using System.Windows.Input;
   using System.Windows.Threading;

   using BuildMonitor.Logic.Contracts;

   using BuildMonitorWpf.Adapter;
   using BuildMonitorWpf.Commands;
   using BuildMonitorWpf.Properties;

   using Microsoft.WindowsAPICodePack.Taskbar;

   /// <summary>The main window view model.</summary>
   /// <seealso cref="System.ComponentModel.INotifyPropertyChanged"/>
   public class MainWindowViewModel : INotifyPropertyChanged
   {
      #region Constants and Fields

      private const double TOLERANCE = 0.01;

      private int actualValue;

      private bool bigSizeMode;

      private bool isRibbonMinimized;

      private int maximum;

      private double zoomFactor;

      private bool useFullWidth;

      private int selectedRefreshInterval;

      private int selectedZoomFactor;

      #endregion

      #region Constructors and Destructors

      /// <summary>
      /// Initializes a new instance of the <see cref="MainWindowViewModel" /> class.
      /// </summary>
      /// <param name="builds">The builds.</param>
      /// <param name="refreshInterval">The refresh interval.</param>
      /// <param name="bigSizeMode">if set to <c>true</c> [big size mode].</param>
      /// <param name="zoomFactor">The zoom factor.</param>
      /// <param name="useFullWidth">if set to <c>true</c> [use full width].</param>
      /// <param name="isRibbonMinimized">if set to <c>true</c> [is ribbon minimized].</param>
      internal MainWindowViewModel(IEnumerable<BuildInformation> builds, int refreshInterval, bool bigSizeMode, double zoomFactor, bool useFullWidth, bool isRibbonMinimized)
      {
         selectedRefreshInterval = refreshInterval;
         selectedZoomFactor = (int)(zoomFactor * 100);
         PinBuildViews = new List<PinBuildView>();
         BuildAdapters = new ObservableCollection<BuildAdapter>(builds.Select(build => new BuildAdapter(this, build, false)));

         ActualValue = Maximum = refreshInterval;
         this.bigSizeMode = bigSizeMode;
         this.useFullWidth = useFullWidth;
         this.zoomFactor = zoomFactor;
         this.isRibbonMinimized = isRibbonMinimized;

         RefreshCommand = new RelayCommand(Refresh);
         SetRefreshIntervalCommand = new RelayCommand(SetRefreshInterval);
         SetZoomFactorCommand = new RelayCommand(SetZoomFactor);
         CloseCommand = new CloseCommand(null);
         AboutCommand = new RelayCommand(About);
         SettingsCommand = new SettingsCommand(this);
         NotificationCommand = new NotificationCommand();
         MinimizeRibbonCommand = new RelayCommand(o => IsRibbonMinimized = !IsRibbonMinimized);

         var intervals = new List<int>();
         for (var i = 15; i <= 150; i += 15)
         {
            intervals.Add(i);
         }

         RefreshIntervals = new ObservableCollection<int>(intervals);

         var factors = new List<int>();
         for (var i = 100; i <= 300; i += 25)
         {
            factors.Add(i);
         }

         ZoomFactors = new ObservableCollection<int>(factors);

         Refresh();

         var dispatcherTimer = new DispatcherTimer(DispatcherPriority.Normal) { Interval = TimeSpan.FromSeconds(1) };
         dispatcherTimer.Tick += DispatcherTimerTick;
         dispatcherTimer.Start();

         if (!Settings.Default.BuildServers.BuildServers.Any())
         {
            SettingsCommand.Execute(null);
         }
      }

      #endregion

      #region Public Events

      /// <summary>Occurs when [refreshing].</summary>
      public event EventHandler<EventArgs> Refreshing;

      #endregion

      #region INotifyPropertyChanged Members

      /// <summary>Occurs when a property value changes.</summary>
      public event PropertyChangedEventHandler PropertyChanged;

      #endregion

      #region Public Properties

      /// <summary>Gets the about command.</summary>
      public ICommand AboutCommand { get; private set; }

      /// <summary>Gets or sets the actual value.</summary>
      public int ActualValue
      {
         get
         {
            return actualValue;
         }

         set
         {
            if (actualValue == value)
            {
               return;
            }

            actualValue = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets or sets a value indicating whether [big size mode].</summary>
      public bool BigSizeMode
      {
         get
         {
            return bigSizeMode;
         }

         set
         {
            if (bigSizeMode == value)
            {
               return;
            }

            bigSizeMode = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets or sets a value indicating whether [big size mode].</summary>
      public bool UseFullWidth
      {
         get
         {
            return useFullWidth;
         }

         set
         {
            if (useFullWidth == value)
            {
               return;
            }

            useFullWidth = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets the build adapters.</summary>
      public ObservableCollection<BuildAdapter> BuildAdapters { get; private set; }

      /// <summary>Gets the close command.</summary>
      public ICommand CloseCommand { get; private set; }

      /// <summary>Gets or sets the column widths.</summary>
      public string ColumnWidths
      {
         get
         {
            return Settings.Default.ColumnWidths;
         }

         set
         {
            if (Settings.Default.ColumnWidths == value)
            {
               return;
            }

            Settings.Default.ColumnWidths = value;
            Settings.Default.Save();
            OnPropertyChanged();
         }
      }

      /// <summary>Gets the debug visibility.</summary>
      public Visibility DebugVisibility
      {
         get
         {
#if DEBUG
            return Visibility.Visible;
#endif
            return Visibility.Collapsed;
         }
      }

      /// <summary>Gets or sets a value indicating whether the ribbon is minimized.</summary>
      public bool IsRibbonMinimized
      {
         get
         {
            return isRibbonMinimized;
         }

         set
         {
            if (isRibbonMinimized == value)
            {
               return;
            }

            isRibbonMinimized = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets the maximum width.</summary>
      public double MaxWidth
      {
         get
         {
            return SystemParameters.FullPrimaryScreenWidth;
         }
      }

      /// <summary>Gets or sets the maximum.</summary>
      public int Maximum
      {
         get
         {
            return maximum;
         }

         set
         {
            if (value == maximum)
            {
               return;
            }

            maximum = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets the minimize ribbon command.</summary>
      public ICommand MinimizeRibbonCommand { get; private set; }

      /// <summary>Gets the notification command.</summary>
      public ICommand NotificationCommand { get; private set; }

      /// <summary>Gets the refresh command.</summary>
      public ICommand RefreshCommand { get; private set; }

      /// <summary>Gets the refresh intervals.</summary>
      public ObservableCollection<int> RefreshIntervals { get; private set; }

      /// <summary>Gets or sets the selected refresh interval.</summary>
      public int SelectedRefreshInterval
      {
         get
         {
            return selectedRefreshInterval;
         }
         set
         {
            if (selectedRefreshInterval == value)
            {
               return;
            }

            selectedRefreshInterval = value;
            OnPropertyChanged();
            OnSelectedRefreshIntervalChanged();
         }
      }

      /// <summary>Gets or sets the selected zoom factor.</summary>
      public int SelectedZoomFactor
      {
         get
         {
            return selectedZoomFactor;
         }

         set
         {
            if (selectedZoomFactor == value)
            {
               return;
            }

            selectedZoomFactor = value;
            OnPropertyChanged();
            ZoomFactor = value / 100.0;
         }
      }

      /// <summary>Gets the set refresh interval command.</summary>
      public ICommand SetRefreshIntervalCommand { get; private set; }

      /// <summary>Gets the settings command.</summary>
      public ICommand SettingsCommand { get; private set; }

      /// <summary>Gets the set zoom factor command.</summary>
      public ICommand SetZoomFactorCommand { get; private set; }

      /// <summary>Gets or sets the zoom factor.</summary>
      public double ZoomFactor
      {
         get
         {
            return zoomFactor;
         }

         set
         {
            if (Math.Abs(zoomFactor - value) < TOLERANCE)
            {
               return;
            }

            zoomFactor = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets the zoom factors.</summary>
      public ObservableCollection<int> ZoomFactors { get; private set; }

      /// <summary>Gets the pin build views.</summary>
      internal IList<PinBuildView> PinBuildViews { get; private set; }

      #endregion

      #region Methods

      /// <summary>Refreshes this instance.</summary>
      internal void Refresh(object parameter = null)
      {
         ActualValue = Maximum;

         var handler = Refreshing;
         if (handler != null)
         {
            handler(this, EventArgs.Empty);
         }

         foreach (var adapter in BuildAdapters)
         {
            adapter.Refresh();
         }
      }

      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
      {
         var handler = PropertyChanged;
         if (handler != null)
            handler(this, new PropertyChangedEventArgs(propertyName));
      }

      private void About(object obj)
      {
         var aboutDialog = new AboutView { Owner = Application.Current.MainWindow };
         var aboutViewModel = new AboutViewModel(aboutDialog);
         aboutViewModel.FillReleaseNotes();
         aboutDialog.DataContext = aboutViewModel;
         aboutDialog.ShowDialog();
      }

      private void ChangeWindowsBarColor()
      {
         var progressBarState = TaskbarProgressBarState.Normal;
         var currentValue = 100;
         if (BuildAdapters.Where(b => !b.IgnoreStatus).Any(b => b.Status == BuildStatus.PartiallySucceeded))
         {
            progressBarState = TaskbarProgressBarState.Paused;
         }

         if (BuildAdapters.Where(b => !b.IgnoreStatus).Any(b => b.Status == BuildStatus.Failed))
         {
            progressBarState = TaskbarProgressBarState.Error;
         }

         if (BuildAdapters.Where(b => !b.IgnoreStatus).All(b => b.Status == BuildStatus.Unknown || b.Status == BuildStatus.Error))
         {
            currentValue = 0;
         }

         var progress = TaskbarManager.Instance;
         progress.SetProgressState(progressBarState);
         progress.SetProgressValue(currentValue, 100);
      }

      private void DispatcherTimerTick(object sender, EventArgs e)
      {
         if (BuildAdapters.All(build => build.Status != BuildStatus.Waiting))
         {
            ChangeWindowsBarColor();
         }

         ActualValue--;
         if (ActualValue > 0)
         {
            return;
         }

         Refresh();
      }

      private void OnSelectedRefreshIntervalChanged()
      {
         Maximum = SelectedRefreshInterval;
         if (ActualValue > SelectedRefreshInterval)
         {
            ActualValue = SelectedRefreshInterval;
         }
      }

      private void SetZoomFactor(object parameter)
      {
         if (parameter is int)
         {
            SelectedZoomFactor = (int)parameter;
         }
      }

      private void SetRefreshInterval(object parameter)
      {
         if (parameter is int)
         {
            SelectedRefreshInterval = (int)parameter;
         }
      }

      #endregion
   }
}