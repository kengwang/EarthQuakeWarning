using System;
using System.Globalization;
using System.Windows.Data;
using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;

namespace EarthquakeWaring.App.Extensions;

public class PositionHumanizer : IValueConverter

{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is EarthQuakeTrackingInformation info)
        {
            return (info.Longitude > 0 ? "E" : "W") + info.Longitude + (info.Latitude > 0 ? ", N" : ", S") + info.Latitude;
        }

        return "未知经纬";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}