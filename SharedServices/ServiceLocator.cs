using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace BruSoftware.SharedServices;

/// <summary>
/// Thanks to http://www.stefanoricciardi.com/2009/09/25/service-locator-pattern-in-csharpa-simple-example/
/// and https://www.martinfowler.com/articles/injection.html
/// </summary>
public static class ServiceLocator
{
    private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

    // map that contains pairs of interfaces and
    // references to concrete implementations
    private static readonly Dictionary<object, object> s_services = new();
    private static bool s_isDisposed;

    /// <summary>
    /// Set by ServicesLoader when all Services have been loaded
    /// </summary>
    public static bool IsServicesLoaded { get; set; }

    /// <summary>
    /// Get a service that has previously been added.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">If the type has not been added."/>"/>.</exception>
    public static T GetService<T>()
    {
        lock (s_services)
        {
            try
            {
                //s_logger.ConditionalTrace($"{nameof(GetService)} {typeof(T)}");
                return (T)s_services[typeof(T)];
            }
            catch (KeyNotFoundException)
            {
                if (s_isDisposed)
                {
                    // ignore
                    return default;
                }
                var message = $"The requested service {typeof(T).Name} is not registered";
                s_logger.Error(message);
                throw new SharedServicesException(message);
            }
        }
    }

    /// <summary>
    /// Get a service that has previously been added, or return default(T) if it doesn't exist
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetServiceOrNull<T>()
    {
        if (!Exists(typeof(T)))
        {
            return default;
        }
        return GetService<T>();
    }

    public static void RemoveService<T>()
    {
        lock (s_services)
        {
            var type = typeof(T);
            if (s_services.ContainsKey(type))
            {
                s_services.Remove(type);
                //s_logger.ConditionalTrace($"Removed {typeof(T)} in {nameof(RemoveService)} ");
            }
        }
    }

    public static void AddOrReplaceService<T>(T service)
    {
        RemoveService<T>();
        AddService(service);
    }

    /// <summary>
    /// Add service after removing if it already exists
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="service"></param>
    public static void RequireService<T>(T service)
    {
        RemoveService<T>();
        AddService(service);
    }

    /// <summary>
    /// Add service
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="service"></param>
    /// <exception cref="SharedServicesException">If the type has already been added.</exception>
    public static void AddService<T>(T service)
    {
        lock (s_services)
        {
            var type = typeof(T);
            if (s_services.ContainsKey(type))
            {
                var message = $"The requested service {typeof(T).Name} has already been registered.";
                throw new SharedServicesException(message);
            }
            s_services.Add(type, service);
            //s_logger.ConditionalTrace($"Added {typeof(T)} in {nameof(AddService)} ");
        }
    }

    /// <summary>
    /// Return true if the service exists (has previously been added)
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private static bool Exists(Type type)
    {
        lock (s_services)
        {
            return s_services.ContainsKey(type);
        }
    }

    public static void DisposeAll()
    {
        lock (s_services)
        {
            var values = s_services.Values.ToList();
            for (var i = 0; i < values.Count; i++)
            {
                var service = values[i];
                if (service is IDisposable s)
                {
                    s.Dispose();
                }
            }
            s_isDisposed = true;
            s_services.Clear();
            //s_logger.ConditionalTrace($"Disposed all services in {nameof(ServiceLocator)} ");
        }
    }
}