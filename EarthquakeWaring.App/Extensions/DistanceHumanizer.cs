using System;
using System.Globalization;
using System.Windows.Data;

namespace EarthquakeWaring.App.Extensions;

public class DistanceHumanizer : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            int distanceInt => "震中距您 " + distanceInt + " 公里",
            double distanceDouble => "震中距您 " + distanceDouble.ToString("F1") + " 公里",
            _ => "震中距您 未知"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}