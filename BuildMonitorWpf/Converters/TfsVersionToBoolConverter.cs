
namespace BuildMonitorWpf.Converters
{
   using System;
   using System.Globalization;
   using System.Windows.Data;

   using BuildMonitor.Logic.Contracts;

   public class TfsVersionToBoolConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         return string.Equals(string.Format("{0}", value), (string)parameter);
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         if ((bool)value)
         {
            return (TfsVersion)Enum.Parse(typeof(TfsVersion), (string)parameter);
         }

         return Binding.DoNothing;
      }
   }
}
