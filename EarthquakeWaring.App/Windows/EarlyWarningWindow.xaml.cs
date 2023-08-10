using EarthquakeWaring.App.Extensions;
using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;
using EarthquakeWaring.App.Infrastructure.Models.SettingModels;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using EarthquakeWaring.App.Pages;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Media;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static System.Threading.Tasks.Task;

namespace EarthquakeWaring.App.Windows;

public partial class EarlyWarningWindow : Window
{
    private readonly EarthQuakeTrackingInformation _information;
    private readonly IServiceProvider _service;
    private readonly SpeechSynthesizer _speech;
    private readonly IntensityDescriptor _intensityDescriptor = new();
    private readonly ISetting<TrackerSetting> _trackerSetting;
    private string _lastDescription = null!;
    private Prompt? _lastPrompt;
    private SpeechSynthesizer? intensitySpeech;
    private SpeechSynthesizer? tipSpeech;
    private IVolumeManager? _volumeManager;

    public EarlyWarningWindow(EarthQuakeTrackingInformation information, IServiceProvider service)
    {
        _information = information;
        _service = service;
        _trackerSetting = _service.GetRequiredService<ISetting<TrackerSetting>>();
        _volumeManager = _service.GetRequiredService<IVolumeManager>();
        DataContext = information;
        InitializeComponent();
        if (_trackerSetting?.Setting?.MaximumVolume is not false)
            _volumeManager?.SetVolumeToMax();
        _speech = new SpeechSynthesizer();
        _speech.SelectVoice(_speech.GetInstalledVoices(CultureInfo.InstalledUICulture)[0].VoiceInfo.Name);
        _speech.SetOutputToDefaultAudioDevice();
        if (_trackerSetting.Setting?.BroadcastLiveIntensity is true)
        {
            intensitySpeech = new SpeechSynthesizer();
            intensitySpeech.SelectVoice(_speech.GetInstalledVoices(CultureInfo.InstalledUICulture)[0].VoiceInfo.Name);
            intensitySpeech.SetOutputToDefaultAudioDevice();
        }

        if (_trackerSetting.Setting?.BroadcastLiveTips is true)
        {
            tipSpeech = new SpeechSynthesizer();
            tipSpeech.SelectVoice(_speech.GetInstalledVoices(CultureInfo.InstalledUICulture)[0].VoiceInfo.Name);
            tipSpeech.SetOutputToDefaultAudioDevice();
        }

        if (_trackerSetting.Setting?.BroadcastEarthQuakeInformation is true)
        {
            var basicInfoSpeech = new SpeechSynthesizer();
            basicInfoSpeech.SelectVoice(_speech.GetInstalledVoices(CultureInfo.InstalledUICulture)[0].VoiceInfo.Name);
            basicInfoSpeech.SetOutputToDefaultAudioDevice();
            basicInfoSpeech.SpeakAsync(
                $"{information.Position} 发生 {information.Magnitude:F1} 级 地震" +
                _intensityDescriptor.Convert(_information.Intensity, typeof(string), null!, null!));
        }

        _information.PropertyChanged += InformationOnPropertyChanged;
    }

    private void InformationOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EarthQuakeTrackingInformation.CountDown))
        {
            SpeakNowCountDown();
        }
    }

    private void SpeakNowCountDown()
    {
        Run(async () =>
        {
            if (_lastPrompt != null) _speech.SpeakAsyncCancel(_lastPrompt);
            if (_information.CountDown <= 0 && _trackerSetting.Setting?.BroadcastCountDown is true)
            {
                _information.PropertyChanged -= InformationOnPropertyChanged;
                _speech.SpeakAsync("地震到达，" +
                                   _intensityDescriptor.Convert(_information.Intensity, typeof(string), null!,
                                       null!));
                return;
            }

            if (_trackerSetting.Setting?.BroadcastCountDown is true)
            {
                _lastPrompt = new Prompt(_information.CountDown.ToString());
                _speech.SpeakAsync(_lastPrompt);
            }

            var beepCount = _information.Stage switch
            {
                EarthQuakeStage.Emergency => 1,
                EarthQuakeStage.Forced => 2,
                _ => 0
            };
            if (_trackerSetting.Setting?.BroadcastLiveIntensity is true)
            {
                var currentIntensityDescription =
                    _intensityDescriptor.Convert(_information.Intensity, typeof(string), null!, null!) as string ??
                    "未知";
                if (currentIntensityDescription != _lastDescription)
                {
                    _lastDescription = currentIntensityDescription;
                    intensitySpeech?.SpeakAsync(new Prompt(_lastDescription));
                }
            }

            if (_trackerSetting.Setting?.BroadcastLiveTips is true && _information.CountDown % 5 == 0)
            {
                switch (_information.Intensity)
                {
                    case >= 3 and <= 4:
                        tipSpeech?.SpeakAsync(new Prompt("躲避悬挂"));
                        break;
                    case > 4:
                        tipSpeech?.SpeakAsync(new Prompt("迅速逃生"));
                        break;
                }
            }

            for (var i = 0; i < beepCount; i++)
            {
                SystemSounds.Exclamation.Play();
                await Task.Delay(200);
            }
        });
    }

    private void ShowDetail(object sender, MouseButtonEventArgs e)
    {
        /*
        var mainWindow = _service.GetService<MainWindow>();
        mainWindow?.RootFrame.Navigate(new EarthQuakeDetail(_information,
            _service.GetService<ISetting<CurrentPosition>>()?.Setting));
        mainWindow?.Show();
        */
        if (!App.MainWindowOpened)
        {
            _service.GetService<MainWindow>()?.Show();
        }

        if (App.MainWindowOpened)
        {
            App.RootFrame?.Navigate(new EarthQuakeDetail(_information,
                _service.GetService<ISetting<CurrentPosition>>()?.Setting));
        }
    }

    private void EarlyWarningWindow_OnClosed(object? sender, EventArgs e)
    {
        _speech.SpeakAsyncCancelAll();
        _information.PropertyChanged -= InformationOnPropertyChanged;
    }
}