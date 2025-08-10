using System;
using System.Diagnostics;

namespace BruSoftware.SharedServices.ExtensionMethods;

/// <summary>
/// Thanks to https://stackoverflow.com/questions/232848/wrapping-stopwatch-timing-with-a-delegate-or-lambda
/// </summary>
public static class StopwatchExtensions
{
    public static long Time(this Stopwatch sw, Action action, int iterations)
    {
        var total = 0L;
        sw.Reset();
        sw.Start();
        for (var i = 0; i < iterations; i++)
        {
            action();
            total += sw.ElapsedMilliseconds;
            sw.Restart();
        }
        sw.Stop();

        return total / iterations;
    }

    public static void TimeConsole(this Stopwatch sw, Action action, int iterations, string text)
    {
        var total = 0L;
        sw.Reset();
        sw.Start();
        for (var i = 0; i < iterations; i++)
        {
            action();
            var elapsed = sw.ElapsedMilliseconds;
            total += elapsed;
            Console.WriteLine($"iteration {i + 1} of {iterations} of {text} in {elapsed} ms.");
            sw.Restart();
        }
        sw.Stop();
        Console.WriteLine($"{iterations} of {text} in {total / iterations} ms avg.");
    }
}