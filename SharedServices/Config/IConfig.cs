using System;
using System.Collections.Generic;

namespace BruSoftware.SharedServices.Config;

public interface IConfig
{
    /// <summary>
    /// Get the setting for id within group, or defaultValue if not previously set.
    /// Get the config as type T. Use for simple types that do have a TypeConverter.
    /// Standard type converters include:
    /// bool, enum, Font, Color, DateTimeOffset, all number types,
    /// See http://msdn.microsoft.com/en-us/library/system.componentmodel.typeconverter_derivedtypelist(VS.85).aspx
    /// If you fail to Load() before doing a GetConfig(), the first GetConfig will start a empty file.
    /// </summary>
    /// <typeparam name="T">the type of the value to get</typeparam>
    /// <param name="group">the group name. For file storage, must be a valid filename without suffix</param>
    /// <param name="id">the unique name of an element within group</param>
    /// <param name="defaultValue">The default value returned if s is null</param>
    /// <returns>The type returned after conversion</returns>
    /// <exception cref="System.NotSupportedException">Thrown when no TypeConverter exists for T</exception>
    T GetConfig<T>(string group, string id, T defaultValue);

    /// <summary>
    /// Get the setting for id within group, or defaultValue if not previously set.
    /// Get the config as type T from Json stored using Json.Net via SetConfigJson().
    /// If you fail to Load() before doing a GetConfig(), the first GetConfig will start a empty file.
    /// </summary>
    /// <typeparam name="T">the type of the value to get</typeparam>
    /// <param name="group">the group name. For file storage, must be a valid filename without suffix</param>
    /// <param name="id">the unique name of an element within group</param>
    /// <param name="defaultValue">The default value returned if s is null</param>
    /// <returns>The type returned after conversion</returns>
    T GetConfigJson<T>(string group, string id, T defaultValue);

    /// <summary>
    /// Get an IEnumerable collection of type T for id within group, or defaultValues if not previously set.
    /// </summary>
    /// <typeparam name="T">the type of the value to get</typeparam>
    /// <param name="group">the group name. For file storage, must be a valid filename without suffix</param>
    /// <param name="id">the unique name of an element within group</param>
    /// <param name="defaultValues"></param>
    /// <returns>IEnumerable collection of type T</returns>
    IEnumerable<T> GetConfigEnumerable<T>(string group, string id, IEnumerable<T> defaultValues);

    /// <summary>
    /// Restore the window pointed to by handle to the state previously saved by SaveWindow().
    /// Or if never previously saved, do nothing.
    /// Note: For form, use the Control.Handle property to get the handle.
    /// Under the covers, this uses User32.GetWindowPlacement() which handles multiple screens, edge cases, etc.
    /// </summary>
    /// <param name="group">the group name. For file storage, must be a valid filename without suffix</param>
    /// <param name="id">the unique name of this window within group</param>
    /// <param name="handle">the window handle</param>
    /// <returns>the placement JSON if a window was restored</returns>
    string RestoreWindow(string group, string id, IntPtr handle);

    /// <summary>
    /// Load a group from a particular fileName instead of from the directory set in the constructor.
    /// Files are loaded from the directory passed in by the constructor, with names group.xml or group.json
    /// If you fail to Load() before doing a GetConfig(), the first GetConfig will start a empty file.
    /// </summary>
    /// <param name="group">A group name cannot have spaces!</param>
    /// <param name="filePath">The fully-qualified fileName</param>
    void Load(string group, string filePath);

    /// <summary>
    /// Save the settings for group to the logical path passed in via the constructor
    /// </summary>
    /// <param name="group">the group name. For file storage, must be a valid filename without suffix</param>
    void Save(string group);

    /// <summary>
    /// Save the settings for group to fileName
    /// </summary>
    /// <param name="group">A group name cannot have spaces!</param>
    /// <param name="filePath">The fully-qualified fileName</param>
    void Save(string group, string filePath);

    /// <summary>
    /// Save the current state of the window pointed to by handle.
    /// Note: For form, use the Control.Handle property to get the handle.
    /// </summary>
    /// <param name="group">the group name. For file storage, must be a valid filename without suffix</param>
    /// <param name="id">the unique name of this window within group</param>
    /// <param name="handle">the window handle</param>
    void SaveWindow(string group, string id, IntPtr handle);

    /// <summary>
    /// Set a configuration to value.
    /// If you fail to Load() before doing a SetConfig(), the first SetConfig will start a empty file.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="group">the group name. For file storage, must be a valid filename without suffix</param>
    /// <param name="id">the unique name of an element within group</param>
    /// <param name="value">the value to save</param>
    void SetConfig<T>(string group, string id, T value);

    /// <summary>
    /// Set a configuration to value using Json.Net to convert the value to JSON.
    /// If you fail to Load() before doing a SetConfig(), the first SetConfig will start a empty file.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="group"></param>
    /// <param name="id"></param>
    /// <param name="value"></param>
    void SetConfigJson<T>(string group, string id, T value);

    /// <summary>
    /// Set configuration for an IEnumerable collection of type T
    /// </summary>
    /// <typeparam name="T">The type of each item in the IEnumerable</typeparam>
    /// <param name="group">the group name. For file storage, must be a valid filename without suffix</param>
    /// <param name="id">the unique name of an element within group</param>
    /// <param name="values">the values to save</param>
    void SetConfigEnumerable<T>(string group, string id, IEnumerable<T> values);

    /// <summary>
    /// Remove an old id from Settings if it exists
    /// </summary>
    /// <param name="group"></param>
    /// <param name="id"></param>
    /// <returns><c>true</c> if id was found and erased</returns>
    bool EraseId(string group, string id);
}