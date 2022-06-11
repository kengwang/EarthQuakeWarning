using System;
using System.ComponentModel;
using System.Globalization;
using System.Media;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using EarthquakeWaring.App.Extensions;
using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;
using EarthquakeWaring.App.Infrastructure.Models.SettingModels;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using EarthquakeWaring.App.Pages;
using EarthquakeWaring.App.Services;
using Microsoft.Extensions.DependencyInjection;
using static System.Threading.Tasks.Task;

namespace EarthquakeWaring.App.Windows;

public partial class EarlyWarningWindow : Window
{
    private readonly EarthQuakeTrackingInformation _information;
    private readonly IServiceProvider _service;
    private readonly SpeechSynthesizer _speech;
    private Prompt? _lastPrompt;

    public EarlyWarningWindow(EarthQuakeTrackingInformation information, IServiceProvider service)
    {
        _information = information;
        _service = service;
        DataContext = information;
        InitializeComponent();
        _speech = new SpeechSynthesizer();
        _speech.SelectVoice(_speech.GetInstalledVoices(CultureInfo.InstalledUICulture)[0].VoiceInfo.Name);
        _speech.SetOutputToDefaultAudioDevice();
        var basicInfoSpeech = new SpeechSynthesizer();
        basicInfoSpeech.SelectVoice(_speech.GetInstalledVoices(CultureInfo.InstalledUICulture)[0].VoiceInfo.Name);
        basicInfoSpeech.SetOutputToDefaultAudioDevice();
        basicInfoSpeech.SpeakAsync(
            $"{information.Position} 发生地震，震级 {information.Magnitude:F1} 级" +
            new IntensityDescriptor().Convert(_information.Intensity, typeof(string), null, null));
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
            if (_information.CountDown <= 0)
            {
                _information.PropertyChanged -= InformationOnPropertyChanged;
                _speech.SpeakAsync("地震波已到达，" +
                                   new IntensityDescriptor().Convert(_information.Intensity, typeof(string), null,
                                       null));
                return;
            }

            _lastPrompt = new Prompt(_information.CountDown.ToString());
            _speech.SpeakAsync(_lastPrompt);
            var beepCount = _information.Stage switch
            {
                EarthQuakeStage.Emergency => 1,
                EarthQuakeStage.Forced => 2,
                _ => 0
            };
            for (var i = 0; i < beepCount; i++)
            {
                SystemSounds.Exclamation.Play();
                await Task.Delay(200);
            }
        });
    }

    private void ShowDetail(object sender, MouseButtonEventArgs e)
    {
        var mainWindow = _service.GetService<MainWindow>();
        mainWindow?.RootFrame.Navigate(new EarthQuakeDetail(_information,
            _service.GetService<ISetting<CurrentPosition>>()?.Setting));
        mainWindow?.Show();
    }

    private void EarlyWarningWindow_OnClosed(object? sender, EventArgs e)
    {
        _speech.SpeakAsyncCancelAll();
        _information.PropertyChanged -= InformationOnPropertyChanged;
    }
}