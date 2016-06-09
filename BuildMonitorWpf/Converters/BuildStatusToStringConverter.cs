
namespace BuildMonitorWpf.Converters
{
   using System;
   using System.Globalization;
   using System.Windows.Data;

   using BuildMonitor.Logic.Contracts;

   public class BuildStatusToStringConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         if (!(value is BuildStatus))
         {
            return null;
         }

         switch ((BuildStatus)value)
         {
            case BuildStatus.Failed:
               return "Build is failed";
            case BuildStatus.PartiallySucceeded:
               return "Build is partially succeeded";
            case BuildStatus.NotStarted:
               return "Build is not started";
            case BuildStatus.Succeeded:
               return "Build is succeeded";
            case BuildStatus.Stopped:
               return "Build was stopped";
            case BuildStatus.Unknown:
               return "No idea - offline";
            case BuildStatus.Error:
               return "An exception occured";
         }

         return null;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         throw new NotImplementedException();
      }
   }
}
