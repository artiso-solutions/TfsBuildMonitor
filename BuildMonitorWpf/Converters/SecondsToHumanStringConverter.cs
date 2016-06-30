namespace BuildMonitorWpf.Converters
{
   using System;
   using System.Globalization;
   using System.Windows.Data;

   using BuildMonitorWpf.Properties;

   /// <summary>The seconds to human string converter.</summary>
   /// <seealso cref="System.Windows.Data.IValueConverter"/>
   public class SecondsToHumanStringConverter : IValueConverter
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
         var seconds = (int)value;
         if (seconds < 60)
         {
            return string.Format(Resources.Seconds, seconds);
         }

         var minutes = (int)Math.Floor(seconds / 60.0);
         seconds -= minutes * 60;
         return string.Format(Resources.MinutesSeconds, minutes, seconds);
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