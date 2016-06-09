namespace BuildMonitorWpf.Commands
{
   using System;
   using System.ComponentModel;
   using System.Linq;
   using System.Windows.Controls;
   using System.Windows.Input;

   using BuildMonitor.Logic.BuildExplorer;
   using BuildMonitor.Logic.Contracts;
   using BuildMonitor.Logic.Interfaces;

   using BuildMonitorWpf.Adapter;

   /// <summary>The TFS connect command.</summary>
   /// <seealso cref="System.Windows.Input.ICommand"/>
   public class TfsConnectCommand : ICommand
   {
      #region Constants and Fields

      private readonly BuildServerAdapter buildServerAdapter;

      private readonly ICrypter crypter;

      private bool busy;

      #endregion

      #region Constructors and Destructors

      /// <summary>Initializes a new instance of the <see cref="TfsConnectCommand"/> class.</summary>
      /// <param name="buildServerAdapter">The build server adapter.</param>
      /// <param name="crypter">The crypter.</param>
      public TfsConnectCommand(BuildServerAdapter buildServerAdapter, ICrypter crypter)
      {
         this.buildServerAdapter = buildServerAdapter;
         this.crypter = crypter;

         buildServerAdapter.PropertyChanged += SettingsViewModelPropertyChanged;
      }

      #endregion

      #region ICommand Members

      /// <summary>Occurs when changes occur that affect whether or not the command should execute.</summary>
      public event EventHandler CanExecuteChanged;

      /// <summary>Defines the method that determines whether the command can execute in its current state.</summary>
      /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
      /// <returns>true if this command can be executed; otherwise, false.</returns>
      public bool CanExecute(object parameter)
      {
         return !busy && !string.IsNullOrEmpty(buildServerAdapter.TfsUrl);
      }

      /// <summary>Defines the method to be called when the command is invoked.</summary>
      /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
      public async void Execute(object parameter)
      {
         var passwordBox = parameter as PasswordBox;
         if (passwordBox != null && !string.IsNullOrEmpty(passwordBox.Password))
         {
            buildServerAdapter.CryptedPassword = crypter.Encrypt(passwordBox.Password);
         }

         var buildServer = new BuildServer
         {
            DomainName = buildServerAdapter.Domain,
            Login = buildServerAdapter.Login,
            PasswordBytes = buildServerAdapter.CryptedPassword,
            Url = buildServerAdapter.TfsUrl
         };

         busy = true;
         OnCanExecuteChanged();
         buildServerAdapter.ConnectionProgress = "Connecting to TFS server...";
         var buildExplorer = BuildExplorerFactory.CreateBuildExplorer(buildServerAdapter.TfsVersion);
         var result = await buildExplorer.GetBuildDefinitions(buildServer);

         buildServerAdapter.ConnectionProgress = "Propagating results ...";

         buildServerAdapter.BuildDefinitionResults.Clear();
         foreach (var buildResult in result.BuildDefinitions.OrderBy(x => x.Name))
         {
            var id = buildResult.Id;
            buildResult.Selected = buildServerAdapter.BuildDefinitions.Any(x => x.Id == id);
            buildServerAdapter.BuildDefinitionResults.Add(buildResult);
         }

         buildServerAdapter.ProjectNames.Clear();
         buildServerAdapter.ProjectNames.Add(BuildServerAdapter.AllString);
         foreach (var projectName in result.BuildDefinitions.Select(x => x.ProjectName).Distinct().OrderBy(x => x))
         {
            buildServerAdapter.ProjectNames.Add(projectName);
         }

         buildServerAdapter.SelectedProjectName = BuildServerAdapter.AllString;
         buildServerAdapter.ConnectionProgress = result.Message;

         busy = false;
         OnCanExecuteChanged();
      }

      #endregion

      #region Methods

      private void OnCanExecuteChanged()
      {
         var handler = CanExecuteChanged;
         if (handler != null)
         {
            handler(this, EventArgs.Empty);
         }
      }

      private void SettingsViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
      {
         if (e.PropertyName == "TfsUrl")
         {
            OnCanExecuteChanged();
         }
      }

      #endregion
   }
}