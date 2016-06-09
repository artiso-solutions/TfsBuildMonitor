
namespace BuildMonitorWpf.Commands
{
   using System;
   using System.Diagnostics;
   using System.Windows.Input;

   public class SendMailCommand : ICommand
   {
      public bool CanExecute(object parameter)
      {
         return true;
      }

      public void Execute(object parameter)
      {
         Process.Start("mailto:ctissot@artiso.com;vgibilmanno@artiso.com;mrink@artiso.com");
      }

      public event EventHandler CanExecuteChanged;
   }
}
