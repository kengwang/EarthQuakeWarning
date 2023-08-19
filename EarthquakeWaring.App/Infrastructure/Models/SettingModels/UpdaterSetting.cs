using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EarthquakeWaring.App.Infrastructure.Models.SettingModels;

public class UpdaterSetting : INotificationOption
{
    private int _updateTimeSpanSecond = 5;
    private bool _showNotifyIcon = true;
    private int _apiType = 0;

    public bool ShowNotifyIcon
    {
        get => _showNotifyIcon;
        set => SetField(ref _showNotifyIcon, value);
    }

    public int UpdateTimeSpanSecond
    {
        get => _updateTimeSpanSecond;
        set => SetField(ref _updateTimeSpanSecond, value);
    }

    public int ApiType
    {
        get => _apiType;
        set => SetField(ref _apiType, value);
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