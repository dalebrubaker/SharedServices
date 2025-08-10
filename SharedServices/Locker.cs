using System;
using System.Threading;

namespace BruSoftware.SharedServices;

/// <summary>
/// This class allows multiple locking actions with guaranteed unlocking when used in a using block.
/// E.g.
/// var locker = new locker(_lock);
/// using (locker.Lock())
/// {
/// } // unlock happens here
/// Note that the scope braces are not required in C# 8
/// </summary>
public class Locker : IDisposable
{
    private readonly Action _actionEnter;
    private readonly Action _actionExit;
    private bool _lockWasTaken;
    private readonly object _lock = new();

    public Locker(Action actionEnter, Action actionExit)
    {
        _actionEnter = actionEnter;
        _actionExit = actionExit;
    }

    /// <summary>
    /// Use this ctor for a locker that doesn't lock
    /// </summary>
    public Locker()
    {
        _actionEnter = null;
        _actionExit = null;
    }

    /// <summary>
    /// Use this ctor for a locker that locks on lockObject (Monitor.Enter/Exit)
    /// </summary>
    /// <param name="lockObject"></param>
    public Locker(object lockObject)
    {
        _actionEnter = () =>
        {
            _lockWasTaken = false;
            Monitor.Enter(lockObject, ref _lockWasTaken);
            if (!_lockWasTaken)
            {
                throw new InvalidOperationException("Failed to acquire lock");
            }
        };
        _actionExit = () => Monitor.Exit(lockObject);
    }

    /// <summary>
    /// Use this for a locker that uses a Mutex to lock on a system-wide semaphore name.
    /// For example, this can be a Path or MapName to lock MemoryMappedFiles system-wide.
    /// Instantiate it with false (not owned)
    /// </summary>
    /// <param name="mutex"></param>
    public Locker(Mutex mutex)
    {
        _actionEnter = () => mutex.WaitOne();
        _actionExit = mutex.ReleaseMutex;
    }

    public Locker Lock(bool throwIfNotUIThread = true)
    {
        lock (_lock)
        {
            if (throwIfNotUIThread)
            {
                Utilities.ThrowIfIsUIThread();
            }
            _actionEnter?.Invoke();
            return this;
        }
    }

    public void Dispose()
    {
        // release lock
        if (_lockWasTaken)
        {
            try
            {
                _actionExit?.Invoke();
            }
            catch
            {
                // ignore
            }
        }
        GC.SuppressFinalize(this);
    }
}