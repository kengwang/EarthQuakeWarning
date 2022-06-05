using System.Reflection;
using EarthquakeWaring.App.Infrastructure.Models.SettingModels;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;

namespace EarthquakeWaring.App.Infrastructure.Models.ViewModels;

public class SettingsPageViewModel
{
    public ISetting<CurrentPosition>? CurrentPosition { get; set; }
    public ISetting<AlertLimit>? AlertLimit { get; set; }

    public string? Version => Assembly.GetExecutingAssembly().GetName().Version?.ToString();

    public SettingsPageViewModel(ISetting<CurrentPosition> currentPosition, ISetting<AlertLimit> alertLimit)
    {
        CurrentPosition = currentPosition;
        AlertLimit = alertLimit;
    }
}