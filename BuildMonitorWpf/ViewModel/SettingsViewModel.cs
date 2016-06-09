namespace BuildMonitorWpf.ViewModel
{
   using System;
   using System.Collections.ObjectModel;
   using System.ComponentModel;
   using System.Linq;
   using System.Runtime.CompilerServices;
   using System.Windows.Input;

   using BuildMonitor.Logic.Contracts;

   using BuildMonitorWpf.Adapter;
   using BuildMonitorWpf.Commands;
   using BuildMonitorWpf.Contracts;
   using BuildMonitorWpf.Properties;
   using BuildMonitorWpf.View;

   /// <summary>The settings view model.</summary>
   /// <seealso cref="System.ComponentModel.INotifyPropertyChanged"/>
   public class SettingsViewModel : INotifyPropertyChanged
   {
      #region Constants and Fields

      private bool bigSizeMode;

      private int refreshInterval;

      private int selectedIndex;

      private int zoomFactor;

      #endregion

      #region Constructors and Destructors

      /// <summary>Initializes a new instance of the <see cref="SettingsViewModel"/> class.</summary>
      /// <param name="owner">The owner.</param>
      /// <param name="mainWindowViewModel">The main window view model.</param>
      public SettingsViewModel(SettingsView owner, MainWindowViewModel mainWindowViewModel)
      {
         refreshInterval = Settings.Default.RefreshInterval;
         bigSizeMode = Settings.Default.BigSize;
         zoomFactor = (int)(Settings.Default.ZoomFactor * 100.0);

         if (Settings.Default.BuildServers == null)
         {
            Settings.Default.BuildServers = new BuildServerCollection();
         }

         if (!Settings.Default.BuildServers.BuildServers.Any())
         {
            Settings.Default.BuildServers.BuildServers.Add(new BuildServer());
         }

         var removeServerCommand = new RemoveServerCommand(this);
         BuildServers = new ObservableCollection<BuildServerAdapter>(Settings.Default.BuildServers.BuildServers.Select(x => new BuildServerAdapter(x, removeServerCommand)));
         ColumnVisibilities = new ObservableCollection<ColumnVisibilityAdapter>();

         AddNewServerCommand = new AddNewServerCommand(this);
         OkCommand = new ValidSettingsCommand(owner, this, mainWindowViewModel);
         CancelCommand = new CloseCommand(owner);

         FillColumnVisibilities();
      }

      #endregion

      #region INotifyPropertyChanged Members

      /// <summary>Occurs when a property value changes.</summary>
      public event PropertyChangedEventHandler PropertyChanged;

      #endregion

      #region Public Properties

      /// <summary>Gets the add new server command.</summary>
      public ICommand AddNewServerCommand { get; private set; }

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

      /// <summary>Gets the build servers.</summary>
      public ObservableCollection<BuildServerAdapter> BuildServers { get; private set; }

      /// <summary>Gets the cancel command.</summary>
      public ICommand CancelCommand { get; private set; }

      /// <summary>Gets the column visibilities.</summary>
      public ObservableCollection<ColumnVisibilityAdapter> ColumnVisibilities { get; private set; }

      /// <summary>Gets the ok command.</summary>
      public ICommand OkCommand { get; private set; }

      /// <summary>Gets or sets the refresh interval.</summary>
      public int RefreshInterval
      {
         get
         {
            return refreshInterval;
         }

         set
         {
            if (refreshInterval == value)
            {
               return;
            }

            refreshInterval = value - value % 10;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets or sets the index of the selected.</summary>
      public int SelectedIndex
      {
         get
         {
            return selectedIndex;
         }

         set
         {
            if (selectedIndex == value)
            {
               return;
            }

            selectedIndex = value;
            OnPropertyChanged();
         }
      }

      /// <summary>Gets or sets the zoom factor.</summary>
      public int ZoomFactor
      {
         get
         {
            return zoomFactor;
         }

         set
         {
            if (zoomFactor == value)
            {
               return;
            }

            zoomFactor = value - value % 25;
            OnPropertyChanged();
         }
      }

      #endregion

      #region Methods

      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
      {
         var handler = PropertyChanged;
         if (handler != null)
            handler(this, new PropertyChangedEventArgs(propertyName));
      }

      private void FillColumnVisibilities()
      {
         var widths = Settings.Default.ColumnWidths.Split(',');
         var titles = new[] { "Ignore build", "Last build", "Running build", "Name", "Details", "Finished", "Action", "Pin build" };

         for (var i = 0; i < titles.Length; i++)
         {
            var information = i < widths.Length ? widths[i].Split(':') : new[] { "100", "true" };
            var ischecked = string.Equals(information[1], "true", StringComparison.InvariantCultureIgnoreCase);
            ColumnVisibilities.Add(new ColumnVisibilityAdapter { Title = string.Format("Column '{0}' is visible", titles[i]), Checked = ischecked });
         }
      }

      #endregion
   }
}