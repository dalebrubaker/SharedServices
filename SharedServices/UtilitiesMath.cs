using System;
using System.Collections.Generic;

namespace BruSoftware.SharedServices;

public static class UtilitiesMath
{
    public delegate double Fx(double x);

    public const double tol = 0.00000001;

    /// <summary>
    /// Return true if list1 crosses above list2 at index
    /// </summary>
    /// <param name="index"></param>
    /// <param name="list1"></param>
    /// <param name="list2"></param>
    /// <returns></returns>
    public static bool CrossesAbove(int index, List<float> list1, List<float> list2)
    {
        if (list1.Count < index - 1 || list2.Count < index - 1)
        {
            return false; // not enough elements yet to check
        }
        var list1Val = list1[index];
        var list1Prev = list1[index - 1];
        var list2Val = list2[index];
        var list2Prev = list2[index - 1];
        return list1Prev <= list2Prev && list1Val > list2Val;
    }

    /// <summary>
    /// Return true if list1 crosses below list2 at index
    /// </summary>
    /// <param name="index"></param>
    /// <param name="list1"></param>
    /// <param name="list2"></param>
    /// <returns></returns>
    public static bool CrossesBelow(int index, List<float> list1, List<float> list2)
    {
        if (list1.Count < index - 1 || list2.Count < index - 1)
        {
            return false; // not enough elements yet to check
        }
        var list1Val = list1[index];
        var list1Prev = list1[index - 1];
        var list2Val = list2[index];
        var list2Prev = list2[index - 1];
        return list1Prev >= list2Prev && list1Val < list2Val;
    }

    public static void Swap(ref double x, ref double y)
    {
        var _ = x;
        x = y;
        y = _;
    }

    public static void Swap(ref DateTime x, ref DateTime y)
    {
        var _ = x;
        x = y;
        y = _;
    }

    /// <summary>
    /// Round val to 0 decimals and the nearest spacing, and divisible by spacing.
    /// e.g. 848.25, 0, 5 -> 850
    /// </summary>
    /// <param name="val"></param>
    /// <param name="decimals"></param>
    /// <param name="spacing"></param>
    /// <returns></returns>
    public static decimal Round(decimal val, int decimals, decimal spacing)
    {
        if (spacing == 0)
        {
            throw new SharedServicesException("0 spacing in " + nameof(Round));
        }
        val = Math.Round(val, decimals);
        var scale = (int)Math.Pow(10, decimals);
        var valInt = (int)(val * scale);
        var spacingInt = (int)(spacing * scale);
        var prevDiff = int.MaxValue;
        var counter = spacingInt;
        var diff = valInt - counter;
        while (Math.Abs(diff) < Math.Abs(prevDiff))
        {
            counter += spacingInt;
            prevDiff = diff;
            diff = valInt - counter;
        }
        counter -= spacingInt; // step back to the smaller one
        var result = (decimal)counter / scale;
        return result;
    }

    /// <summary>
    /// Round val to the nearest multiple of interval
    /// </summary>
    /// <param name="val"></param>
    /// <param name="interval"></param>
    /// <returns></returns>
    public static decimal Round(decimal val, decimal interval)
    {
        var nearestInterval = (int)((val + interval / 2) / (int)interval) * (int)interval;
        decimal result = nearestInterval;
        return result;
    }

    /// <summary>
    /// Return true for value1 is approximately equal to value2 using ApproxCompare())
    /// </summary>
    /// <param name="value1"></param>
    /// <param name="value2"></param>
    /// <returns></returns>
    public static bool IsApproxEqual(this double value1, double value2)
    {
        var result = value1.ApproxCompare(value2) == 0;
        return result;
    }

    /// <summary>
    /// This is used for NT8 indicators and custom barsTypes
    /// Is in the NT8 Public APIvp
    /// </summary>
    /// <param name="value1"></param>
    /// <param name="value2"></param>
    /// <returns></returns>
    public static int ApproxCompare(this double value1, double value2)
    {
        if (value1 < value2 - 1E-10)
        {
            return -1;
        }
        if (value2 < value1 - 1E-10)
        {
            return 1;
        }
        return 0;
    }

