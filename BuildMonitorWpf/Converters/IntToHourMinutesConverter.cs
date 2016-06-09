
namespace BuildMonitorWpf.Converters
{
   using System;
   using System.Globalization;
   using System.Windows.Data;

   public class IntToHourMinutesConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         if (!(value is int))
         {
            return string.Empty;
         }

         var totalMinutes = (int)value;
         var totalHours = totalMinutes / 60;
         var restMinutes = totalMinutes - totalHours * 60;
         return string.Format("{0:D}h{1:D2}", totalHours, restMinutes);
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         throw new NotImplementedException();
      }
   }
}
