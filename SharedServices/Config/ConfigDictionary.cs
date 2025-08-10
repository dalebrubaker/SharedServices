using System.Collections.Concurrent;

namespace BruSoftware.SharedServices.Config;

public class ConfigDictionary
{
    private readonly string _configGroup;

    public ConfigDictionary(string configGroup)
    {
        _configGroup = configGroup;
        Values = new ConcurrentDictionary<string, string>();
    }

    public ConcurrentDictionary<string, string> Values { get; }

    public int Count => Values.Count;

    public string this[string key]
    {
        get => Values[key];
        set => Values[key] = value;
    }

    /// <summary>
    /// Return the string for the key if it is present in this dictionary,
    /// else return null
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string Element(string key)
    {
        if (Values.TryGetValue(key, out var value))
        {
            return value;
        }
        return null;
    }

    /// <summary>
    /// Add value for key to dictionary
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void SetElementValue(string key, string value)
    {
        Values[key] = value;
    }

    /// <summary>
    /// EraseKey
    /// </summary>
    /// <param name="key"></param>
    /// <returns><c>true</c> if key was found and erased</returns>
    public bool EraseKey(string key)
    {
        var result = Values.TryRemove(key, out _);
        return result;
    }

    /// <summary>
    /// Erase all settings in the dictionary
    /// </summary>
    public void Clear()
    {
        Values.Clear();
    }

    public override string ToString()
    {
        return $"{_configGroup} {Values.Count:N0} values";
    }
}