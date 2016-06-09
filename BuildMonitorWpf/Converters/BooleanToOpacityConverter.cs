namespace BuildMonitorWpf.Converters
{
   using System;
   using System.Globalization;
   using System.Windows.Data;

   public class BooleanToOpacityConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         if (!(value is bool))
         {
            return null;
         }

         var enabled = (bool)value;
         return enabled ? 1.0 : 0.5;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         return null;
      }
   }
}
