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

      public ClosePinBuildCommand(PinBuildView pinBuildView, BuildInformation buildInformation)
      {
         this.pinBuildView = pinBuildView;
         this.buildInformation = buildInformation;
      }

      public bool CanExecute(object parameter)
      {
         return true;
      }

      public void Execute(object parameter)
      {
         var buildDefinition = MonitorSettingsContainer.BuildServers.SelectMany(x => x.BuildDefinitions).FirstOrDefault(x => x.Id == buildInformation.BuildDefinitionId);
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
