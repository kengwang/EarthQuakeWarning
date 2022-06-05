using System.Windows;
using EarthquakeWaring.App.Infrastructure.Models;
using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;
using EarthquakeWaring.App.Services;

namespace EarthquakeWaring.App.Windows;

public partial class EarlyWarningWindow : Window
{
    public EarlyWarningWindow(EarthQuakeTrackingInformation information)
    {
        DataContext = information;
        InitializeComponent();
    }
}