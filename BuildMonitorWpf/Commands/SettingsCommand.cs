
namespace BuildMonitorWpf.Commands
{
   using System;
   using System.Windows;
   using System.Windows.Input;

   using BuildMonitorWpf.View;
   using BuildMonitorWpf.ViewModel;

   public class SettingsCommand : ICommand
   {
      private readonly MainWindowViewModel mainWindowViewModel;

      public SettingsCommand(MainWindowViewModel mainWindowViewModel)
      {
         this.mainWindowViewModel = mainWindowViewModel;
      }

      public bool CanExecute(object parameter)
      {
         return true;
      }

      public void Execute(object parameter)
      {
         var settingsView = new SettingsView { Owner = Application.Current.MainWindow };
         settingsView.DataContext = new SettingsViewModel(settingsView, mainWindowViewModel);
         settingsView.ShowDialog();
      }

      public event EventHandler CanExecuteChanged;
   }
}
