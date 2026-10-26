using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace EarthquakeWaring.App.Infrastructure.Models.SettingModels;

public class UpdaterSetting : INotificationOption
{
    private int _updateTimeSpanSecond = 5;
    private bool _showNotifyIcon = true;
    private string? _sourceId;
    private int? _apiType;

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

    public string SourceId
    {
        get
        {
            if (_sourceId is not null) return _sourceId;
            _sourceId = EarthquakeSourceIds.FromLegacyIndex(_apiType ?? 0);
            _apiType = null;
            return _sourceId;
        }
        set
        {
            if (string.IsNullOrWhiteSpace(value)) value = EarthquakeSourceIds.Huania;
            _apiType = null;
            SetField(ref _sourceId, value);
        }
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ApiType
    {
        get => _sourceId is null ? _apiType : null;
        set
        {
            if (_sourceId is not null) return;
            if (SetField(ref _apiType, value)) OnPropertyChanged(nameof(SourceId));
        }
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
