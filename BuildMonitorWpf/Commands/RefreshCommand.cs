
namespace BuildMonitorWpf.Commands
{
   using System;
   using System.Windows.Input;

   using BuildMonitorWpf.ViewModel;

   public class RefreshCommand : ICommand
   {
      private readonly MainWindowViewModel owner;

      public RefreshCommand(MainWindowViewModel owner)
      {
         this.owner = owner;
      }

      public bool CanExecute(object parameter)
      {
         return true;
      }

      public void Execute(object parameter)
      {
         owner.Refresh();
      }

      public event EventHandler CanExecuteChanged;
   }
}
