using System;

namespace BruSoftware.SharedServices;

/// <summary>
/// http://stackoverflow.com/questions/246498/creating-a-datetime-in-a-specific-time-zone-in-c-sharp-fx-3-5
/// </summary>
public struct DateTimeWithZone
{
    public DateTimeWithZone(DateTime dateTime, TimeZoneInfo timeZone)
    {
        UniversalTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified), timeZone);
        TimeZone = timeZone;
    }

    public DateTimeWithZone(DateTime dateTime, string timeZoneName) : this(dateTime, TimeZoneInfo.FindSystemTimeZoneById(timeZoneName))
    {
    }

    public DateTime UniversalTime { get; }

    public TimeZoneInfo TimeZone { get; }

    public DateTime LocalTime => TimeZoneInfo.ConvertTime(UniversalTime, TimeZone);

    public override string ToString()
    {
        return $"Local:{LocalTime} UTC: {UniversalTime} in {TimeZone}";
    }
}