using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;

namespace EarthquakeWaring.App.Infrastructure.Models.SettingModels;

public class AlertLimit : INotificationOption
{
    private double _magnitude;
    private double _intensity;

    public double Magnitude
    {
        get => _magnitude;
        set => SetField(ref _magnitude, value);
    }

    public double Intensity
    {
        get => _intensity;
        set => SetField(ref _intensity, value);
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