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
   using System.Windows.Data;
   using System.Windows.Input;
   using System.Windows.Threading;

   using BuildMonitor.Logic.Contracts;

   using BuildMonitorWpf.Adapter;
   using BuildMonitorWpf.Commands;
   using BuildMonitorWpf.Contracts;
   using BuildMonitorWpf.Properties;

   using Microsoft.WindowsAPICodePack.Taskbar;

   /// <summary>The main window view model.</summary>
   /// <seealso cref="System.ComponentModel.INotifyPropertyChanged"/>
   public class MainWindowViewModel : INotifyPropertyChanged
   {
      #region Constants and Fields

      private const double Tolerance = 0.01;

      private int actualValue;

      private bool bigSizeMode;

      private bool isRibbonMinimized;

      private int maximum;

      private double zoomFactor;

      private bool useFullWidth;

      private string userTag;

      private FilterTag selectedExistingTag;

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

         AvailableTags = new ObservableCollection<FilterTag>(new[] { new FilterTag { IsAllFilter = true, IsSelected = true, Label = Resources.AllFilterLabel } });
         FillAvailableTags();

         CollectionViewSourceBuildAdapters = new CollectionViewSource { Source = BuildAdapters };
         CollectionViewSourceBuildAdapters.Filter += CollectionViewSourceBuildAdaptersFilter;
         SelectedBuildAdapters = new ObservableCollection<BuildAdapter>();

         ActualValue = Maximum = refreshInterval;
         this.bigSizeMode = bigSizeMode;
         this.useFullWidth = useFullWidth;
         this.zoomFactor = zoomFactor;
         this.isRibbonMinimized = isRibbonMinimized;

         ApplyExistingTagToBuildCommand = new ApplyExistingTagToBuildCommand(this);
         ApplyNewTagToBuildCommand = new ApplyNewTagToBuildCommand(this);
         RemoveTagFromBuildCommand = new RemoveTagFromBuildCommand(this);
         RefreshCommand = new RelayCommand(Refresh);
         SetRefreshIntervalCommand = new RelayCommand(SetRefreshInterval);
         SetZoomFactorCommand = new RelayCommand(SetZoomFactor);
         CloseCommand = new CloseCommand(null);
         AboutCommand = new RelayCommand(About);
         SettingsCommand = new SettingsCommand(this);
         NotificationCommand = new NotificationCommand();
         MinimizeRibbonCommand = new RelayCommand(o => IsRibbonMinimized = !IsRibbonMinimized);
         DoFilterCommand = new RelayCommand(DoFilter);

         var intervals = new List<int>();
         for (var i = 15; i <= 300; i += 15)
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

      /// <summary>Gets the apply existing tag to build command.</summary>
      public ICommand ApplyExistingTagToBuildCommand { get; private set; }

      /// <summary>Gets the apply new tag to build command.</summary>
      public ICommand ApplyNewTagToBuildCommand { get; private set; }

      /// <summary>Gets the available tags.</summary>
      public ObservableCollection<FilterTag> AvailableTags { get; private set; }

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

      /// <summary>Gets or sets the user tag.</summary>
      public string UserTag
      {
         get
         {
            return userTag;
         }

         set
         {
            if (userTag == value)
            {
               return;
            }

            userTag = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets the build adapters.</summary>
      internal ObservableCollection<BuildAdapter> BuildAdapters { get; private set; }

      /// <summary>Gets the collection view source build definitions.</summary>
      internal CollectionViewSource CollectionViewSourceBuildAdapters { get; private set; }

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

      /// <summary>Gets the do filter command.</summary>
      public ICommand DoFilterCommand { get; private set; }

      /// <summary>Gets the filtered build adapters.</summary>
      public ICollectionView FilteredBuildAdapters
      {
         get
         {
            return CollectionViewSourceBuildAdapters.View;
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

      /// <summary>Gets the remove tag from build command.</summary>
      public ICommand RemoveTagFromBuildCommand { get; private set; }

      /// <summary>Gets or sets the selected build adapters.</summary>
      public ObservableCollection<BuildAdapter> SelectedBuildAdapters { get; private set; }

      /// <summary>Gets or sets the selected existing tag.</summary>
      public FilterTag SelectedExistingTag
      {
         get
         {
            return selectedExistingTag;
         }

         set
         {
            if (selectedExistingTag == value)
            {
               return;
            }

            selectedExistingTag = value;
            OnPropertyChanged();
         }
      }

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
            if (Math.Abs(zoomFactor - value) < Tolerance)
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

         foreach (var adapter in FilteredBuildAdapters.OfType<BuildAdapter>())
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
         var notIgnoredBuildAdapters = FilteredBuildAdapters.OfType<BuildAdapter>().Where(b => !b.IgnoreStatus).ToList();
         if (notIgnoredBuildAdapters.Any(b => b.Status == BuildStatus.PartiallySucceeded))
         {
            progressBarState = TaskbarProgressBarState.Paused;
         }

         if (notIgnoredBuildAdapters.Any(b => b.Status == BuildStatus.Failed))
         {
            progressBarState = TaskbarProgressBarState.Error;
         }

         if (notIgnoredBuildAdapters.All(b => b.Status == BuildStatus.Unknown || b.Status == BuildStatus.Error))
         {
            currentValue = 0;
         }

         var progress = TaskbarManager.Instance;
         progress.SetProgressState(progressBarState);
         progress.SetProgressValue(currentValue, 100);
      }

      private void DispatcherTimerTick(object sender, EventArgs e)
      {
         if (FilteredBuildAdapters.OfType<BuildAdapter>().All(build => build.Status != BuildStatus.Waiting))
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

      private void DoFilter(object obj)
      {
         var filterTag = obj as FilterTag;
         if (filterTag == null)
         {
            return;
         }

         if (!filterTag.IsAllFilter && filterTag.IsSelected)
         {
            AvailableTags.First(x => x.IsAllFilter).IsSelected = false;
         }

         if (!filterTag.IsAllFilter && !filterTag.IsSelected && AvailableTags.All(x => !x.IsSelected))
         {
            AvailableTags.First(x => x.IsAllFilter).IsSelected = true;
         }

         if (filterTag.IsAllFilter && filterTag.IsSelected)
         {
            foreach (var tag in AvailableTags.Where(x => !x.IsAllFilter))
            {
               tag.IsSelected = false;
            }
         }

         CollectionViewSourceBuildAdapters.View.Refresh();
      }

      /// <summary>Fills the available tags.</summary>
      internal void FillAvailableTags()
      {
         foreach (var tag in BuildAdapters.SelectMany(x => x.Tags))
         {
            InsertSorted(tag);
         }

         for (var i = 0; i < AvailableTags.Count; i++)
         {
            if (AvailableTags[i].IsAllFilter)
            {
               continue;
            }

            if (!BuildAdapters.SelectMany(x => x.Tags).Any(x => string.Equals(x, AvailableTags[i].Label, StringComparison.CurrentCulture)))
            {
               AvailableTags.RemoveAt(i);
               i--;
            }
         }
      }

      private void CollectionViewSourceBuildAdaptersFilter(object sender, FilterEventArgs e)
      {
         if (AvailableTags.Count == 0 || AvailableTags.First(x => x.IsAllFilter).IsSelected)
         {
            e.Accepted = true;
            return;
         }

         var buildAdapter = e.Item as BuildAdapter;
         if (buildAdapter == null)
         {
            e.Accepted = false;
            return;
         }

         e.Accepted =
            AvailableTags.Where(filterTag => filterTag.IsSelected).Any(
               filterTag => buildAdapter.Tags.Any(word => string.Equals(filterTag.Label, word, StringComparison.InvariantCultureIgnoreCase)));
      }

      private void InsertSorted(string tag)
      {
         if (AvailableTags.Any(x => string.Equals(tag, x.Label, StringComparison.InvariantCultureIgnoreCase)))
         {
            return;
         }

         var index = -1;

         for (var i = 0; i < AvailableTags.Count; i++)
         {
            if (string.Compare(AvailableTags[i].Label, tag, StringComparison.InvariantCultureIgnoreCase) > 0)
            {
               index = i;
               break;
            }
         }

         if (index < 0)
         {
            AvailableTags.Add(new FilterTag { Label = tag });
         }
         else
         {
            AvailableTags.Insert(index, new FilterTag { Label = tag });
         }
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