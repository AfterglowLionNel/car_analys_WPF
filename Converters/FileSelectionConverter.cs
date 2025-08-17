using System;
using System.Globalization;
using System.Windows.Data;

namespace CarAnalysisDashboard.Converters
{
    public class FileSelectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}