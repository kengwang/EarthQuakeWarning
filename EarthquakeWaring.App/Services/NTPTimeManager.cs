using EarthquakeWaring.App.Infrastructure.Models.SettingModels;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using GuerrillaNtp;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Vanara.PInvoke;

namespace EarthquakeWaring.App.Services
{
    public class NTPTimeManager : INTPHandler
    {
        public TimeSpan Offset => _offset;
        private  TimeSpan _offset = TimeSpan.Zero;
        private ISetting<TimeSetting> _setting;
        private ILogger<NTPTimeManager> _logger;
        private Timer _timer;
        private NtpClient _ntpClient;

        public string NTPServer { get;}
        public void GetNTPServerTime(object? sender, ElapsedEventArgs e)
        {
            _ = GetNTPServerTime();
        }
        public async Task<bool> GetNTPServerTime()
        {
            try
            {
                var result = await _ntpClient.QueryAsync();
                if(result.Synchronized)
                {
                    if (_setting.Setting?.SetNTPTimeToMachine ?? false != true)
                    {
                        _offset = result.CorrectionOffset;                       
                    }
                    else
                    {
                        var sysTimeResult = TrySetSystemTime(result.Now.LocalDateTime);
                        if (!sysTimeResult) _offset = result.CorrectionOffset;
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,$"Query Failed. NTP Server Is {NTPServer}");
                return false;
            }
            return false;
        }

        public bool TrySetSystemTime(DateTime dateTime)
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
                if(isAdmin)
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
        public NTPTimeManager(ISetting<TimeSetting> setting, ILogger<NTPTimeManager> logger)
        {
            _setting = setting;
            NTPServer = setting.Setting?.NTPServer ?? "cn.ntp.org.cn";
            _logger = logger;
            _ntpClient = new NtpClient(NTPServer, TimeSpan.FromMilliseconds(500));
            var interval = TimeSpan.FromMinutes(setting.Setting?.NTPTimeInterval ?? 30d);
            _timer = new Timer(interval.TotalMilliseconds);
            _timer.AutoReset = true;
            _timer.Elapsed += GetNTPServerTime;
            _timer.Start();            
        }
    }
}