    /// <summary>
    /// Thanks to http://stackoverflow.com/questions/1906525/c-generic-math-functions-min-max-etc
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static T Max<T>(T x, T y)
    {
        return Comparer<T>.Default.Compare(x, y) > 0 ? x : y;
    }

    /// <summary>
    /// Thanks to http://stackoverflow.com/questions/1906525/c-generic-math-functions-min-max-etc
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static T Min<T>(T x, T y)
    {
        return Comparer<T>.Default.Compare(x, y) < 0 ? x : y;
    }

    /// <summary>
    /// Returns <c>true</c> if x and y are both non-zero and have different signs
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static bool IsReversing(int x, int y)
    {
        if (x == 0 || y == 0)
        {
            return false;
        }
        return Math.Sign(x) != Math.Sign(y);
    }

    public static Fx composeFunctions(Fx f1, Fx f2)
    {
        return x => f1(x) + f2(x);
    }

    public static Fx f_xirr(double p, double dt, double dt0)
    {
        return x => p * Math.Pow(1.0 + x, (dt0 - dt) / 365.0);
    }

    public static Fx df_xirr(double payment, double dayN, double day0)
    {
        return x => 1.0 / 365.0 * (day0 - dayN) * payment * Math.Pow(x + 1.0, (day0 - dayN) / 365.0 - 1.0);
    }

    public static Fx total_f_xirr(List<double> payments, List<double> days)
    {
        Fx resf = x => 0.0;

        for (var i = 0; i < payments.Count; i++)
        {
            resf = composeFunctions(resf, f_xirr(payments[i], days[i], days[0]));
        }

        return resf;
    }

    /// <summary>
    /// Thanks to https://stackoverflow.com/questions/5179866/xirr-calculation
    /// </summary>
    /// <param name="payments"></param>
    /// <param name="days">days of payment (as day of year)</param>
    /// <returns></returns>
    public static Fx total_df_xirr(List<double> payments, List<double> days)
    {
        Fx resf = x => 0.0;

        for (var i = 0; i < payments.Count; i++)
        {
            resf = composeFunctions(resf, df_xirr(payments[i], days[i], days[0]));
        }

        return resf;
    }

    public static double Newtons_method(double guess, Fx f, Fx df)
    {
        var x0 = guess;
        var err = 1e+100;
        var iterationsCount = 0;
        while (err > tol && iterationsCount++ < 100)
        {
            var x1 = x0 - f(x0) / df(x0);
            err = Math.Abs(x1 - x0);
            x0 = x1;
        }

        return x0;
    }

    public static double XIrr(List<double> valList, List<DateTime> dates, double guess = 0.1)
    {
        if (dates.Count <= 0 || dates.Count != valList.Count)
        {
            return double.NaN;
        }
        var startDate = dates[0].Date;
        var dtList = new List<double>(dates.Count);
        foreach (var date in dates)
        {
            dtList.Add((date.Date - startDate.Date).TotalDays);
        }
        var xirr = Newtons_method(0.1,
            total_f_xirr(valList, dtList),
            total_df_xirr(valList, dtList));
        return xirr;
    }

    /// <summary>
    /// Thanks to https://stackoverflow.com/questions/64622424/interpolation-on-a-graph-where-x-is-logarithmic-semi-log-graph
    /// </summary>
    /// <param name="x0"></param>
    /// <param name="x1"></param>
    /// <param name="y0"></param>
    /// <param name="y1"></param>
    /// <param name="x"></param>
    /// <returns></returns>
    public static double LogLinearInterpolation(double x0, double x1, double y0, double y1, double x)
    {
        return LinearInterpolation(Math.Log(x0), Math.Log(x1), y0, y1, Math.Log(x));
    }

    /// <summary>
    /// Thanks to https://stackoverflow.com/questions/64622424/interpolation-on-a-graph-where-x-is-logarithmic-semi-log-graph
    /// </summary>
    /// <param name="x0"></param>
    /// <param name="x1"></param>
    /// <param name="y0"></param>
    /// <param name="y1"></param>
    /// <param name="x"></param>
    /// <returns></returns>
    public static double LinearInterpolation(double x0, double x1, double y0, double y1, double x)
    {
        var m = (x - x0) / (x1 - x0);
        return y0 * (1 - m) + y1 * m;
    }
}