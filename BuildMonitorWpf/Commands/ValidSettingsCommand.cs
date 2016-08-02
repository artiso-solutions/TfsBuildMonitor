namespace BuildMonitorWpf.Commands
{
   using System;
   using System.Linq;
   using System.Windows.Input;

   using BuildMonitor.Logic.Contracts;

   using BuildMonitorWpf.Adapter;
   using BuildMonitorWpf.Extensions;
   using BuildMonitorWpf.View;
   using BuildMonitorWpf.ViewModel;

   /// <summary>The valid settings command.</summary>
   /// <seealso cref="System.Windows.Input.ICommand"/>
   public class ValidSettingsCommand : ICommand
   {
      #region Constants and Fields

      private readonly MainWindowViewModel mainWindowViewModel;

      private readonly SettingsView settingsView;

      private readonly SettingsViewModel settingsViewModel;

      #endregion

      #region Constructors and Destructors

      /// <summary>Initializes a new instance of the <see cref="ValidSettingsCommand"/> class.</summary>
      /// <param name="settingsView">The settings view.</param>
      /// <param name="settingsViewModel">The settings view model.</param>
      /// <param name="mainWindowViewModel">The main window view model.</param>
      public ValidSettingsCommand(SettingsView settingsView, SettingsViewModel settingsViewModel, MainWindowViewModel mainWindowViewModel)
      {
         this.settingsView = settingsView;
         this.settingsViewModel = settingsViewModel;
         this.mainWindowViewModel = mainWindowViewModel;

         foreach (var serverAdapter in settingsViewModel.BuildServers)
         {
            serverAdapter.TfsConnectCommand.CanExecuteChanged += TfsConnectCommandCanExecuteChanged;
         }
      }

      #endregion

      #region ICommand Members

      /// <summary>Defines the method that determines whether the command can execute in its current state.</summary>
      /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
      /// <returns>true if this command can be executed; otherwise, false.</returns>
      public bool CanExecute(object parameter)
      {
         return settingsViewModel.BuildServers.All(server => server.TfsConnectCommand.CanExecute(null));
      }

      /// <summary>Defines the method to be called when the command is invoked.</summary>
      /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
      public void Execute(object parameter)
      {
         var oldBuildAdapters = mainWindowViewModel.BuildAdapters.ToList();

         MonitorSettingsContainer.BuildServers.Clear();
         mainWindowViewModel.BuildAdapters.Clear();
         foreach (var buildServerAdapter in settingsViewModel.BuildServers)
         {
            var buildServer = new BuildServer
            {
               ServerName = buildServerAdapter.ServerName,
               DomainName = buildServerAdapter.Domain,
               Login = buildServerAdapter.Login,
               PasswordBytes = buildServerAdapter.CryptedPassword,
               Url = buildServerAdapter.TfsUrl,
               DetailBuildUrl = buildServerAdapter.DetailBuildUrl,
               TfsVersion = buildServerAdapter.TfsVersion
            };
            MonitorSettingsContainer.BuildServers.Add(buildServer);

            foreach (var buildDefinitionResult in buildServerAdapter.BuildDefinitionResults.Where(x => x.Selected))
            {
               var buildDefinition = new BuildDefinition
               {
                  Id = buildDefinitionResult.Id,
                  Name = buildDefinitionResult.Name,
                  Uri = buildDefinitionResult.Uri,
                  Url = buildDefinitionResult.Url,
                  ProjectId = buildDefinitionResult.ProjectId
               };

               var existingAdapter = oldBuildAdapters.FirstOrDefault(x => x.BuildInformation.BuildDefinitionId == buildDefinition.Id);
               if (existingAdapter != null)
               {
                  buildDefinition.Tags = existingAdapter.Tags.ToArray();
               }

               buildServer.BuildDefinitions.Add(buildDefinition);
            }

            foreach (var build in buildServer.GetBuilds())
            {
               mainWindowViewModel.BuildAdapters.Add(new BuildAdapter(mainWindowViewModel, build, false));
            }
         }

         mainWindowViewModel.Refresh();
         MonitorSettingsContainer.SaveBuildServers();

         settingsView.Close();
      }

      /// <summary>Occurs when changes occur that affect whether or not the command should execute.</summary>
      public event EventHandler CanExecuteChanged;

      #endregion

      #region Methods

      private void TfsConnectCommandCanExecuteChanged(object sender, EventArgs e)
      {
         var handler = CanExecuteChanged;
         if (handler != null)
         {
            handler(this, EventArgs.Empty);
         }
      }

      #endregion
   }
}