
namespace BuildMonitorWpf.Converters
{
   using System;
   using System.Globalization;
   using System.Windows.Data;

   using BuildMonitor.Logic.Contracts;

   using BuildMonitorWpf.Properties;

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
               return Resources.BuildFailed;
            case BuildStatus.PartiallySucceeded:
               return Resources.BuildPartiallySucceeded;
            case BuildStatus.NotStarted:
               return Resources.BuildNotStarted;
            case BuildStatus.Succeeded:
               return Resources.BuildSucceeded;
            case BuildStatus.Stopped:
               return Resources.BuildStopped;
            case BuildStatus.Unknown:
               return Resources.BuildUnknown;
            case BuildStatus.Error:
               return Resources.BuildError;
         }

         return null;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         throw new NotImplementedException();
      }
   }
}
