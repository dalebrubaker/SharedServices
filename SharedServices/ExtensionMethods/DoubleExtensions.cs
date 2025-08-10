
using System.Runtime.CompilerServices;
// ReSharper disable once CheckNamespace
using System;

namespace BruSoftware.SharedServices;

/// <summary>
/// Handle doubles precision comparison
/// </summary>
public static class DoubleExtensions
{
    private static readonly double _epsilon;

    static DoubleExtensions()
    {
        // From CoPilot
        _epsilon = 1.0;
        while (1.0 + _epsilon / 2.0 != 1.0)
        {
            _epsilon /= 2.0;
        }
    }

    /*
     * BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4169/23H2/2023Update/SunValley3)
        Intel Core i7-14700, 1 CPU, 28 logical and 20 physical cores
        .NET SDK 8.0.400
          [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
          DefaultJob : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
```
        | Method              | Mean     | Error   | StdDev  |
        |-------------------- |---------:|--------:|--------:|
        | TestAreDoublesEqual | 475.3 μs | 1.08 μs | 0.96 μs |
        | TestIsEqual         | 476.8 μs | 0.85 μs | 0.79 μs |

     */
    /// <summary>
    /// Benchmarking shows that this is essentially the same speed as !(x GT y) &amp;&amp; !(y LT x)
    /// See BenchmarkDoublesPrecision.sln
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEqual(this double a, double b)
    {
        return Math.Abs(a - b) < _epsilon;
    }

    /// <summary>
    /// Benchmarking shows that this is the fastest way to compare two doubles for equality, much better than !(x GT y) &amp;&amp; !(y LT x)
    /// See BenchmarkDoublesPrecision.sln
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNotEqual(this double a, double b)
    {
        return !IsEqual(a, b);
    }

    /// <summary>
    /// Return true for value equal to 0.0 using the c++ approach not(x less than y) and not(y less than x)
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsZero(this double value)
    {
        if (value < 0)
        {
            return false;
        }
        if (0 < value)
        {
            return false;
        }
        return true;
    }

    public static bool IsNaN(this double value)
    {
        var result = double.IsNaN(value);
        return result;
    }

    public static bool IsInfinity(this double value)
    {
        var result = double.IsInfinity(value);
        return result;
    }

    public static bool IsNegativeInfinity(this double value)
    {
        var result = double.IsNegativeInfinity(value);
        return result;
    }

    public static bool IsPositiveInfinity(this double value)
    {
        var result = double.IsPositiveInfinity(value);
        return result;
    }

    /// <summary>
    /// Return true for value equal to double.MinValue
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsMinValue(this double value)
    {
        if (value > double.MinValue)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Return true for value equal to double.MaxValue
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsMaxValue(this double value)
    {
        if (value < double.MaxValue)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Return the number of decimal places
    /// http://stackoverflow.com/questions/13477689/find-number-of-decimal-places-in-decimal-value-regardless-of-culture
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int DecimalPlaces(this double value)
    {
        var result = BitConverter.GetBytes(decimal.GetBits((decimal)value)[3])[2];
        return result;
    }
}