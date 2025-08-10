using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;

namespace BruSoftware.SharedServices.ExtensionMethods;

public static class DateTimeExtensions
{
    public static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToYYYYMMDD(this DateTime dateTime)
    {
        var result = dateTime.Year * 10000 + dateTime.Month * 100 + dateTime.Day;
        return result;
    }

    public static int ToYYYYMMDD(this DateTimeOffset dateTime)
    {
        var result = dateTime.Year * 10000 + dateTime.Month * 100 + dateTime.Day;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToYYYYMM(this DateTime dateTime)
    {
        var result = dateTime.Year * 100 + dateTime.Month;
        return result;
    }

    /// <summary>
    /// Return the value of dateTime in total milliseconds
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns>the value of dateTime in total milliseconds</returns>
    public static long ToMSecs(this DateTime dateTime)
    {
        return dateTime.Ticks.ToMSecs();
    }

    /// <summary>
    /// Return a time in total milliseconds from a DateTime.Ticks value
    /// </summary>
    /// <param name="ticks">a DateTime.Ticks value</param>
    /// <returns>a time in total milliseconds from a DateTime.Ticks value</returns>
    public static long ToMSecs(this long ticks)
    {
        return ticks / TimeSpan.TicksPerMillisecond;
    }

    /// <summary>
    /// Return the number of DateTime.Ticks from a time in total milliseconds
    /// </summary>
    /// <param name="msecs">a time in total milliseconds</param>
    /// <returns>the number of DateTime.Ticks from a time in total milliseconds</returns>
    public static long FromMSecsTicks(this long msecs)
    {
        return msecs * TimeSpan.TicksPerMillisecond;
    }

    /// <summary>
    /// Return a DateTime (UTC) from a time in total milliseconds
    /// </summary>
    /// <param name="msecs">total milliseconds</param>
    /// <returns>a DateTime (UTC) from a time in total milliseconds</returns>
    public static DateTime FromMsecsUtc(this long msecs)
    {
        var ticks = msecs.FromMSecsTicks();
        return new DateTime(ticks, DateTimeKind.Utc);
    }

    /// <summary>
    /// Truncate to the nearest timespan interval.
    /// For example, if timespan is TimeSpan.FromSeconds(1), truncate to the end of the previous second.
    /// Thanks to http://stackoverflow.com/questions/1004698/how-to-truncate-milliseconds-off-of-a-net-datetime
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime Truncate(this DateTime dateTime, TimeSpan timeSpan)
    {
        return timeSpan == TimeSpan.Zero ? dateTime : dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
    }

    /// <summary>
    /// Truncate to the end of the previous second.
    /// Thanks to http://stackoverflow.com/questions/1004698/how-to-truncate-milliseconds-off-of-a-net-datetime
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime TruncateToSecond(this DateTime dateTime)
    {
        var extraTicks = dateTime.Ticks % TimeSpan.TicksPerSecond;
        if (extraTicks > 0)
        {
            var result = dateTime.AddTicks(-extraTicks);
            return result;
        }
        return dateTime;
    }

    /// <summary>
    /// Truncate to the end of the previous minute.
    /// Thanks to http://stackoverflow.com/questions/1004698/how-to-truncate-milliseconds-off-of-a-net-datetime
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime TruncateToMinute(this DateTime dateTime)
    {
        var extraTicks = dateTime.Ticks % TimeSpan.TicksPerMinute;
        if (extraTicks > 0)
        {
            var result = dateTime.AddTicks(-extraTicks);
            return result;
        }
        return dateTime;
    }

    /// <summary>
    /// Get a dateTime at the end of the minute. If dateTime is exactly at the beginning of a minute, it returns 1 minute later
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime RoundToMinute(this DateTime dateTime)
    {
        var extraTicks = dateTime.Ticks % TimeSpan.TicksPerMinute;
        var ticksToAdd = TimeSpan.TicksPerMinute - extraTicks;
        if (ticksToAdd > 0)
        {
            var result = dateTime.AddTicks(ticksToAdd);
            return result;
        }
        return dateTime;
    }

    /// <summary>
    /// Truncate to the nearest timespan interval.
    /// For example, if timespan is TimeSpan.FromSeconds(1), truncate to the end of the previous second.
    /// Thanks to http://stackoverflow.com/questions/1004698/how-to-truncate-milliseconds-off-of-a-net-datetime
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime? Truncate(this DateTime? dateTime, TimeSpan timeSpan)
    {
        if (dateTime == null)
        {
            return null;
        }
        if (timeSpan == TimeSpan.Zero)
        {
            return dateTime; // Or could throw an ArgumentException
        }
        var notNullDateTime = (DateTime)dateTime;
        return notNullDateTime.AddTicks(-(notNullDateTime.Ticks % timeSpan.Ticks));
    }

    /// <summary>
    /// Do a safe subtraction of numDays from this DateTime, not going back before DateTime.MinValue
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="numDays"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime SubtractDays(this DateTime dateTime, int numDays)
    {
        var timeSpan = dateTime - DateTime.MinValue;
        return numDays * TimeSpan.TicksPerDay >= timeSpan.Ticks ? DateTime.MinValue : dateTime.AddDays(-numDays);
    }

    /// <summary>
    /// Do a safe subtraction of numDays from this DateTime, not going back before DateTime.MinValue
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime SubtractSafe(this DateTime dateTime, TimeSpan timeSpan)
    {
        var timeSpanAvailable = dateTime - DateTime.MinValue;
        var timeSpanTicks = Math.Min(timeSpan.Ticks, timeSpanAvailable.Ticks);
        var result = dateTime.AddTicks(-timeSpanTicks);
        return result;
    }

    /// <summary>
    /// Do a safe addition of numDays to this DateTime
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="numDays"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime AddDaysSafe(this DateTime dateTime, int numDays)
    {
        if (DateTime.MaxValue - dateTime < TimeSpan.FromDays(1))
        {
            return DateTime.MaxValue;
        }
        return dateTime.AddDays(numDays);
    }

    /// <summary>
    /// Return dateTime in a format suitable for inclusion in a fileName
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToFileNameString(this DateTime dateTime)
    {
        var str = $"{dateTime:yyyy-MM-dd_hh-mm-ss-tt}";
        return str;
    }

    /// <summary>
    /// Thanks to https://stackoverflow.com/questions/38039/how-can-i-get-the-datetime-for-the-start-of-the-week
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static DateTime StartOfWeek(this DateTime dt)
    {
        //var sunday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
        //return sunday;
        var ci = Thread.CurrentThread.CurrentCulture;
        var fdow = ci.DateTimeFormat.FirstDayOfWeek;
        var offset = (dt.DayOfWeek - fdow + 7) % 7;
        return DateTime.Today.SubtractDays(offset);
    }

    /// <summary>
    /// Thanks to colin at http://joelabrahamsson.com/getting-the-first-day-in-a-week-with-c/
    /// </summary>
    /// <param name="dayInWeek"></param>
    /// <param name="firstDay"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime GetFirstDayInWeek(this DateTime dayInWeek, DayOfWeek firstDay)
    {
        var difference = (int)dayInWeek.DayOfWeek - (int)firstDay;
        difference = (7 + difference) % 7;
        return dayInWeek.SubtractDays(difference).Date;
    }

    /// <summary>
    /// Thanks to colin at http://joelabrahamsson.com/getting-the-first-day-in-a-week-with-c/
    /// </summary>
    /// <param name="dayInWeek"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime GetFirstDayInWeek(this DateTime dayInWeek)
    {
        return GetFirstDayInWeek(dayInWeek, CultureInfo.CurrentCulture);
    }

    public static DateTime GetFirstDayInWeek(this DateTime dayInWeek, CultureInfo cultureInfo)
    {
        return GetFirstDayInWeek(dayInWeek, cultureInfo.DateTimeFormat.FirstDayOfWeek);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime ToDateTime(this long ticks)
    {
        return new DateTime(ticks);
    }

    /// <summary>
    /// Return the minute of day, 0 being 1200-1201 a.m. and 1439 being 1159-1200 pm
    /// This extension is on DateTime rather than on TimeSpan so as not to waste creating a TImeSpan instance
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToMinuteOfDay(this DateTime dateTime)
    {
        return (int)dateTime.TimeOfDay.TotalMinutes;
        //var timeOfDay = dateTime.Ticks % TimeSpan.TicksPerDay;
        //var minuteOfDay = (int)(timeOfDay / TimeSpan.TicksPerMinute);
        //return minuteOfDay;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime FromEasternTimeZone(this DateTime dateTime, TimeZoneInfo toTimeZoneInfo)
    {
        MyDebug.Assert(dateTime.Kind == DateTimeKind.Unspecified, "Kind must not be specified");
        const string easternZoneId = "Eastern Standard Time";
        var easternZone = TimeZoneInfo.FindSystemTimeZoneById(easternZoneId);
        var result = TimeZoneInfo.ConvertTime(dateTime, easternZone, toTimeZoneInfo);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime FromEasternTimeZoneToCurrentTimeZone(this DateTime dateTime)
    {
        const string easternZoneId = "Eastern Standard Time";
        var easternZone = TimeZoneInfo.FindSystemTimeZoneById(easternZoneId);
        var result = TimeZoneInfo.ConvertTime(dateTime, easternZone, TimeZoneInfo.Local);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime FromLocalTimeZoneToEasternTimeZone(this DateTime dateTime)
    {
        const string easternZoneId = "Eastern Standard Time";
        var easternZone = TimeZoneInfo.FindSystemTimeZoneById(easternZoneId);
        var result = TimeZoneInfo.ConvertTime(dateTime, TimeZoneInfo.Local, easternZone);
        return result;
    }
}