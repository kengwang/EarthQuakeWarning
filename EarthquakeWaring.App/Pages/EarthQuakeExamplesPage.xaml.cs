using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EarthquakeWaring.App.Infrastructure.Models.ApiModels;
using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;
using EarthquakeWaring.App.Infrastructure.Models.SettingModels;
using EarthquakeWaring.App.Infrastructure.Models.ViewModels;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using EarthquakeWaring.App.Services;
using EarthquakeWaring.App.Windows;
using Microsoft.Extensions.DependencyInjection;
using Button = WPFUI.Controls.Button;

namespace EarthquakeWaring.App.Pages;

public partial class EarthQuakeExamplesPage : Page
{
    private readonly ISetting<CurrentPosition> _currentPosition;
    private readonly ISetting<AlertLimit> _alertLimit;
    private readonly IEarthQuakeApi _quakeApi;
    private readonly IEarthQuakeCalculator _calculator;
    private readonly IServiceProvider _service;
    private readonly IJsonConvertService _jsonConvert;
    private readonly EarthQuakesListPageViewModel _viewModel;


    private const string Examples =
        @"[{""eventId"":4147076,""startAt"":1401441614000,""longitude"":97.856,""latitude"":25.007,""depth"":10,""magnitude"":5.2,""updateAt"":1401441621000,""epicenter"":""云南盈江"",""sations"":1,""insideNet"":0},{""eventId"":4147076,""startAt"":1401441614000,""longitude"":97.856,""latitude"":25.007,""depth"":10,""magnitude"":5.8,""updateAt"":1401441625000,""epicenter"":""云南盈江"",""sations"":2,""insideNet"":0},{""eventId"":4147076,""startAt"":1401441614000,""longitude"":97.856,""latitude"":25.007,""depth"":10,""magnitude"":6,""updateAt"":1401441627000,""epicenter"":""云南盈江"",""sations"":3,""insideNet"":0},{""eventId"":4147076,""startAt"":1401441614000,""longitude"":97.856,""latitude"":25.007,""depth"":10,""magnitude"":6.1,""updateAt"":1401441629000,""epicenter"":""云南盈江"",""sations"":4,""insideNet"":0},{""eventId"":4147076,""startAt"":1401441614000,""longitude"":97.856,""latitude"":25.007,""depth"":10,""magnitude"":6.2,""updateAt"":1401441633000,""epicenter"":""云南盈江"",""sations"":5,""insideNet"":0},{""eventId"":5032996,""startAt"":1407083411000,""longitude"":103.338,""latitude"":27.113,""depth"":10,""magnitude"":4.5,""updateAt"":1407083418000,""epicenter"":""云南鲁甸"",""sations"":1,""insideNet"":0},{""eventId"":5032996,""startAt"":1407083411000,""longitude"":103.338,""latitude"":27.113,""depth"":10,""magnitude"":4.8,""updateAt"":1407083420000,""epicenter"":""云南鲁甸"",""sations"":2,""insideNet"":0},{""eventId"":5032996,""startAt"":1407083411000,""longitude"":103.338,""latitude"":27.113,""depth"":10,""magnitude"":5,""updateAt"":1407083422000,""epicenter"":""云南鲁甸"",""sations"":3,""insideNet"":0},{""eventId"":5032996,""startAt"":1407083411000,""longitude"":103.338,""latitude"":27.113,""depth"":10,""magnitude"":5.4,""updateAt"":1407083425000,""epicenter"":""云南鲁甸"",""sations"":4,""insideNet"":0},{""eventId"":5032996,""startAt"":1407083411000,""longitude"":103.338,""latitude"":27.113,""depth"":10,""magnitude"":5.9,""updateAt"":1407083427000,""epicenter"":""云南鲁甸"",""sations"":5,""insideNet"":0},{""eventId"":5032996,""startAt"":1407083411000,""longitude"":103.338,""latitude"":27.113,""depth"":10,""magnitude"":6,""updateAt"":1407083432000,""epicenter"":""云南鲁甸"",""sations"":6,""insideNet"":0},{""eventId"":5580100,""startAt"":1410028661000,""longitude"":115.432,""latitude"":40.282,""depth"":10,""magnitude"":3.8,""updateAt"":1410028667000,""epicenter"":""河北涿鹿"",""sations"":1,""insideNet"":0},{""eventId"":5580100,""startAt"":1410028661000,""longitude"":115.432,""latitude"":40.282,""depth"":10,""magnitude"":3.9,""updateAt"":1410028668000,""epicenter"":""河北涿鹿"",""sations"":2,""insideNet"":0},{""eventId"":5580100,""startAt"":1410028661000,""longitude"":115.432,""latitude"":40.282,""depth"":10,""magnitude"":4.2,""updateAt"":1410028669000,""epicenter"":""河北涿鹿"",""sations"":3,""insideNet"":0},{""eventId"":5580100,""startAt"":1410028661000,""longitude"":115.432,""latitude"":40.282,""depth"":10,""magnitude"":4.3,""updateAt"":1410028674000,""epicenter"":""河北涿鹿"",""sations"":4,""insideNet"":0},{""eventId"":5580100,""startAt"":1410028661000,""longitude"":115.432,""latitude"":40.282,""depth"":10,""magnitude"":4.4,""updateAt"":1410028676000,""epicenter"":""河北涿鹿"",""sations"":5,""insideNet"":0},{""eventId"":5580100,""startAt"":1410028661000,""longitude"":115.432,""latitude"":40.282,""depth"":10,""magnitude"":4.5,""updateAt"":1410028679000,""epicenter"":""河北涿鹿"",""sations"":6,""insideNet"":0},{""eventId"":6781692,""startAt"":1416034995000,""longitude"":103.729,""latitude"":37.099,""depth"":8,""magnitude"":3.4,""updateAt"":1416035004000,""epicenter"":""甘肃景泰"",""sations"":1,""insideNet"":0},{""eventId"":6781692,""startAt"":1416034995000,""longitude"":103.729,""latitude"":37.099,""depth"":8,""magnitude"":3.8,""updateAt"":1416035005000,""epicenter"":""甘肃景泰"",""sations"":2,""insideNet"":0},{""eventId"":6781692,""startAt"":1416034995000,""longitude"":103.748,""latitude"":37.08,""depth"":8,""magnitude"":3.9,""updateAt"":1416035010000,""epicenter"":""甘肃景泰"",""sations"":3,""insideNet"":0},{""eventId"":6781692,""startAt"":1416034995000,""longitude"":103.748,""latitude"":37.08,""depth"":8,""magnitude"":4.1,""updateAt"":1416035011000,""epicenter"":""甘肃景泰"",""sations"":4,""insideNet"":0},{""eventId"":6781692,""startAt"":1416034995000,""longitude"":103.748,""latitude"":37.08,""depth"":8,""magnitude"":4.2,""updateAt"":1416035016000,""epicenter"":""甘肃景泰"",""sations"":5,""insideNet"":0},{""eventId"":6968774,""startAt"":1416675327000,""longitude"":101.649,""latitude"":30.249,""depth"":8,""magnitude"":4.5,""updateAt"":1416675334000,""epicenter"":""四川康定"",""sations"":1,""insideNet"":0},{""eventId"":6968774,""startAt"":1416675325000,""longitude"":101.644,""latitude"":30.356,""depth"":10.834,""magnitude"":4.9,""updateAt"":1416675335000,""epicenter"":""四川康定"",""sations"":2,""insideNet"":0},{""eventId"":6968774,""startAt"":1416675326000,""longitude"":101.652,""latitude"":30.271,""depth"":8,""magnitude"":5.2,""updateAt"":1416675337000,""epicenter"":""四川康定"",""sations"":3,""insideNet"":0},{""eventId"":6968774,""startAt"":1416675326000,""longitude"":101.652,""latitude"":30.271,""depth"":8,""magnitude"":5.5,""updateAt"":1416675340000,""epicenter"":""四川康定"",""sations"":4,""insideNet"":0},{""eventId"":6968774,""startAt"":1416675326000,""longitude"":101.652,""latitude"":30.271,""depth"":8,""magnitude"":5.8,""updateAt"":1416675341000,""epicenter"":""四川康定"",""sations"":5,""insideNet"":0},{""eventId"":6968774,""startAt"":1416675326000,""longitude"":101.652,""latitude"":30.271,""depth"":8,""magnitude"":6,""updateAt"":1416675342000,""epicenter"":""四川康定"",""sations"":6,""insideNet"":0},{""eventId"":6968774,""startAt"":1416675326000,""longitude"":101.652,""latitude"":30.271,""depth"":8,""magnitude"":6.3,""updateAt"":1416675345000,""epicenter"":""四川康定"",""sations"":7,""insideNet"":0},{""eventId"":6968774,""startAt"":1416675326000,""longitude"":101.652,""latitude"":30.271,""depth"":8,""magnitude"":6.4,""updateAt"":1416675345000,""epicenter"":""四川康定"",""sations"":8,""insideNet"":0},{""eventId"":10029953,""startAt"":1429112368000,""longitude"":106.416,""latitude"":39.797,""depth"":9.551,""magnitude"":4.1,""updateAt"":1429112380000,""epicenter"":""内蒙古阿拉善左旗"",""sations"":1,""insideNet"":0},{""eventId"":10029953,""startAt"":1429112368000,""longitude"":106.416,""latitude"":39.797,""depth"":9.551,""magnitude"":4.4,""updateAt"":1429112380000,""epicenter"":""内蒙古阿拉善左旗"",""sations"":2,""insideNet"":0},{""eventId"":10029953,""startAt"":1429112368000,""longitude"":106.415,""latitude"":39.797,""depth"":9.019,""magnitude"":5.1,""updateAt"":1429112381000,""epicenter"":""内蒙古阿拉善左旗"",""sations"":3,""insideNet"":0},{""eventId"":10029953,""startAt"":1429112368000,""longitude"":106.415,""latitude"":39.797,""depth"":9.019,""magnitude"":5.4,""updateAt"":1429112396000,""epicenter"":""内蒙古阿拉善左旗"",""sations"":4,""insideNet"":0},{""eventId"":10029953,""startAt"":1429112368000,""longitude"":106.415,""latitude"":39.797,""depth"":9.019,""magnitude"":5.8,""updateAt"":1429112397000,""epicenter"":""内蒙古阿拉善左旗"",""sations"":5,""insideNet"":0},{""eventId"":10029953,""startAt"":1429112368000,""longitude"":106.415,""latitude"":39.797,""depth"":9.019,""magnitude"":6,""updateAt"":1429112400000,""epicenter"":""内蒙古阿拉善左旗"",""sations"":6,""insideNet"":0},{""eventId"":25403257,""startAt"":1502227188000,""longitude"":103.789,""latitude"":33.21,""depth"":8,""magnitude"":4.8,""updateAt"":1502227202000,""epicenter"":""四川九寨沟"",""sations"":1,""insideNet"":0},{""eventId"":25403257,""startAt"":1502227188000,""longitude"":103.789,""latitude"":33.21,""depth"":8,""magnitude"":5,""updateAt"":1502227203000,""epicenter"":""四川九寨沟"",""sations"":2,""insideNet"":0},{""eventId"":25403257,""startAt"":1502227188000,""longitude"":103.802,""latitude"":33.215,""depth"":8,""magnitude"":5.3,""updateAt"":1502227214000,""epicenter"":""四川九寨沟"",""sations"":3,""insideNet"":0},{""eventId"":25403257,""startAt"":1502227188000,""longitude"":103.802,""latitude"":33.215,""depth"":8,""magnitude"":5.8,""updateAt"":1502227217000,""epicenter"":""四川九寨沟"",""sations"":4,""insideNet"":0},{""eventId"":25403257,""startAt"":1502227188000,""longitude"":103.802,""latitude"":33.215,""depth"":8,""magnitude"":5.9,""updateAt"":1502227218000,""epicenter"":""四川九寨沟"",""sations"":5,""insideNet"":0},{""eventId"":25403257,""startAt"":1502227188000,""longitude"":103.802,""latitude"":33.215,""depth"":8,""magnitude"":6,""updateAt"":1502227219000,""epicenter"":""四川九寨沟"",""sations"":6,""insideNet"":0},{""eventId"":35361266,""startAt"":1560812144000,""longitude"":104.886,""latitude"":28.356,""depth"":8,""magnitude"":5.4,""updateAt"":1560812151000,""epicenter"":""四川长宁"",""sations"":1,""insideNet"":0},{""eventId"":35361266,""startAt"":1560812144000,""longitude"":104.886,""latitude"":28.356,""depth"":8,""magnitude"":6,""updateAt"":1560812151000,""epicenter"":""四川长宁"",""sations"":2,""insideNet"":0},{""eventId"":35361266,""startAt"":1560812144000,""longitude"":104.882,""latitude"":28.362,""depth"":8,""magnitude"":6.3,""updateAt"":1560812152000,""epicenter"":""四川长宁"",""sations"":3,""insideNet"":0},{""eventId"":35361266,""startAt"":1560812144000,""longitude"":104.882,""latitude"":28.362,""depth"":8,""magnitude"":6,""updateAt"":1560812160000,""epicenter"":""四川长宁"",""sations"":4,""insideNet"":0},{""eventId"":35361266,""startAt"":1560812144000,""longitude"":104.882,""latitude"":28.362,""depth"":8,""magnitude"":6.1,""updateAt"":1560812163000,""epicenter"":""四川长宁"",""sations"":5,""insideNet"":0}]";

