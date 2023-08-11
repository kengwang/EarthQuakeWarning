using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EarthquakeWaring.App.Infrastructure.Models.SettingModels
{
    public class GNSSSetting : INotificationOption
    {

        private string _port = "COM3";
        public string Port
        {
            get => _port;
            set
            {
                SetField(ref _port, value);
            }
        }
        private int _baud = 9600;
        public int Baud
        {
            get => _baud;
            set
            {
                SetField(ref _baud, value);
            }
        }
        private bool _useGNSS = false;
        public bool UseGNSS
        {
            get => _useGNSS;
            set
            {
                SetField(ref _useGNSS, value);
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
