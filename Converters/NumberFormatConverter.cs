using System;
using System.Globalization;
using System.Windows.Data;

namespace CarAnalysisDashboard.Converters
{
    public class NumberFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;

            var format = parameter as string ?? "N0"; // デフォルトは整数のカンマ区切り
            
            if (value is int intValue)
            {
                return intValue.ToString(format);
            }
            else if (value is decimal decimalValue)
            {
                return decimalValue.ToString(format);
            }
            else if (value is double doubleValue)
            {
                return doubleValue.ToString(format);
            }
            else if (value is float floatValue)
            {
                return floatValue.ToString(format);
            }

            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && !string.IsNullOrWhiteSpace(stringValue))
            {
                // カンマを除去してパース
                var cleanValue = stringValue.Replace(",", "");
                
                if (targetType == typeof(int) || targetType == typeof(int?))
                {
                    if (int.TryParse(cleanValue, out int intResult))
                        return intResult;
                }
                else if (targetType == typeof(decimal) || targetType == typeof(decimal?))
                {
                    if (decimal.TryParse(cleanValue, out decimal decimalResult))
                        return decimalResult;
                }
                else if (targetType == typeof(double) || targetType == typeof(double?))
                {
                    if (double.TryParse(cleanValue, out double doubleResult))
                        return doubleResult;
                }
            }

            return value;
        }
    }
}