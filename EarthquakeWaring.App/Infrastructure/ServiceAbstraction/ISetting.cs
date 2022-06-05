namespace EarthquakeWaring.App.Infrastructure.ServiceAbstraction;

public interface ISetting<TSetting>
{
    public TSetting? Setting { get; }
}