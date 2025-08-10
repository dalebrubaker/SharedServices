using System;
using System.Timers;

namespace BruSoftware.SharedServices;

/// <summary>
/// Holds a timer than fires once per day, and only once per day, after a given time of day (HHMMSS)
/// </summary>
public class OncePerDay : IDisposable
{
    private readonly Timer _timer;
    private DateTime _lastDateSet;

    /// <summary>
    /// </summary>
    /// <param name="timeOfDay">24:00:00 effectively disables this class</param>
    public OncePerDay(TimeSpan timeOfDay)
    {
        TimeOfDay = timeOfDay;
        if (TimeOfDay < TimeSpan.FromHours(24))
        {
            _timer = new Timer();
            if (TimeOfDay > DateTime.Now.TimeOfDay)
            {
                var interval = TimeOfDay - DateTime.Now.TimeOfDay;
                _timer.Interval = interval.TotalMilliseconds;
            }
            else
            {
                _lastDateSet = DateTime.Now.Date;
                SetIntervalUntilTomorrow();
            }
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }
    }

    public TimeSpan TimeOfDay { get; }

    /// <summary>
    /// <c>true</c> means the given timeOfDay has passed.
    /// This is set once and only once per day
    /// Set this to <c>false</c> by calling <see cref="Reset" /> when you are finished with it for today
    /// </summary>
    public bool IsSet { get; private set; }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    /// <summary>
    /// Call this to reset <see cref="IsSet" /> to <c>false</c> until tomorrow
    /// </summary>
    public void Reset()
    {
        IsSet = false;
    }

    private void TimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        if (DateTime.Now.Date > _lastDateSet)
        {
            _timer.Stop(); // stop for easier debugging
            _lastDateSet = DateTime.Now.Date;
            IsSet = true;
            SetIntervalUntilTomorrow();
            _timer.Start();
        }
    }

    private void SetIntervalUntilTomorrow()
    {
        var interval = _lastDateSet.AddDays(1) + TimeOfDay - DateTime.Now;
        _timer.Interval = interval.TotalMilliseconds;
    }

    public override string ToString()
    {
        return _timer == null
            ? $"Not running, because TimeOfDay={TimeOfDay}"
            : $"IsSet={IsSet} TimeOfDay={TimeOfDay} _lastDateSet={_lastDateSet}";
    }
}