namespace BuildMonitorWpf.Commands
{
   using System;
   using System.Linq;
   using System.Windows.Input;

   using BuildMonitor.Logic.Contracts;

   using BuildMonitorWpf.Adapter;
   using BuildMonitorWpf.Contracts;
   using BuildMonitorWpf.Extensions;
   using BuildMonitorWpf.Properties;
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
         var settings = Settings.Default;
         if (mainWindowViewModel.Maximum != settingsViewModel.RefreshInterval)
         {
            settings.RefreshInterval = settingsViewModel.RefreshInterval;

            mainWindowViewModel.Maximum = settingsViewModel.RefreshInterval;
            if (mainWindowViewModel.ActualValue > settingsViewModel.RefreshInterval)
            {
               mainWindowViewModel.ActualValue = settingsViewModel.RefreshInterval;
            }
         }

         settings.BigSize = settingsViewModel.BigSizeMode;
         mainWindowViewModel.BigSizeMode = settingsViewModel.BigSizeMode;

         settings.ZoomFactor = settingsViewModel.ZoomFactor / 100.0;
         mainWindowViewModel.ZoomFactor = settings.ZoomFactor;

         if (settings.BuildServers == null)
         {
            settings.BuildServers = new BuildServerCollection();
         }

         var widths = settings.ColumnWidths.Split(',');
         for (var i = 0; i < widths.Length; i++)
         {
            var information = widths[i].Split(':');
            widths[i] = string.Concat(information[0], ":", settingsViewModel.ColumnVisibilities[i].Checked ? "true" : "false");
         }

         mainWindowViewModel.ColumnWidths = string.Join(",", widths);

         settings.BuildServers.BuildServers.Clear();
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
            settings.BuildServers.BuildServers.Add(buildServer);

            foreach (var buildDefinitionResult in buildServerAdapter.BuildDefinitionResults.Where(x => x.Selected))
            {
               buildServer.BuildDefinitions.Add(
                  new BuildDefinition
                  {
                     Id = buildDefinitionResult.Id, 
                     Name = buildDefinitionResult.Name, 
                     Uri = buildDefinitionResult.Uri, 
                     Url = buildDefinitionResult.Url, 
                     ProjectId = buildDefinitionResult.ProjectId
                  });
            }

            foreach (var build in buildServer.GetBuilds())
            {
               mainWindowViewModel.BuildAdapters.Add(new BuildAdapter(mainWindowViewModel, build, false));
            }
         }

         mainWindowViewModel.Refresh();
         settings.Save();

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