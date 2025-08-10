namespace BruSoftware.SharedServices.Config;

/// <summary>
/// Use this interface to pass a parent form or user control ConfigGroup into a user control
/// </summary>
public interface IConfigParent
{
    /// <summary>
    /// Gets the parent's ConfigGroup
    /// </summary>
    string ConfigGroup { get; }

    /// <summary>
    /// Gets the parent's IConfig
    /// </summary>
    IConfig Config { get; }
}