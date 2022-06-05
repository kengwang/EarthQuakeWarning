using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;

namespace EarthquakeWaring.App.Infrastructure.Models.SettingModels;

public class CurrentPosition : INotificationOption
{
    private double _latitude = 30.539772;
    private double _longitude = 104.075426;

    public double Latitude
    {
        get => _latitude;
        set => SetField(ref _latitude, value);
    }

    public double Longitude
    {
        get => _longitude;
        set => SetField(ref _longitude, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}