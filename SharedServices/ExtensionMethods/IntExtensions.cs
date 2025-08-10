using System;

namespace BruSoftware.SharedServices.ExtensionMethods;

public static class IntExtensions
{
    /// <summary>
    /// Return a valid DateTime, of DateTime.MinValue if yyyymmdd is invalid
    /// Somehow this can happen on JSON deserialization
    /// </summary>
    /// <param name="yyyymmdd"></param>
    /// <returns></returns>
    public static DateTime FromYYYYMMDD(this int yyyymmdd)
    {
        if (yyyymmdd > 99991231 || yyyymmdd < 19000101)
        {
            return DateTime.MinValue;
        }
        var year = yyyymmdd / 10000;
        var month = yyyymmdd % 10000 / 100;
        var day = yyyymmdd % 100;
        try
        {
            return new DateTime(year, month, day);
        }
        catch
        {
            // IGNORING EXCEPTION
            return DateTime.MinValue;
        }
    }

    public static DateTime FromYYYYMM(this int yyyymm)
    {
        if (yyyymm == 0)
        {
            return DateTime.MinValue;
        }
        var year = yyyymm / 100;
        var month = yyyymm % 100;
        return new DateTime(year, month, 1);
    }
}