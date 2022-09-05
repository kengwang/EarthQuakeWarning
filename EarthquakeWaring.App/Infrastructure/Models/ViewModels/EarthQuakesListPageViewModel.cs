using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;

namespace EarthquakeWaring.App.Infrastructure.Models.ViewModels;

public class EarthQuakesListPageViewModel : INotifyPropertyChanged
{
    private Visibility _progressRingVisibility;
    public ObservableCollection<EarthQuakeTrackingInformation> InformationList { get; } = new();

    public Visibility ProgressRingVisibility
    {
        get => _progressRingVisibility;
        set => SetField(ref _progressRingVisibility, value);
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