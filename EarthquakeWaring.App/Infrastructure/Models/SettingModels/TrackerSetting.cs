using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EarthquakeWaring.App.Infrastructure.Models.SettingModels;

public class TrackerSetting : INotificationOption
{
    private int _trackerTimeSpanMillisecond = 5;
    private bool _broadcastEarthQuakeInformation = true;
    private bool _broadcastLiveIntensity;
    private bool _broadcastLiveTips;
    private bool _broadcastCountDown = true;
    private bool _maximumVolume = true;

    public bool MaximumVolume
    {
        get => _maximumVolume;
        set => SetField(ref _maximumVolume, value);
    }

    public bool BroadcastCountDown
    {
        get => _broadcastCountDown;
        set => SetField(ref _broadcastCountDown, value);
    }

    public int TrackerTimeSpanMillisecond
    {
        get => _trackerTimeSpanMillisecond;
        set => SetField(ref _trackerTimeSpanMillisecond, value);
    }

    public bool BroadcastEarthQuakeInformation
    {
        get => _broadcastEarthQuakeInformation;
        set => SetField(ref _broadcastEarthQuakeInformation, value);
    }

    public bool BroadcastLiveIntensity
    {
        get => _broadcastLiveIntensity;
        set => SetField(ref _broadcastLiveIntensity, value);
    }

    public bool BroadcastLiveTips
    {
        get => _broadcastLiveTips;
        set => SetField(ref _broadcastLiveTips, value);
    }


    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}