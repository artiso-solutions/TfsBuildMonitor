
namespace BuildMonitorWpf.Converters
{
   using System;
   using System.Globalization;
   using System.Windows.Data;
   using System.Windows.Media.Imaging;

   using BuildMonitor.Logic.Contracts;

   public class BuildStatusToImageConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         if (!(value is BuildStatus))
         {
            return null;
         }

         string color = null;
         switch ((BuildStatus)value)
         {
            case BuildStatus.Failed:
               color = "red";
               break;
            case BuildStatus.PartiallySucceeded:
               color = "orange";
               break;
            case BuildStatus.NotStarted:
               color = "notstarted";
               break;
            case BuildStatus.Succeeded:
               color = "green";
               break;
            case BuildStatus.Stopped:
               color = "stopped";
               break;
            case BuildStatus.Waiting:
               color = "wait";
               break;
            case BuildStatus.Unknown:
               color = "noidea";
               break;
            case BuildStatus.Error:
               color = "error";
               break;
         }

         if (string.IsNullOrEmpty(color))
         {
            return null;
         }

         var uri = new Uri("pack://application:,,,/BuildMonitorWpf;component/Images/" + color + ".png");
         return new BitmapImage(uri);
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         throw new NotImplementedException();
      }
   }
}
