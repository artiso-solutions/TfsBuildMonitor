namespace BuildMonitorWpf.Converters
{
   using System;
   using System.Globalization;
   using System.Linq;
   using System.Windows;
   using System.Windows.Data;

   /// <summary>The enumeration to visibility converter.</summary>
   /// <seealso cref="System.Windows.Data.IValueConverter"/>
   public class EnumerationToVisibilityConverter : IValueConverter
   {
      #region IValueConverter Members

      /// <summary>Converts a value.</summary>
      /// <param name="value">The value produced by the binding source.</param>
      /// <param name="targetType">The type of the binding target property.</param>
      /// <param name="parameter">The converter parameter to use.</param>
      /// <param name="culture">The culture to use in the converter.</param>
      /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         if (!(value is Enum))
         {
            return Visibility.Collapsed;
         }

         var parameters = (parameter as string).Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
         var stringValue = ((Enum)value).ToString("G");
         return parameters.Any(param => string.Equals(stringValue, param, StringComparison.InvariantCultureIgnoreCase)) ? Visibility.Visible : Visibility.Collapsed;
      }

      /// <summary>Converts a value.</summary>
      /// <param name="value">The value that is produced by the binding target.</param>
      /// <param name="targetType">The type to convert to.</param>
      /// <param name="parameter">The converter parameter to use.</param>
      /// <param name="culture">The culture to use in the converter.</param>
      /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         return Binding.DoNothing;
      }

      #endregion
   }
}