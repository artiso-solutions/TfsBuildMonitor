
namespace BuildMonitorWpf.Commands
{
   using System;
   using System.Windows;
   using System.Windows.Input;

   public class CloseCommand : ICommand
   {
      private readonly Window window;

      public CloseCommand(Window owner)
      {
         window = owner;
      }

      public bool CanExecute(object parameter)
      {
         return true;
      }

      public void Execute(object parameter)
      {
         if (window == null)
         {
            Application.Current.Shutdown();
         }
         else
         {
            window.Close();
         }
      }

      public event EventHandler CanExecuteChanged;
   }
}
