using System;
using System.Diagnostics;

namespace BruSoftware.SharedServices.ExtensionMethods;

public static class TimespanExtensions
{
    public static string ToHHMMStr(this TimeSpan timeSpan)
    {
        return $"{timeSpan.Hours:00}{timeSpan.Minutes:00}";
    }

    public static int ToHHMM(this TimeSpan timeSpan)
    {
        return timeSpan.Hours * 100 + timeSpan.Minutes;
    }

    public static int ToHHMMSS(this TimeSpan timeSpan)
    {
        return timeSpan.Hours * 10000 + timeSpan.Minutes * 100 + timeSpan.Seconds;
    }

    public static TimeSpan FromHHMM(this int hhmm)
    {
        var hours = hhmm / 100;
        var minutes = hhmm % 100;
        var result = new TimeSpan(hours, minutes, 0);
        return result;
    }

    public static string Pretty(this TimeSpan timeSpan)
    {
        if (timeSpan.TotalSeconds < 1)
        {
            return $"{timeSpan.TotalMilliseconds:N0} ms";
        }
        if (timeSpan.TotalMinutes < 1)
        {
            return $"{timeSpan.TotalSeconds:N0} sec";
        }
        if (timeSpan.TotalHours < 1)
        {
            // Remove fractional portion so we don't show rounded-up minutes
            var min = Math.Floor(timeSpan.TotalMinutes);
            return $"{min:N0}:{timeSpan.Seconds:00}";
        }
        if (timeSpan.TotalDays < 1)
        {
            // Remove fractional portion so we don't show rounded-up hours
            var hours = Math.Floor(timeSpan.TotalHours);
            return $"{hours:N0}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
        }
        // Remove fractional portion so we don't show rounded-up days
        var days = Math.Floor(timeSpan.TotalDays);
        return $"{days:N0}:{timeSpan.Hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
    }

    public static string PrettyShort(this TimeSpan timeSpan)
    {
        if (timeSpan.TotalMinutes < 1)
        {
            return $"{timeSpan.TotalMinutes:N2} minutes";
        }
        if (timeSpan.TotalHours < 1)
        {
            return $"{timeSpan.TotalHours:N2} hours";
        }
        return $"{timeSpan.TotalDays:N2} days";
    }

    public static bool IsWeekdayBetween(this DayOfWeek dayOfWeek, DayOfWeek prior, DayOfWeek next)
    {
        if (dayOfWeek == DayOfWeek.Monday && !GlobalsShared.IsUnitTesting)
        {
            Debugger.Break();
        }
        var dayOfWeekInt = (int)dayOfWeek;
        var priorInt = (int)prior;
        var nextInt = (int)next;
        if (nextInt < priorInt)
        {
            nextInt += 7;
        }
        if (dayOfWeekInt < priorInt)
        {
            dayOfWeekInt += 7;
        }
        if (dayOfWeekInt > priorInt && dayOfWeekInt < nextInt)
        {
            return true;
        }
        return false;
    }
}