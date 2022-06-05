using System;
using System.Globalization;
using System.Windows.Data;

namespace EarthquakeWaring.App.Extensions;

public class DoubleLimiter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            double numberDouble => numberDouble.ToString("F1"),
            float numberFloat => numberFloat.ToString("F1"),
            decimal numberDecimal => numberDecimal.ToString("F1"),
            _ => "NaN"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}