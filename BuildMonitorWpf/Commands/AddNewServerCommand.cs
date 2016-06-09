
namespace BuildMonitorWpf.Commands
{
   using System;
   using System.Windows.Input;

   using BuildMonitor.Logic.Contracts;

   using BuildMonitorWpf.Adapter;
   using BuildMonitorWpf.Contracts;
   using BuildMonitorWpf.ViewModel;

   public class AddNewServerCommand : ICommand
   {
      private readonly SettingsViewModel settingsViewModel;

      public AddNewServerCommand(SettingsViewModel settingsViewModel)
      {
         this.settingsViewModel = settingsViewModel;
      }

      public bool CanExecute(object parameter)
      {
         return true;
      }

      public void Execute(object parameter)
      {
         settingsViewModel.BuildServers.Add(new BuildServerAdapter(new BuildServer(), new RemoveServerCommand(settingsViewModel)));
         settingsViewModel.SelectedIndex = settingsViewModel.BuildServers.Count - 1;
      }

      public event EventHandler CanExecuteChanged;
   }
}
