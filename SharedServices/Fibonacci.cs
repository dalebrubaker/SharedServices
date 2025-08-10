using System;

namespace BruSoftware.SharedServices;

/// <summary>
/// Methods for dealing with Fibonacci numbers.
/// </summary>
[Serializable]
public static class Fibonacci
{
    public const int FibonacciIndexMax = 26;
    public const int MaxValue = 317811;

    // ReSharper disable once IdentifierTypo
    private static readonly int[] s_fibonaccis;

    static Fibonacci()
    {
        s_fibonaccis = new[]
        {
            1,
            2,
            3,
            5,
            8,
            13,
            21,
            34,
            55,
            89,
            144,
            233,
            377,
            610,
            987,
            1597,
            2584,
            4181,
            6765,
            10946,
            17711,
            28657,
            46368,
            75025,
            121393,
            196418,
            317811
        };
    }

    /// <summary>
    /// Return the calculated fibonnaci number for index, where indices 0,1,2,3,4,5,6,7,8,9,10... correspond to Fibonacci numbers
    /// 1,2,3,5,8,13,21,34,55,89,144...
    /// Fibonacci: 0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 233, 377, 610, 987, 1597, 2584, 4181, 6765, 10946, 17711, 28657, 46368, 75025, 121393,
    /// 196418, 317811
    /// Index:           0, 1, 2, 3, 4,  5,  6,  7,  8,  9,  10,  11,  12,  13,  14,   15,   16,   17,   18,    19,    20,    21,    22,    23,     24,
    /// 25,     26
    /// </summary>
    /// <param name="index"></param>
    public static int GetFibonacciSlow(int index)
    {
        if (index < 0)
        {
            return 1;
        }
        var prevPrev = 0;
        var prev = 1;
        var result = 0;
        for (var i = 0; i <= index; i++)
        {
            result = prevPrev + prev;
            prevPrev = prev;
            prev = result;
        }
        return result;
    }

    /// <summary>
    /// Return the Fibonacci number for index, up to FibonacciIndexMax, where indices 0,1,2,3,4,5,6,7,8,9,10... correspond to Fibonacci numbers 1,2,3,5,8...
    /// Fibonacci: 0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 233, 377, 610, 987, 1597, 2584, 4181, 6765, 10946, 17711, 28657, 46368, 75025, 121393,
    /// 196418, 317811
    /// Index:           0, 1, 2, 3, 4,  5,  6,  7,  8,  9,  10,  11,  12,  13,  14,   15,   16,   17,   18,    19,    20,    21,    22,    23,     24,
    /// 25,     26
    /// This fast version goes no higher than index 26 (Fibonacci 317811)
    /// </summary>
    /// <param name="index"></param>
    public static int GetFibonacciFast(int index)
    {
        if (index < 0 || index > FibonacciIndexMax)
        {
            throw new ArgumentException($"Maximum Fibonacci index is {FibonacciIndexMax}");
        }
        return s_fibonaccis[index];
    }

    public static int GetIndexFastNotAbove(int length)
    {
        for (var i = s_fibonaccis.Length - 1; i >= 0; i--)
        {
            if (s_fibonaccis[i] <= length)
            {
                return i;
            }
        }
        throw new ArgumentException($"Maximum Fibonacci index is {FibonacciIndexMax}");
    }

    public static int GetFibonacciLonger(int period)
    {
        for (var i = 0; i < s_fibonaccis.Length; i++)
        {
            var result = s_fibonaccis[i];
            if (result > period)
            {
                return result;
            }
        }
        throw new ArgumentException($"Maximum Fibonacci period is {s_fibonaccis[^1]}");
    }

    public static int GetFibonacciShorter(int period)
    {
        for (var i = s_fibonaccis.Length - 1; i >= 0; i--)
        {
            var result = s_fibonaccis[i];
            if (result < period)
            {
                return result;
            }
        }
        throw new ArgumentException($"Maximum Fibonacci period is {s_fibonaccis[^1]}");
    }
}