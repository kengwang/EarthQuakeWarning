using EarthquakeWaring.App.Infrastructure.Models.SettingModels;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EarthquakeWaring.App.Infrastructure.Models.ViewModels;

public class SettingsPageViewModel : INotifyPropertyChanged
{
    private IServiceProvider _provider { get; set; }
    public ISetting<CurrentPosition>? CurrentPosition { get; set; }
    public ISetting<AlertLimit>? AlertLimit { get; set; }
    public ISetting<UpdaterSetting>? UpdateSetting { get; set; }
    public ISetting<TrackerSetting>? TrackerSetting { get; set; }
    public ISetting<TimeSetting>? TimeSetting { get; set; }
    public ISetting<GNSSSetting>? GNSSSetting { get; set; }
    public string? Version => Assembly.GetExecutingAssembly().GetName().Version?.ToString();
    public string? LastUpdated => _provider.GetService<ITimeHandler>()?.LastUpdated.ToString();
    public IReadOnlyList<EarthquakeSourceOption> EarthquakeSources { get; } = new[]
    {
        new EarthquakeSourceOption(EarthquakeSourceIds.Huania, "成都高新减灾研究所"),
        new EarthquakeSourceOption(EarthquakeSourceIds.Sichuan, "四川地震局"),
        new EarthquakeSourceOption(EarthquakeSourceIds.WolfxSceewHttp, "Wolfx API (四川地震局)"),
        new EarthquakeSourceOption(EarthquakeSourceIds.WolfxCencHttp, "Wolfx API (中国地震台网)"),
        new EarthquakeSourceOption(EarthquakeSourceIds.Miui, "MIUI 预警"),
        new EarthquakeSourceOption(EarthquakeSourceIds.CencHttp, "CENC HTTP 正式报 (中国地震台网)"),
        new EarthquakeSourceOption(EarthquakeSourceIds.CencRapidHttp, "CENC HTTP 速报 (中国地震台网)"),
        new EarthquakeSourceOption(EarthquakeSourceIds.WolfxWebSocket, "Wolfx WebSocket（实时）+ HTTP（查询）")
    };

    public bool IsPollingSource => UpdateSetting?.Setting?.SourceId != EarthquakeSourceIds.WolfxWebSocket;

    public SettingsPageViewModel(ISetting<CurrentPosition> currentPosition, ISetting<AlertLimit> alertLimit,
        ISetting<UpdaterSetting>? updateSetting, ISetting<TrackerSetting>? trackerSetting, ISetting<TimeSetting>? timeSetting, ISetting<GNSSSetting> gnssSetting, IServiceProvider provider)
    {
        CurrentPosition = currentPosition;
        AlertLimit = alertLimit;
        UpdateSetting = updateSetting;
        TrackerSetting = trackerSetting;
        TimeSetting = timeSetting;
        _provider = provider;
        GNSSSetting = gnssSetting;
        if (UpdateSetting?.Setting is not null)
            UpdateSetting.Setting.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName is nameof(UpdaterSetting.SourceId) or nameof(UpdaterSetting.ApiType))
                    OnPropertyChanged(nameof(IsPollingSource));
            };
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public sealed record EarthquakeSourceOption(string Id, string DisplayName);
