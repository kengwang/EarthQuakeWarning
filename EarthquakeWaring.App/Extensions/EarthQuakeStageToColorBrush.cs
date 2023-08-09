using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace EarthquakeWaring.App.Extensions;

public class EarthQuakeStageToColorBrush : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is EarthQuakeStage stage)
        {
            return new SolidColorBrush(stage switch
            {
                EarthQuakeStage.Ignore => Colors.Gray,
                EarthQuakeStage.Record => Colors.Gray,
                EarthQuakeStage.Warning => Colors.Orange,
                EarthQuakeStage.Emergency => Colors.Red,
                EarthQuakeStage.Forced => Colors.Red,
                _ => Colors.Gray
            });
        }

        return Colors.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}