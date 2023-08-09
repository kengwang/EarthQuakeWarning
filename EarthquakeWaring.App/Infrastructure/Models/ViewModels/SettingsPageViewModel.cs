using System.Reflection;
using EarthquakeWaring.App.Infrastructure.Models.SettingModels;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;

namespace EarthquakeWaring.App.Infrastructure.Models.ViewModels;

public class SettingsPageViewModel
{
    public ISetting<CurrentPosition>? CurrentPosition { get; set; }
    public ISetting<AlertLimit>? AlertLimit { get; set; }
    public ISetting<UpdaterSetting>? UpdateSetting { get; set; }
    public ISetting<TrackerSetting>? TrackerSetting { get; set; }
    public ISetting<TimeSetting>? TimeSetting { get; set; }

    public string? Version => Assembly.GetExecutingAssembly().GetName().Version?.ToString();

    public SettingsPageViewModel(ISetting<CurrentPosition> currentPosition, ISetting<AlertLimit> alertLimit,
        ISetting<UpdaterSetting>? updateSetting, ISetting<TrackerSetting>? trackerSetting, ISetting<TimeSetting>? timeSetting)
    {
        CurrentPosition = currentPosition;
        AlertLimit = alertLimit;
        UpdateSetting = updateSetting;
        TrackerSetting = trackerSetting;
        TimeSetting = timeSetting;
    }
}