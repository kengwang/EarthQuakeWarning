using System;
using System.Globalization;
using System.Windows.Data;

namespace EarthquakeWaring.App.Extensions;

public class DateTimeHumanizer : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
        {
            return dateTime.ToString("yyyy年MM月dd日 hh:mm:ss");
        }

        return "未知时间";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}