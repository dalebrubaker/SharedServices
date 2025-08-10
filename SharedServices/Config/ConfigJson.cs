using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BruSoftware.SharedServices.ExtensionMethods;
using Newtonsoft.Json;
using NLog;

namespace BruSoftware.SharedServices.Config;

/// <summary>
/// This class does not use a dictionary of XElement, but just a dictionary of strings
/// The dictionary is serialized to the config file using Json.Net
/// </summary>
public class ConfigJson : IConfig
{
    private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Does the actual Load and Save
    /// </summary>
    private readonly IConfigJsonIO _configJsonIO;

    /// <summary>
    /// The path to which group.json will be added
    /// </summary>
    private readonly string _directory;

    private readonly object _lockSettings = new();

    /// <summary>
    /// The key is group. Each group has its own dictionary of JSON strings
    /// </summary>
    private readonly ConcurrentDictionary<string, ConfigDictionary> _settingsByGroup;

    /// <summary>
    /// This one is for NinjaTrader 8 which apparently can't handle default parameters
    /// Info to configure the Config is passed into the constructor.
    /// Config files are saved like this: appData\companyName\appName\group.json
    /// </summary>
    /// <param name="companyName">The first part of the directory under appData</param>
    /// <param name="appName">The second part of the directory under appData</param>
    /// <param name="configJsonIO">Does the actual Load and Save</param>
    public ConfigJson(string companyName, string appName, IConfigJsonIO configJsonIO)
    {
        _configJsonIO = configJsonIO;
        _settingsByGroup = new ConcurrentDictionary<string, ConfigDictionary>();
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _directory = Path.Combine(appData, companyName, appName);
    }

