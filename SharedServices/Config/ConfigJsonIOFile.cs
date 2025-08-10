using System.IO;
using Newtonsoft.Json;
using NLog;

namespace BruSoftware.SharedServices.Config;

public class ConfigJsonIOFile : IConfigJsonIO
{
    private const string ConfigIOFileMutex = "367E25B4-083D-4E21-A46A-259D4138C12D";
    private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

    /// <inheritdoc />
    public ConfigDictionary Load(string filePath, string group)
    {
        using (new MutexProtector(ConfigIOFileMutex))
        {
            if (!File.Exists(filePath))
            {
                return new ConfigDictionary(group);
            }
            var str = File.ReadAllText(filePath);
            if (string.IsNullOrEmpty(str))
            {
                return new ConfigDictionary(group);
            }
            var settings = JsonConvert.DeserializeObject<ConfigDictionary>(str, ConfigJson.JsonSerializerSettings);
            return settings;
        }
    }

    /// <inheritdoc />
    public void Save(string filePath, ConfigDictionary settings)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Utilities.CreateDirectory(directory);
        }
        using (new MutexProtector(ConfigIOFileMutex))
        {
            var str = JsonConvert.SerializeObject(settings, ConfigJson.JsonSerializerSettings);
            var length = str.Length;
            if (File.Exists(filePath))
            {
                // Start over, so we can remove old settings
                File.Delete(filePath);
            }
            File.WriteAllText(filePath, str);
        }
    }
}