
namespace BuildMonitorWpf.Adapter
{
   using System;
   using System.Collections.Generic;
   using System.Windows;
   using System.Windows.Media.Imaging;

   using BuildMonitor.Logic.Contracts;

   using BuildMonitorWpf.View;
   using BuildMonitorWpf.ViewModel;

   /// <summary>
   /// Defines the toast notifications;
   /// </summary>
   internal static class ToastNotifications
   {
      private static readonly List<double> DisplayedToastViewHeights = new List<double>();

      /// <summary>
      /// Creates the toast notification.
      /// </summary>
      /// <param name="result">The result.</param>
      /// <param name="isForTest">if set to <c>true</c> [is for test].</param>
      /// <param name="toastOnActivated">The toast on activated.</param>
      internal static void CreateToastNotification(BuildResult result, bool isForTest, EventHandler<EventArgs> toastOnActivated)
      {
         var toastView = new ToastView { Owner = Application.Current.MainWindow };
         var toastModel = new ToastViewModel(toastView)
         {
            Title = result.Name,
            MessageLine1 = isForTest ? "Running build will fail" : result.Status.ToString(),
            MessageLine2 = isForTest ? result.RunningBuildRequestedBy : result.RequestedBy,
         };
         toastView.DataContext = toastModel;

         toastView.Left = SystemParameters.FullPrimaryScreenWidth - toastView.Width - 10;
         var top = SystemParameters.FullPrimaryScreenHeight - toastView.Height;
         while (DisplayedToastViewHeights.Contains(top))
         {
            top -= toastView.Height + 10;
         }

         toastView.Top = top;
         DisplayedToastViewHeights.Add(top);

         string image = null;
         switch (result.Status)
         {
            case BuildStatus.InProgress:
               image = "inprogress_128";
               break;

            case BuildStatus.Stopped:
               image = "stop_128";
               break;

            case BuildStatus.PartiallySucceeded:
               image = "orange_128";
               break;

            case BuildStatus.Failed:
               image = "red_128";
               break;
         }

         if (isForTest)
         {
            image = "build_128";
         }

         if (image != null)
         {
            var uri = new Uri("pack://application:,,,/BuildMonitorWpf;component/Images/" + image + ".png");
            toastModel.Image = new BitmapImage(uri);
         }

         System.Media.SystemSounds.Hand.Play();
         toastView.Closing += ToastViewClosing;
         if (toastOnActivated != null)
         {
            toastView.MouseDown += (o, e) => toastOnActivated(o, e);
         }

         toastView.Show();
      }

      private static void ToastViewClosing(object sender, EventArgs e)
      {
         var toastView = (ToastView)sender;
         toastView.Closing -= ToastViewClosing;
         DisplayedToastViewHeights.Remove(toastView.Top);
      }
   }
}
