namespace BuildMonitorWpf.ViewModel
{
   using System.Collections.ObjectModel;
   using System.Linq;
   using System.Windows.Input;

   using BuildMonitor.Logic.Contracts;

   using BuildMonitorWpf.Adapter;
   using BuildMonitorWpf.Commands;
   using BuildMonitorWpf.View;

   /// <summary>The settings view model.</summary>
   /// <seealso cref="System.ComponentModel.INotifyPropertyChanged"/>
   public class SettingsViewModel : ViewModelBase
   {
      #region Constants and Fields

      private int selectedIndex;

      #endregion

      #region Constructors and Destructors

      /// <summary>Initializes a new instance of the <see cref="SettingsViewModel"/> class.</summary>
      /// <param name="owner">The owner.</param>
      /// <param name="mainWindowViewModel">The main window view model.</param>
      public SettingsViewModel(SettingsView owner, MainWindowViewModel mainWindowViewModel)
      {
         if (!MonitorSettingsContainer.BuildServers.Any())
         {
            MonitorSettingsContainer.BuildServers.Add(new BuildServer());
         }

         var removeServerCommand = new RemoveServerCommand(this);
         BuildServers = new ObservableCollection<BuildServerAdapter>(MonitorSettingsContainer.BuildServers.Select(x => new BuildServerAdapter(x, removeServerCommand)));

         AddNewServerCommand = new AddNewServerCommand(this);
         OkCommand = new ValidSettingsCommand(owner, this, mainWindowViewModel);
         CancelCommand = new CloseCommand(owner);
      }

      #endregion

      #region Public Properties

      /// <summary>Gets the add new server command.</summary>
      public ICommand AddNewServerCommand { get; private set; }

      /// <summary>Gets the build servers.</summary>
      public ObservableCollection<BuildServerAdapter> BuildServers { get; private set; }

      /// <summary>Gets the cancel command.</summary>
      public ICommand CancelCommand { get; private set; }

      /// <summary>Gets the ok command.</summary>
      public ICommand OkCommand { get; private set; }

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

      #endregion
   }
}