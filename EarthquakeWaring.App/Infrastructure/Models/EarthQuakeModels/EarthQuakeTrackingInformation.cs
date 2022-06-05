using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;

public class EarthQuakeTrackingInformation : INotifyPropertyChanged
{
    private DateTime _startTime;
    private string _position = "未知地区";
    private double _magnitude;
    private double _intensity;
    private int _countDown;
    private DateTime _updateTime;
    private long _eventId;
    private double _latitude;
    private double _longitude;
    private double _depth;
    private long _sations;
    private double _distance;
    private int _theoryCountDown;
    private EarthQuakeStage _stage;

    public EarthQuakeStage Stage
    {
        get => _stage;
        set => SetField(ref _stage, value);
    }

    public double Distance
    {
        get => _distance;
        set => SetField(ref _distance, value);
    }

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

    public double Depth
    {
        get => _depth;
        set => SetField(ref _depth, value);
    }

    public long Sations
    {
        get => _sations;
        set => SetField(ref _sations, value);
    }


    public long EventId
    {
        get => _eventId;
        set => SetField(ref _eventId, value);
    }


    public DateTime UpdateTime
    {
        get => _updateTime;
        set => SetField(ref _updateTime, value);
    }

    public DateTime StartTime
    {
        get => _startTime;
        set => SetField(ref _startTime, value);
    }

    public string Position
    {
        get => _position;
        set => SetField(ref _position, value);
    }

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

    public int TheoryCountDown
    {
        get => _theoryCountDown;
        set => SetField(ref _theoryCountDown, value);
    }

    public int CountDown
    {
        get => _countDown;
        set => SetField(ref _countDown, value);
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



    public string Description
    {
        get
        {
            if (Intensity < 1) return Intensity > 0 ? "当前位置可能有震感" : "当前位置无震感";
            return $"预警时间 {TheoryCountDown} 秒 | 预估烈度 {Intensity.ToString("F1")} 度";
        }
    }
}