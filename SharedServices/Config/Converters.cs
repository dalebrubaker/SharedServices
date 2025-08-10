using System;
using System.ComponentModel;

namespace BruSoftware.SharedServices.Config;

/// <summary>
/// Useful converters, from ILSpy SessionSettings.cs
/// Throws NotSupportedException if no TypeConverter between T and String.
/// </summary>
public static class Converters
{
    /// <summary>
    /// Get a value from a string
    /// </summary>
    /// <typeparam name="T">The type of the defaultValue and the type returned</typeparam>
    /// <param name="s">The input string</param>
    /// <param name="defaultValue">The default value returned if s is null</param>
    /// <returns>The type returned after conversion from s</returns>
    /// <exception cref="System.NotSupportedException">Thrown when no TypeConverter exists for T</exception>
    /// <exception cref="T:System.BruTraderException"></exception>
    public static T FromString<T>(string s, T defaultValue)
    {
        if (s == null)
        {
            return defaultValue;
        }
        try
        {
            var c = TypeDescriptor.GetConverter(typeof(T));
            var canConvertFromString = c.CanConvertFrom(typeof(string));
            if (canConvertFromString)
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter is EnumConverter enumConverter && string.IsNullOrEmpty(s))
                {
                    // Avoid FormatException
                    return defaultValue;
                }
                if (converter.CanConvertFrom(typeof(string)))
                {
                    return (T)converter.ConvertFromString(s);
                }
                throw new SharedServicesException($"Can't convert from string to {typeof(T).FullName}");
            }
            return (T)c.ConvertFromInvariantString(s);
        }
        catch (FormatException)
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Get from string
    /// </summary>
    /// <typeparam name="T">The type of the defaultValue and the type returned</typeparam>
    /// <param name="s">The input string</param>
    /// <returns>The type returned after conversion from s</returns>
    /// <exception cref="System.NotSupportedException">Thrown when no TypeConverter exists for T</exception>
    /// <exception cref="T:System.BruTraderException"></exception>
    public static T FromString<T>(string s)
    {
        var c = TypeDescriptor.GetConverter(typeof(T));
        if (c?.CanConvertFrom(typeof(string)) != true)
        {
            throw new SharedServicesException($"Can't convert from string to {typeof(T).FullName}");
        }
        return (T)c.ConvertFromInvariantString(s);
    }

    public static string ToString<T>(T obj)
    {
        var c = TypeDescriptor.GetConverter(typeof(T));
        if (c?.CanConvertTo(typeof(string)) != true)
        {
            throw new SharedServicesException($"Can't convert from {typeof(T).FullName} to string");
        }
        return c.ConvertToInvariantString(obj);
    }
}