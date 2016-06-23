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

      private int maximum;

      private double zoomFactor;

      private bool useFullWidth;

      #endregion

      #region Constructors and Destructors

      /// <summary>Initializes a new instance of the <see cref="MainWindowViewModel"/> class.</summary>
      /// <param name="builds">The builds.</param>
      /// <param name="refreshInterval">The refresh interval.</param>
      /// <param name="bigSizeMode">if set to <c>true</c> [big size mode].</param>
      /// <param name="zoomFactor">The zoom factor.</param>
      internal MainWindowViewModel(IEnumerable<BuildInformation> builds, int refreshInterval, bool bigSizeMode, double zoomFactor, bool useFullWidth)
      {
         PinBuildViews = new List<PinBuildView>();
         BuildAdapters = new ObservableCollection<BuildAdapter>(builds.Select(build => new BuildAdapter(this, build, false)));

         ActualValue = Maximum = refreshInterval;
         this.bigSizeMode = bigSizeMode;
         this.useFullWidth = useFullWidth;
         this.zoomFactor = zoomFactor;

         RefreshCommand = new RefreshCommand(this);
         CloseCommand = new CloseCommand(null);
         AboutCommand = new AboutCommand();
         SettingsCommand = new SettingsCommand(this);
         NotificationCommand = new NotificationCommand();

         Refresh();

         var dispatcherTimer = new DispatcherTimer(DispatcherPriority.Normal) { Interval = TimeSpan.FromSeconds(1) };
         dispatcherTimer.Tick += dispatcherTimer_Tick;
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

      /// <summary>Gets the notification command.</summary>
      public ICommand NotificationCommand { get; private set; }

      /// <summary>Gets the refresh command.</summary>
      public ICommand RefreshCommand { get; private set; }

      /// <summary>Gets the settings command.</summary>
      public ICommand SettingsCommand { get; private set; }

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

      /// <summary>Gets the pin build views.</summary>
      internal IList<PinBuildView> PinBuildViews { get; private set; }

      #endregion

      #region Methods

      /// <summary>Refreshes this instance.</summary>
      internal void Refresh()
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

      private void dispatcherTimer_Tick(object sender, EventArgs e)
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

      #endregion
   }
}