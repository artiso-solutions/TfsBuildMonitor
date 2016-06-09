
namespace BuildMonitorWpf.Commands
{
   using System;
   using System.ComponentModel;
   using System.Linq;
   using System.Windows;
   using System.Windows.Input;

   using BuildMonitor.Logic.Contracts;

   using BuildMonitorWpf.Properties;
   using BuildMonitorWpf.View;
   using BuildMonitorWpf.ViewModel;

   public class PinBuildCommand : ICommand
   {
      private readonly BuildInformation buildInformation;

      private readonly MainWindowViewModel mainWindowViewModel;

      private bool isPined;

      public PinBuildCommand(MainWindowViewModel mainWindowViewModel, BuildInformation buildInformation)
      {
         this.buildInformation = buildInformation;
         this.mainWindowViewModel = mainWindowViewModel;
      }

      public bool CanExecute(object parameter)
      {
         return !isPined;
      }

      public void Execute(object parameter)
      {
         if (isPined)
         {
            return;
         }

         isPined = true;
         OnCanExecuteChanged();

         var pinBuildView = new PinBuildView { Owner = Application.Current.MainWindow };
         pinBuildView.DataContext = new PinBuildViewModel(pinBuildView, mainWindowViewModel, buildInformation);
         pinBuildView.Left = SystemParameters.FullPrimaryScreenWidth - pinBuildView.Width - 10;
         pinBuildView.Top = SystemParameters.FullPrimaryScreenHeight - pinBuildView.Height - 10;
         pinBuildView.Closing += PinBuildViewClosing;

         var buildDefinition = Settings.Default.BuildServers.BuildServers.SelectMany(x => x.BuildDefinitions).FirstOrDefault(x => x.Id == buildInformation.BuildDefinitionId);
         if (buildDefinition != null)
         {
            buildDefinition.IsPined = true;
            Settings.Default.Save();

            if (buildDefinition.PinLeft > 0 && buildDefinition.PinTop > 0)
            {
               pinBuildView.Left = buildDefinition.PinLeft;
               pinBuildView.Top = buildDefinition.PinTop;
            }
         }

         pinBuildView.Show();
      }

      private void PinBuildViewClosing(object sender, CancelEventArgs e)
      {
         isPined = false;
         OnCanExecuteChanged();
      }

      private void OnCanExecuteChanged()
      {
         var handler = CanExecuteChanged;
         if (handler != null)
         {
            handler(this, EventArgs.Empty);
         }
      }

      public event EventHandler CanExecuteChanged;
   }
}
