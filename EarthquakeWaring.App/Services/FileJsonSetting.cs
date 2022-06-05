using System;
using System.ComponentModel;
using System.IO;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using Microsoft.Extensions.Logging;

namespace EarthquakeWaring.App.Services;

public class FileJsonSetting<TSetting> : ISetting<TSetting> where TSetting : INotificationOption
{
    private readonly IJsonConvertService _jsonConvertService;
    private readonly ILogger<FileJsonSetting<TSetting>> _logger;

    private TSetting? _inMemorySetting;
    private string _settingName = nameof(TSetting);

    public TSetting? Setting => _inMemorySetting;

    public FileJsonSetting(IJsonConvertService jsonConvertService, ILogger<FileJsonSetting<TSetting>> logger)
    {
        _jsonConvertService = jsonConvertService;
        _logger = logger;
        _settingName = typeof(TSetting).Name;

        // Pre Load Settings
        LoadSettingsFromFile();

        // Add FileSystem Monitor
        var fileSystemWatcher = new FileSystemWatcher();
        fileSystemWatcher.Path = $"{Directory.GetCurrentDirectory()}/settings";
        fileSystemWatcher.Changed += FileMonitorOnChanged;
        fileSystemWatcher.Created += FileMonitorOnChanged;
        fileSystemWatcher.Deleted += FileMonitorOnChanged;
        fileSystemWatcher.Renamed += FileMonitorOnChanged;
        fileSystemWatcher.Filters.Add("*.json");
        fileSystemWatcher.EnableRaisingEvents = true;

        // Add Option Monitor
        if (_inMemorySetting != null) _inMemorySetting.PropertyChanged += InMemorySettingOnPropertyChanged;
    }

    private void InMemorySettingOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // This will automatically fire FileSystemWatcher
        if (_inMemorySetting != null)
        {
            File.WriteAllText($"settings/{_settingName}.json", _jsonConvertService.ConvertBack(_inMemorySetting));
            return;
        }
        File.Delete($"settings/{_settingName}.json");
    }

    private void FileMonitorOnChanged(object sender, FileSystemEventArgs e)
    {
        if (e.FullPath.EndsWith($"{_settingName}.json"))
        {
            LoadSettingsFromFile();
        }
    }

    private void LoadSettingsFromFile()
    {
        _logger.LogTrace("Loading Configuration of {SettingName}", nameof(TSetting));
        try
        {
            if (!Directory.Exists("settings")) Directory.CreateDirectory("settings");
            if (File.Exists($"settings/{_settingName}.json"))
                _inMemorySetting =
                    _jsonConvertService.ConvertTo<TSetting>(
                        File.ReadAllText($"settings/{_settingName}.json"));
            else
            {
                File.WriteAllText($"settings/{_settingName}.json", "{}");
                _inMemorySetting = default;
            }
        }
        catch (Exception e)
        {
            _inMemorySetting = default;
        }
    }
}