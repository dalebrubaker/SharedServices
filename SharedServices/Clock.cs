using System;
using System.Diagnostics;

namespace BruSoftware.SharedServices;

/// <summary>
/// This clock is "variable-speed" and is used by Playback to report the current time.
/// </summary>
public class Clock
{
    private DateTime _initialUtcNow;

    private double _speedMultiple;

    /// <summary>
    /// This is the current time for this clock. Not related to wall-clock time.
    /// </summary>
    private DateTime _startTimestampUtc;

    private Stopwatch _stopwatch;

    public bool IsPaused;

    public Clock()
    {
        _initialUtcNow = DateTime.MinValue;
    }

    public bool IsStarted { get; private set; }

    /// <summary>
    /// Return the current time (UTC) based on the StartTimestampUtc and the SpeedMultiple
    /// If never Start() and never InitUtcNow(), return DateTime.UtcNow
    /// </summary>
    public DateTime UtcNow
    {
        get
        {
            if (!IsStarted)
            {
                if (_initialUtcNow == DateTime.MinValue)
                {
                    return DateTime.UtcNow;
                }
                return _initialUtcNow;
            }
            var elapsedMsecs = (long)(_speedMultiple * _stopwatch.ElapsedMilliseconds);
            return _startTimestampUtc.AddMilliseconds(elapsedMsecs);
        }
    }

    /// <summary>
    /// Start this clock at startTimestampUtc and run it at speedMultiple times real time
    /// </summary>
    /// <param name="startTimestampUtc"></param>
    /// <param name="speedMultiple"></param>
    public void Start(DateTime startTimestampUtc, double speedMultiple)
    {
        _startTimestampUtc = startTimestampUtc;
        _speedMultiple = speedMultiple;
        _stopwatch = Stopwatch.StartNew();
        IsStarted = true;
    }

    /// <summary>
    /// Toggle from Paused to Not Paused
    /// </summary>
    public void PauseToggle()
    {
        if (!IsPaused)
        {
            _stopwatch.Stop();
            IsPaused = true;
        }
        else
        {
            _stopwatch.Start();
            IsPaused = false;
        }
    }

    public void Stop()
    {
        _stopwatch?.Stop();
        IsStarted = false;
        _stopwatch = null;
    }

    public void InitUtcNow(DateTime timeStampUtc)
    {
        _initialUtcNow = timeStampUtc;
    }
}