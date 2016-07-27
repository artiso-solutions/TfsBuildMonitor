using System;
using BuildMonitorWpf.ViewModel;

namespace BuildMonitorWpf.Commands
{
   using System.Linq;
   using System.Windows.Input;

   using BuildMonitor.Logic.Contracts;

   using BuildMonitorWpf.Properties;
   using BuildMonitorWpf.View;

   public class ClosePinBuildCommand : ICommand
   {
      private readonly PinBuildView pinBuildView;

      private readonly BuildInformation buildInformation;
      private readonly MainWindowViewModel mainWindowViewModel;

      public ClosePinBuildCommand(PinBuildView pinBuildView, BuildInformation buildInformation, MainWindowViewModel mainWindowViewModel)
      {
         this.pinBuildView = pinBuildView;
         this.buildInformation = buildInformation;
         this.mainWindowViewModel = mainWindowViewModel;
      }

      public bool CanExecute(object parameter)
      {
         return true;
      }

      public void Execute(object parameter)
      {
         var buildDefinition = mainWindowViewModel.MonitorViewModel.BuildServers.SelectMany(x => x.BuildDefinitions).FirstOrDefault(x => x.Id == buildInformation.BuildDefinitionId);
         if (buildDefinition != null)
         {
            buildDefinition.IsPined = false;
            Settings.Default.Save();
         }

         pinBuildView.Close();
      }

      public event EventHandler CanExecuteChanged;
   }
}
