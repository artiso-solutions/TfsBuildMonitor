
namespace BuildMonitorWpf.Commands
{
   using System;
   using System.Diagnostics;
   using System.Windows.Input;

   public class OpenBrowserCommand : ICommand
   {
      public bool CanExecute(object parameter)
      {
         return true;
      }

      public void Execute(object parameter)
      {
         var uri = string.Format("{0}", parameter);
         if (!string.IsNullOrEmpty(uri))
         {
            Process.Start(uri);
         }
      }

      public event EventHandler CanExecuteChanged;
   }
}
