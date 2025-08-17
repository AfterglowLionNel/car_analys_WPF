using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CarAnalysisDashboard.Converters
{
    public class ViewModeVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string viewMode && parameter is string targetMode)
            {
                var modeMap = new Dictionary<string, string>
                {
                    {"Overview", "概要"},
                    {"PriceTrend", "価格推移"},
                    {"GradeAnalysis", "グレード分析"},
                    {"MileagePrice", "走行距離vs価格"}
                };

                if (modeMap.ContainsKey(targetMode))
                {
                    return viewMode == modeMap[targetMode] ? Visibility.Visible : Visibility.Collapsed;
                }
                
                return viewMode == targetMode ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}