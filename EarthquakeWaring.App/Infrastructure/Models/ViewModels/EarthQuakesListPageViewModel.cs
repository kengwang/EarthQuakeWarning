using System.Collections.ObjectModel;
using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;

namespace EarthquakeWaring.App.Infrastructure.Models.ViewModels;

public class EarthQuakesListPageViewModel
{
    public ObservableCollection<EarthQuakeTrackingInformation> InformationList { get; } = new();
}