namespace EarthquakeWaring.App.Infrastructure.ServiceAbstraction;

public interface IJsonConvertService
{
    public T? ConvertTo<T>(string json);
    public string ConvertBack<T>(T obj);
}