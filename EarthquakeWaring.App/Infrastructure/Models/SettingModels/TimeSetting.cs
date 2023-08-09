using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EarthquakeWaring.App.Infrastructure.Models.SettingModels
{
    public class TimeSetting : INotificationOption
    {

        private string _ntpServer = "cn.ntp.org.cn";
        public string NTPServer
        {
            get => _ntpServer;
            set
            {
                SetField(ref _ntpServer, value);
            }
        }
        private bool _setNTPTimeToMachine = false;
        public bool SetNTPTimeToMachine
        {
            get => _setNTPTimeToMachine;
            set
            {
                SetField(ref _setNTPTimeToMachine, value);
            }
        }
        private double _ntpTimeInterval = 30;
        public double NTPTimeInterval
        {
            get => _ntpTimeInterval;
            set
            {
                SetField(ref _ntpTimeInterval, value);
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
