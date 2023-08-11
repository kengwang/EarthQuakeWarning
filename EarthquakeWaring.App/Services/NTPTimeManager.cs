using EarthquakeWaring.App.Infrastructure.Models.SettingModels;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using GuerrillaNtp;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Vanara.PInvoke;

namespace EarthquakeWaring.App.Services
{
    public class NTPTimeManager : INTPHandler
    {
        private ISetting<TimeSetting> _setting;
        private ILogger<NTPTimeManager> _logger;
        private ITimeHandler _timeHandler;
        private NtpClient _ntpClient;

        public string NTPServer { get; }
        public void GetNTPServerTime(object? sender, ElapsedEventArgs e)
        {
            _ = GetNTPServerTime();
        }
        public async Task<bool> GetNTPServerTime(CancellationToken ctk = default)
        {
            if (_setting.Setting!.UseGNSSTime) return false;
            try
            {
                var result = await _ntpClient.QueryAsync(ctk);
                if (result.Synchronized)
                {
                    if (_setting.Setting?.SetAccurateTimeToMachine ?? false != true)
                    {
                        _timeHandler.Offset = result.CorrectionOffset;
                    }
                    else
                    {
                        var sysTimeResult = TrySetSystemTime(result.Now.DateTime);
                        if (!sysTimeResult) _timeHandler.Offset = result.CorrectionOffset;
                    }
                    _timeHandler.LastUpdated = result.Now.DateTime;
                    return true;
                }
                else
                {
                    _timeHandler.Offset = TimeSpan.Zero;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Query Failed. NTP Server Is {NTPServer}");
                _timeHandler.Offset = TimeSpan.Zero;
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
        public NTPTimeManager(ISetting<TimeSetting> setting, ITimeHandler timeHandler, ILogger<NTPTimeManager> logger)
        {
            _setting = setting;
            _timeHandler = timeHandler;
            timeHandler.Timer.Elapsed += GetNTPServerTime;
            NTPServer = setting.Setting?.NTPServer ?? "ntp.ntsc.ac.cn";
            _logger = logger;
            _ntpClient = new NtpClient(NTPServer, TimeSpan.FromMilliseconds(500));
        }
    }
}
