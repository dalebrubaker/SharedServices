namespace BruSoftware.SharedServices.Config;

/// <summary>
/// The actual I/O for saving and getting settings
/// </summary>
public interface IConfigJsonIO
{
    /// <summary>
    /// Return a ConfigDictionary for group from filePath if possible.
    /// If the file doesn't exist or there is an error, return an empty ConfigDictionary for group (a new batch of settings for group)
    /// </summary>
    /// <param name="filePath">The fully-qualified fileName</param>
    /// <param name="group"></param>
    /// <returns>fileName</returns>
    ConfigDictionary Load(string filePath, string group);

    /// <summary>
    /// Save settings to filePath
    /// </summary>
    /// <param name="filePath">The fully-qualified filePath</param>
    /// <param name="settings">A ConfigDictionary of the settings for a group</param>
    void Save(string filePath, ConfigDictionary settings);
}