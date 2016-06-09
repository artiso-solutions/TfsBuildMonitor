
namespace BuildMonitorWpf.Converters
{
   using System;
   using System.Globalization;
   using System.Windows;
   using System.Windows.Data;

   public class BooleanToVisibilityConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         if (parameter is string && string.Equals((string)parameter, "NOT", StringComparison.InvariantCultureIgnoreCase))
         {
            return (bool)value ? Visibility.Collapsed : Visibility.Visible;
         }
         
         return (bool)value ? Visibility.Visible : Visibility.Collapsed;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         throw new NotImplementedException();
      }
   }
}
