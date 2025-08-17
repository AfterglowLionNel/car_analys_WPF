using System;
using System.Globalization;
using System.Windows.Data;

namespace CarAnalysisDashboard.Converters
{
    public class ViewModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string viewMode && parameter is string targetMode)
            {
                return viewMode == targetMode;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked && parameter is string targetMode)
            {
                return targetMode;
            }
            return Binding.DoNothing;
        }
    }
}