    public EarthQuakeExamplesPage(IServiceProvider service, IEarthQuakeCalculator calculator,
        ISetting<CurrentPosition> currentPosition, ISetting<AlertLimit> alertLimit, IEarthQuakeApi quakeApi,
        IJsonConvertService jsonConvert)
    {
        _service = service;
        _calculator = calculator;
        _currentPosition = currentPosition;
        _alertLimit = alertLimit;
        _quakeApi = quakeApi;
        _jsonConvert = jsonConvert;
        _viewModel = new EarthQuakesListPageViewModel();
        InitializeComponent();
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        var results = _jsonConvert.ConvertTo<List<HuaniaEarthQuake>>(Examples);
        results = results?.GroupBy(t => t.EventId).Select(g => g.Last()).ToList();
        results?.ForEach(t => _viewModel.InformationList.Add(ConvertToInformation(t)));
        ListView.ItemsSource = _viewModel.InformationList;
    }

    private EarthQuakeTrackingInformation ConvertToInformation(HuaniaEarthQuake latestInfo)
    {
        var trackingInformation = new EarthQuakeTrackingInformation
        {
            // Update the tracking information
            Position = latestInfo.Epicenter,
            StartTime = latestInfo.StartAt.AddHours(16),
            UpdateTime = latestInfo.UpdateAt.AddHours(16),
            Depth = latestInfo.Depth,
            Latitude = latestInfo.Latitude,
            Longitude = latestInfo.Longitude,
            EventId = latestInfo.EventId,
            Sations = latestInfo.Sations,
            Magnitude = latestInfo.Magnitude
        };

        if (_currentPosition.Setting == null) return trackingInformation;
        trackingInformation.Distance = _calculator.GetDistance(_currentPosition.Setting.Latitude,
            _currentPosition.Setting.Longitude, trackingInformation.Latitude, trackingInformation.Longitude);
        trackingInformation.TheoryCountDown =
            (int)_calculator.GetCountDownSeconds(trackingInformation.Depth,
                trackingInformation.Distance);
        trackingInformation.Intensity =
            _calculator.GetIntensity(trackingInformation.Magnitude, trackingInformation.Distance);
        trackingInformation.Stage = EarthQuakeTracker.GetEarthQuakeAlertStage(trackingInformation);
        trackingInformation.DontUseShouldAlert =
            EarthQuakeTracker.ShouldPopupAlert(trackingInformation, _alertLimit.Setting ?? new AlertLimit());
        return trackingInformation;
    }


    private void SimulateEarthQuake(object sender, RoutedEventArgs e)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var info = (((sender as Button)?.Tag as EarthQuakeTrackingInformation)!);
        var tracker = _service.GetService<IEarthQuakeTracker>();
        tracker!.SimulateTimeSpan = DateTime.Now - info.StartTime;
        var results = _jsonConvert.ConvertTo<List<EarthQuakeUpdate>>(Examples);
        tracker!.SimulateUpdates = results?.Where(t => t.EventId == info.EventId).Select(t =>
        {
            t.StartAt = t.StartAt.AddHours(16);
            t.UpdateAt = t.UpdateAt.AddHours(16);
            return t;
        }).ToList();
        tracker?.StartTrack(new HuaniaEarthQuake()
        {
            EventId = info.EventId
        }, cancellationTokenSource);
    }

    private void ShowEarthQuakeDetail(object sender, MouseButtonEventArgs e)
    {
        if (((Grid)sender).Tag is EarthQuakeTrackingInformation info)
        {
            _service.GetService<MainWindow>()?.RootFrame.Navigate(new EarthQuakeDetail(info, _currentPosition.Setting));
        }
    }
}