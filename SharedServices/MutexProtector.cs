using System;
using System.IO;
using System.Threading;
using BruSoftware.SharedServices.ExtensionMethods;
using NLog;

namespace BruSoftware.SharedServices;

/// <summary>
/// Helper class for serializing access to a resource (e.g. the config file) when multiple instances are running.
/// </summary>
public sealed class MutexProtector : IDisposable
{
    private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

    private readonly Mutex _mutex;

    /// <summary>
    /// Create an instance of this class.
    /// </summary>
    /// <param name="name">unique (system-wide). Will be cleaned here: comma and space and backslash</param>
    public MutexProtector(string name)
    {
        var cleanPath = name.RemoveCharFromString(Path.DirectorySeparatorChar);
        cleanPath = cleanPath.RemoveCharFromString(',');
        cleanPath = cleanPath.RemoveCharFromString(' ');
        _mutex = new Mutex(true, cleanPath, out var createdNew);
        if (!createdNew)
        {
            try
            {
                _mutex.WaitOne();
            }
#pragma warning disable CC0004 // Catch block cannot be empty
            catch (AbandonedMutexException)
            {
                // ignore
                s_logger.Warn("Ignoring AbandonedMutexException for {Name}", name);
            }
#pragma warning restore CC0004 // Catch block cannot be empty
        }
    }

    /// <summary>
    /// Do to mutex thread affinity, this must be called from the same thread on which this class was created,
    /// or ReleaseMutex() will throw an exception.
    /// </summary>
    public void Dispose()
    {
        _mutex.ReleaseMutex();
        _mutex.Close(); // also does Dispose()
        GC.SuppressFinalize(this);
    }
}