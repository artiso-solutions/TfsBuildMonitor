
namespace BuildMonitorWpf.Converters
{
   using System;
   using System.Globalization;
   using System.Windows.Data;
   using System.Windows.Media;

   using BuildMonitor.Logic.Contracts;

   /// <summary>The build status to brush converter.</summary>
   /// <seealso cref="System.Windows.Data.IValueConverter"/>
   public class BuildStatusToBrushConverter : IValueConverter
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
         if (!(value is BuildStatus))
         {
            return null;
         }

         switch ((BuildStatus)value)
         {
            case BuildStatus.Failed:
               return new SolidColorBrush(Color.FromArgb(255, 255, 123, 113));

            case BuildStatus.PartiallySucceeded:
               return new SolidColorBrush(Color.FromArgb(255, 255, 220, 151));

            case BuildStatus.Succeeded:
               return new SolidColorBrush(Color.FromArgb(255, 176, 230, 176));

            default:
               return Brushes.WhiteSmoke;
         }
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