
namespace BuildMonitorWpf.Commands
{
   using System;
   using System.Windows;
   using System.Windows.Input;

   using BuildMonitor.Logic.Contracts;

   using BuildMonitorWpf.Adapter;

   public class NotificationCommand : ICommand
   {
      public bool CanExecute(object parameter)
      {
#if DEBUG
         return true;
#endif

         return false;
      }

      public void Execute(object parameter)
      {
         var result = new BuildResult { Name = "Product-Main", Status = BuildStatus.Failed, RequestedBy = "Some user" };
         ToastNotifications.CreateToastNotification(result, false, (o, e) => MessageBox.Show("Toast notification clicked"));
      }

      public event EventHandler CanExecuteChanged;
   }
}
