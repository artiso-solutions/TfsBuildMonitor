
using BuildMonitorWpf.ViewModel;

namespace BuildMonitorWpf.Converters
{
   using System;
   using System.Globalization;
   using System.Linq;
   using System.Windows.Data;

   public class ColumnWidthStringToDoubleConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         var stringValue = string.Format("{0}", value);
         if (string.IsNullOrEmpty(stringValue))
         {
            return double.NaN;
         }

         int index;
         if (!int.TryParse(string.Format("{0}", parameter), out index))
         {
            return double.NaN;
         }

         var widths = stringValue.Split(',');
         if (index >= widths.Length)
         {
            return double.NaN;
         }

         var informations = widths[index].Split(':');
         if (string.Equals(informations[1], "false"))
         {
            return 0;
         }

         return string.Equals(informations[0], "NaN") ? double.NaN : double.Parse(informations[0]);
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         var columnWidths = MonitorViewModel.MonitorSettings.ColumnWidths;

         if (!(value is double))
         {
            return columnWidths;
         }

         int index;
         if (!int.TryParse(string.Format("{0}", parameter), out index))
         {
            return columnWidths;
         }

         var widths = columnWidths.Split(',').ToList();
         if (index >= widths.Count)
         {
            widths.Add("100:true");
         }

         var informations = widths[index].Split(':');

         var doubleValue = (double)value;
         widths[index] = string.Concat(double.IsNaN(doubleValue) ? "NaN" : string.Format("{0:N0}", doubleValue), ":", informations[1]);
         return string.Join(",", widths);
      }
   }
}
