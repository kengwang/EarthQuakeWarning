using System;
using System.Globalization;
using System.Windows.Data;

namespace EarthquakeWaring.App.Extensions;

public class IntensityDescriptor : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double intensity)
        {
            return intensity switch
            {
                < 2 => "基本无晃动",
                < 4 => "可能有轻微震感，请勿慌张",
                < 6 => "有强烈震感，请远离悬挂物",
                >= 6 => "请迅速逃生",
                _ => "未知"
            };
        }

        return "未知";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}