﻿using EarthquakeWaring.App.Infrastructure.Models.SettingModels;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace EarthquakeWaring.App.Infrastructure.Models.ViewModels;

public class SettingsPageViewModel
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
    }
}