    public static JsonSerializerSettings JsonSerializerSettings
    {
        get
        {
            if (field != null)
            {
                return field;
            }
            var namespaceMigrationSerializationBinder = new NamespaceMigrationSerializationBinder();
            field = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                Formatting = Formatting.None,

                //DefaultValueHandling = DefaultValueHandling.Ignore, Commented this out or Plot.PlotStyle is not saved even when changed. Bug in Json.Net I think
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ContractResolver = new BruTraderJsonCustomResolver(),

                // Binder is obsolete. Use SerializationBinder instead.
                SerializationBinder = namespaceMigrationSerializationBinder
            };
            return field;
        }
    }

    /// <inheritdoc />
    public T GetConfig<T>(string group, string id, T defaultValue)
    {
        var settings = RequireSettings(group);
        var value = settings.Element(id);
        if (value == null)
        {
            return defaultValue;
        }
        try
        {
            if (typeof(T) == typeof(int))
            {
                // Handle a change from long to int
                var _ = value.ToInt64();
                if (_ > int.MaxValue)
                {
                    value = int.MaxValue.ToString();
                }
            }
            var result = Converters.FromString(value, defaultValue);
            return result;
        }
        catch (Exception ex)
        {
            s_logger.Error(ex, "{Message}", ex.Message);
            throw;
        }
    }

    /// <inheritdoc />
    public T GetConfigJson<T>(string group, string id, T defaultValue)
    {
        try
        {
            var settings = RequireSettings(group);
            if (settings == null)
            {
                return defaultValue;
            }
            var value = settings.Element(id);
            if (value == null || value == "null")
            {
                return defaultValue;
            }
            var length = value.Length;
            var result = JsonConvert.DeserializeObject<T>(value, JsonSerializerSettings);
            return result;
        }
        catch (Exception ex)
        {
            var msg = $"Returning default value from GetConfigJson. {ex.Message}";
            s_logger.Warn(ex, msg);
            if (ex.Message.Contains("default constructor"))
            {
                // Fix the class with default ctor or [JsonConstructor]
                throw;
            }
            return defaultValue;
        }
    }

    /// <inheritdoc />
    public IEnumerable<T> GetConfigEnumerable<T>(string group, string id, IEnumerable<T> defaultValues)
    {
        return GetConfigJson(group, id, defaultValues);
    }

    /// <inheritdoc />
    public void Load(string group, string filePath)
    {
        if (string.IsNullOrEmpty(group))
        {
            throw new SharedServicesException("group cannot be null or empty");
        }
        if (string.IsNullOrEmpty(filePath))
        {
            throw new SharedServicesException("fileName cannot be null or empty");
        }
        lock (_lockSettings)
        {
            if (!_settingsByGroup.TryGetValue(group, out var settings))
            {
                // This is the first time we've seen this group. Load it if it exists, or get empty settings if it doesn't exist
                settings = _configJsonIO.Load(filePath, group);
                var added = _settingsByGroup.TryAdd(group, settings);
                MyDebug.Assert(added);
            }
        }
    }

    /// <inheritdoc />
    public void Save(string group)
    {
        lock (_lockSettings)
        {
            if (string.IsNullOrEmpty(group))
            {
                throw new SharedServicesException("group cannot be null or empty");
            }
            var fileName = GetConfigFilePath(group);
            var settings = RequireSettings(group);
            _configJsonIO.Save(fileName, settings);
        }
    }

    public void Save(string group, string filePath)
    {
        lock (_lockSettings)
        {
            if (string.IsNullOrEmpty(group))
            {
                throw new SharedServicesException("group cannot be null or empty");
            }
            var settings = RequireSettings(group);
            _configJsonIO.Save(filePath, settings);
        }
    }

    /// <inheritdoc />
    public string RestoreWindow(string group, string id, IntPtr handle)
    {
        var settings = RequireSettings(group);
        var placement = settings.Element(id);
        if (placement == null)
        {
            // Not previously saved, so do nothing.
            return null;
        }
        //s_logger.Error($"RestoreWindow {group} {id} placement={placement} in {this} handle={handle}");
        WindowPlacement.SetPlacement(handle, placement);
        return placement;
    }

    /// <inheritdoc />
    public void SaveWindow(string group, string id, IntPtr handle)
    {
        var placement = WindowPlacement.GetPlacement(handle);
        //s_logger.Error($"SaveWindow   {group} {id} placement={placement} in {this} handle={handle}");
        SetConfig(group, id, placement);
    }

    /// <inheritdoc />
    public void SetConfig<T>(string group, string id, T value)
    {
        if (group == null)
        {
            s_logger.Warn("null group called for id={Id}", id);
            return;
        }
        var settings = RequireSettings(group);
        var str = Converters.ToString(value);
        settings.SetElementValue(id, str);
    }

    public void SetConfigJson<T>(string group, string id, T value)
    {
        var settings = RequireSettings(group);
        try
        {
            var str = JsonConvert.SerializeObject(value, JsonSerializerSettings);
            var length = str.Length;
            settings.SetElementValue(id, str);
        }
        catch (JsonSerializationException jsex)
        {
            var _ = jsex.ToString();
            throw;
        }
    }

    /// <inheritdoc />
    public void SetConfigEnumerable<T>(string group, string id, IEnumerable<T> values)
    {
        SetConfigJson(group, id, values);
    }

    /// <inheritdoc />
    public bool EraseId(string group, string id)
    {
        var settings = RequireSettings(group);
        var result = settings.EraseKey(id);
        return result;
    }

    public static void AddMigration(NamespaceMigration migration)
    {
        var binder = (NamespaceMigrationSerializationBinder)JsonSerializerSettings.SerializationBinder;
        binder?.Add(migration);
    }

    /// <summary>
    /// Return an IConfig based on a ConfigJsonIOFile
    /// Config files are saved like this: $(root)\companyName\projectName\group.json
    /// </summary>
    /// <param name="projectName"></param>
    /// <returns></returns>
    public static IConfig GetConfig(string projectName)
    {
        IConfig config = new ConfigJson(nameof(BruSoftware), projectName, new ConfigJsonIOFile());
        return config;
    }

    /// <summary>
    /// Get the file path corresponding to group.
    /// </summary>
    /// <param name="group">the group name. For file storage, must be a valid filename without suffix</param>
    /// <returns>the fully qualified file path</returns>
    private string GetConfigFilePath(string group)
    {
        return Path.Combine(_directory, group + ".json");
    }

    /// <summary>
    /// Get existing settings for group from the dictionary.
    /// When group is first seen, load the existing settings (or start new settings if none exists)
    /// </summary>
    /// <param name="group">the group name. For file storage, must be a valid filename without suffix</param>
    /// <returns>the settings <c>XElement</c></returns>
    private ConfigDictionary RequireSettings(string group)
    {
        lock (_lockSettings)
        {
            if (!_settingsByGroup.TryGetValue(group, out var settings))
            {
                // This is the first time we've seen this group. Load it if it exists, or get empty settings if it doesn't exist
                var filePath = GetConfigFilePath(group);
                settings = _configJsonIO.Load(filePath, group);
                var added = _settingsByGroup.TryAdd(group, settings);
                MyDebug.Assert(added);
            }
            return settings;
        }
    }

    public override string ToString()
    {
        var result = "";
        foreach (var key in _settingsByGroup.Keys)
        {
            var values = _settingsByGroup[key].Values.Keys.ToList();
            var str = string.Join(',', values);
            result += $"{key} {str} ";
        }
        return $"{result} at {_directory}";
    }
}