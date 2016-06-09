
namespace BuildMonitorWpf.Commands
{
   using System;
   using System.Windows.Input;

   using BuildMonitorWpf.Adapter;
   using BuildMonitorWpf.ViewModel;

   public class RemoveServerCommand : ICommand
   {
      private readonly SettingsViewModel settingsViewModel;

      public RemoveServerCommand(SettingsViewModel settingsViewModel)
      {
         this.settingsViewModel = settingsViewModel;
      }

      public bool CanExecute(object parameter)
      {
         return true;
      }

      public event EventHandler CanExecuteChanged;

      public void Execute(object parameter)
      {
         var buildServerAdapter = parameter as BuildServerAdapter;
         if (buildServerAdapter == null)
         {
            return;
         }

         settingsViewModel.SelectedIndex--;
         settingsViewModel.BuildServers.Remove(buildServerAdapter);
      }
   }
}
