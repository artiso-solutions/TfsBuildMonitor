
namespace BuildMonitorWpf.ViewModel
{
   using System;
   using System.ComponentModel;
   using System.Windows.Input;
   using System.Windows.Media;
   using System.Windows.Threading;

   using BuildMonitorWpf.Commands;
   using BuildMonitorWpf.View;

   public class ToastViewModel : ViewModelBase
   {
      private readonly DispatcherTimer toastLifeTimeTimer;

      private readonly DispatcherTimer opacityTimer;

      private const double Tolerance = 0.001;

      private ImageSource image;

      private double opacity = 1;

      private string title;

      private string message1;

      private string message2;

      public ToastViewModel(ToastView owner)
      {
         CloseCommand = new CloseCommand(owner);

         toastLifeTimeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5), IsEnabled = true };
         toastLifeTimeTimer.Tick += ToastLifeTimeTimerTick;

         opacityTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(75), IsEnabled = false };
         opacityTimer.Tick += OpacityTimerTick;

         owner.MouseDown += ToastViewMouseDown;
         owner.MouseMove += ToastViewMouseMove;
         owner.Closing += ToastViewClosing;
      }

      public ICommand CloseCommand { get; private set; }

      public ImageSource Image
      {
         get
         {
            return image;
         }
         set
         {
            if (image == value)
            {
               return;
            }

            image = value;
            OnPropertyChanged();
         }
      }

      public double Opacity
      {
         get
         {
            return opacity;
         }
         set
         {
            if (Math.Abs(opacity - value) < Tolerance)
            {
               return;
            }

            opacity = value;
            OnPropertyChanged();
         }
      }

      public string Title
      {
         get
         {
            return title;
         }
         set
         {
            if (title == value)
            {
               return;
            }

            title = value;
            OnPropertyChanged();
         }
      }

      public string MessageLine1
      {
         get
         {
            return message1;
         }
         set
         {
            if (message1 == value)
            {
               return;
            }

            message1 = value;
            OnPropertyChanged();
         }
      }


      public string MessageLine2
      {
         get
         {
            return message2;
         }
         set
         {
            if (message2 == value)
            {
               return;
            }

            message2 = value;
            OnPropertyChanged();
         }
      }

      private void ToastViewMouseDown(object sender, MouseButtonEventArgs e)
      {
         CloseCommand.Execute(null);
      }

      private void OpacityTimerTick(object sender, EventArgs e)
      {
         Opacity -= 0.05;
         if (Opacity <= 0)
         {
            CloseCommand.Execute(null);
         }
      }

      private void ToastLifeTimeTimerTick(object sender, EventArgs e)
      {
         opacityTimer.IsEnabled = true;
      }

      private void ToastViewMouseMove(object sender, MouseEventArgs e)
      {
         if (opacityTimer.IsEnabled)
         {
            Opacity = 1;
            opacityTimer.Stop();
         }

         toastLifeTimeTimer.Stop();
         toastLifeTimeTimer.Start();
      }

      private void ToastViewClosing(object sender, CancelEventArgs e)
      {
         var toastView = (ToastView)sender;
         toastView.Closing -= ToastViewClosing;
         toastView.MouseDown -= ToastViewMouseDown;
         toastView.MouseMove -= ToastViewMouseMove;
         opacityTimer.Stop();
         toastLifeTimeTimer.Stop();
         opacityTimer.Tick -= OpacityTimerTick;
         toastLifeTimeTimer.Tick -= ToastLifeTimeTimerTick;
      }
   }
}
