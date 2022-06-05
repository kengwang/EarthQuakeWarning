using System;
using System.ComponentModel;
using System.Globalization;
using System.Speech.Synthesis;
using System.Timers;
using System.Windows;
using EarthquakeWaring.App.Extensions;
using EarthquakeWaring.App.Infrastructure.Models;
using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;
using EarthquakeWaring.App.Services;
using static System.Threading.Tasks.Task;

namespace EarthquakeWaring.App.Windows;

public partial class EarlyWarningWindow : Window
{
    private readonly EarthQuakeTrackingInformation _information;
    private readonly SpeechSynthesizer _speech;
    private Prompt? _lastPrompt = null;

    public EarlyWarningWindow(EarthQuakeTrackingInformation information)
    {
        _information = information;
        DataContext = information;
        InitializeComponent();
        _speech = new SpeechSynthesizer();
        _speech.SelectVoice(_speech.GetInstalledVoices(CultureInfo.InstalledUICulture)[0].VoiceInfo.Name);
        _speech.SetOutputToDefaultAudioDevice();
        _speech.SpeakAsync($"{information.Position} 发生地震，震级 {information.Magnitude:F1} 级");
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
        Run(() =>
        {
            if (_lastPrompt != null) _speech.SpeakAsyncCancel(_lastPrompt);
            if (_information.CountDown <= 0)
            {
                _information.PropertyChanged -= InformationOnPropertyChanged;
                _speech.SpeakAsync("地震波已到达，" + new IntensityDescriptor().Convert(_information.Intensity,typeof(string),null,null));
                return;
            }

            _lastPrompt = new Prompt(_information.CountDown.ToString());
            _speech.SpeakAsync(_lastPrompt);
        });
    }
}