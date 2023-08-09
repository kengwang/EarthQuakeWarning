using EarthquakeWaring.App.Infrastructure.Models.ApiModels;
using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EarthquakeWaring.App.Services;

public class HuaniaEarthQuakeApi : IEarthQuakeApi
{
    private readonly IHttpRequester _httpRequester;
    private readonly IJsonConvertService _jsonConvert;
    private readonly ILogger<HuaniaEarthQuakeApi> _logger;
    private const string HuaniaApi = "https://mobile-new.chinaeew.cn/v1/";

    public HuaniaEarthQuakeApi(IHttpRequester httpRequester, IJsonConvertService jsonConvert,
        ILogger<HuaniaEarthQuakeApi> logger)
    {
        _httpRequester = httpRequester;
        _jsonConvert = jsonConvert;
        _logger = logger;
    }

    public async Task<List<HuaniaEarthQuake>> GetEarthQuakeList(long startPointer, CancellationToken cancellationToken)
    {
        try
        {
            var response = _jsonConvert.ConvertTo<HuaniaWarningsResponse>(
                await _httpRequester.GetString(HuaniaApi + "earlywarnings?updates=3&start_at=" + startPointer, null,
                    cancellationToken).ConfigureAwait(false));
            if (response?.Code != 0)
                throw new Exception(response?.Message);
            return response.Data;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while calling Huania Api");
        }

        return new List<HuaniaEarthQuake>();
    }

    public async Task<List<EarthQuakeUpdate>> GetEarthQuakeInfo(long earthQuakeId, CancellationToken cancellationToken)
    {
        try
        {
            var response = _jsonConvert.ConvertTo<HuaniaEarthQuakeInfoResponse>(
                await _httpRequester.GetString(HuaniaApi + "earlywarnings/" + earthQuakeId, null,
                    cancellationToken));
            if (response?.Code != 0)
                throw new Exception(response?.Message);
            return response.Data;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while calling Huania Api");
        }
        return new List<EarthQuakeUpdate>();
    }
}