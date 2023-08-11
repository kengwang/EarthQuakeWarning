using EarthquakeWaring.App.Infrastructure.Models.SettingModels;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using Microsoft.Extensions.Logging;
using NmeaParser;
using NmeaParser.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using Vanara.PInvoke;

namespace EarthquakeWaring.App.Services
{
    public class GNSSManager : IGNSSHandler
    {
        private bool _nmeaDeviceShouldOpen = false;
        private ILogger<GNSSManager> _logger;
        private ISetting<TimeSetting> _timeSetting;
        private ISetting<CurrentPosition> _positionSetting;
        private ISetting<GNSSSetting> _gnssSetting;
        private ITimeHandler _timeHandler;
        public NmeaDevice? NMEADevice { get; set; }

        public async Task<bool> GetCurrentInfoAsync(CancellationToken token = default)
        {
            if (!_gnssSetting.Setting!.UseGNSS) return false;
            if (NMEADevice?.IsOpen is true) return false;
            var baud = _gnssSetting.Setting!.Baud;
            var port = _gnssSetting.Setting!.Port;
            var contains = SerialPort.GetPortNames().Contains(port);
            if (!contains) return false;
            _nmeaDeviceShouldOpen = true;
            NMEADevice = new SerialPortDevice(new SerialPort(port,baud));
            var result = await OpenNMEADevice(token);
            return result;
        }
        private async Task<bool> OpenNMEADevice(CancellationToken cts)
        {
            try
            {
                if (NMEADevice is null) return false;
                await NMEADevice.OpenAsync();
                NMEADevice.MessageReceived += NMEADevice_MessageReceived;
                while (_nmeaDeviceShouldOpen)
                {
                    if (cts.IsCancellationRequested)
                    {
                        return false;
                    }
                    await Task.Delay(500);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot Use Serial Port");
                return false;
            }
            finally
            {
                await Task.Delay(1000);
                _nmeaDeviceShouldOpen = false;
                NMEADevice?.CloseAsync();
                NMEADevice?.Dispose();
                NMEADevice = null;
            }
            
        }
        public bool TrySetSystemTime(DateTime dateTime)
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
                if (isAdmin)
                {
                    var systemTime = new SYSTEMTIME()
                    {
                        wYear = (ushort)dateTime.Year,
                        wMonth = (ushort)dateTime.Month,
                        DayOfWeek = dateTime.DayOfWeek,
                        wDayOfWeek = (ushort)dateTime.DayOfWeek,
                        wDay = (ushort)dateTime.Day,
                        wHour = (ushort)dateTime.Hour,
                        wMinute = (ushort)dateTime.Minute,
                        wSecond = (ushort)dateTime.Second,
                        wMilliseconds = (ushort)dateTime.Millisecond
                    };
                    Kernel32.SetLocalTime(systemTime);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Set LocalTime Failed.");
                return false;
            }
        }
        private void NMEADevice_MessageReceived(object? sender, NmeaMessageReceivedEventArgs e)
        {
            if(e.Message is Rmc message)
            {
                if (message.Active)
                {
                    if (_timeSetting.Setting!.UseGNSSTime)
                    {
                        var time = message.FixTime.LocalDateTime;
                        var result = TrySetSystemTime(time);
                        TimeSpan offset;
                        if (!result)
                        {
                            offset = time - DateTime.Now;
                        }
                        else
                        {
                            offset = TimeSpan.Zero;
                        }
                        _timeHandler.Offset = offset;
                    }
                    var longitude = Math.Round(message.Longitude, 6);
                    var latitude = Math.Round(message.Latitude, 6);
                    _positionSetting.Setting!.Longitude = longitude;
                    _positionSetting.Setting!.Latitude = latitude;
                    _timeHandler.LastUpdated = message.FixTime.LocalDateTime;
                    if (NMEADevice is not null) NMEADevice.MessageReceived -= NMEADevice_MessageReceived;
                    _nmeaDeviceShouldOpen = false; 
                }
            }
        }
        private void OnTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            _ = GetCurrentInfoAsync();
        }
        public GNSSManager(ISetting<TimeSetting> timeSetting, ISetting<CurrentPosition> positionSetting, ITimeHandler timeHandler,
                ISetting<GNSSSetting> gnssSetting, ILogger<GNSSManager> logger)
        {
            _timeHandler = timeHandler;
            _timeSetting = timeSetting;
            _positionSetting = positionSetting;
            _gnssSetting = gnssSetting;
            _logger = logger;
            timeHandler.Timer.Elapsed += OnTimerElapsed;
        }
    }
}
