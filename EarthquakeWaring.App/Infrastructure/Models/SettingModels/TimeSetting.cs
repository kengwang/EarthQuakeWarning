using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EarthquakeWaring.App.Infrastructure.Models.SettingModels
{
    public class TimeSetting : INotificationOption
    {

        private string _ntpServer = "ntp.ntsc.ac.cn";
        public string NTPServer
        {
            get => _ntpServer;
            set
            {
                SetField(ref _ntpServer, value);
            }
        }
        private bool _setAccurateTimeToMachine = false;
        public bool SetAccurateTimeToMachine
        {
            get => _setAccurateTimeToMachine;
            set => SetField(ref _setAccurateTimeToMachine, value);
        }
        private double _timeCheckInterval = 30;
        public double TimeCheckInterval
        {
            get => _timeCheckInterval;
            set
            {
                SetField(ref _timeCheckInterval, value);
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
}
