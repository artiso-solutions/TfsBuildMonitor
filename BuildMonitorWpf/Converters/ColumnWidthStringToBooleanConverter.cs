
namespace BuildMonitorWpf.Converters
{
   using System;
   using System.Globalization;
   using System.Linq;
   using System.Windows.Data;

   using BuildMonitorWpf.Properties;

   public class ColumnWidthStringToBooleanConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         var stringValue = string.Format("{0}", value);
         if (string.IsNullOrEmpty(stringValue))
         {
            return false;
         }

         int index;
         if (!int.TryParse(string.Format("{0}", parameter), out index))
         {
            return false;
         }

         var widths = stringValue.Split(',');
         if (index >= widths.Length)
         {
            return false;
         }

         var informations = widths[index].Split(':');
         return informations[1] == "true";
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         var columnWidths = Settings.Default.ColumnWidths;

         if (!(value is bool))
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

         var boolValue = (bool)value;
         widths[index] = string.Concat(informations[0], ":", boolValue ? "true" : "false");
         return string.Join(",", widths);

      }
   }
}
