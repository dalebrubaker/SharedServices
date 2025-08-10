using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BruSoftware.SharedServices;
using FluentAssertions;
using NLog;
using Xunit;

namespace BruSoftware.SharedServicesTests;

internal class IdDateTime
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }

    public override string ToString()
    {
        return $"{Id}"; // {Timestamp.ToTimestampStringSortsWithTicks()}";
    }
}

public class ThrottleTests
{
    private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

    [Fact(Skip = "Timing-sensitive test - can be flaky on different systems")]
    public async Task DelayAfterSendTests()
    {
        const int NumValues = 10;
        using var throttle = new Throttle<IdDateTime>(false, 10, true);
        var listIn = new List<IdDateTime>();
        var listOut = new List<IdDateTime>();

        var timestamp = DateTime.MinValue;
        for (var i = 0; i < NumValues; i++)
        {
            var value = new IdDateTime { Id = 10 * (i + 1), Timestamp = timestamp };
            listIn.Add(value);
            timestamp = timestamp.AddTicks(1);
        }
        throttle.ThrottledOutput += (sender, tuple) =>
        {
            var id = tuple.Id;
            var timestampOut = tuple.Timestamp;
            listOut.Add(tuple);
            //s_logger.ConditionalDebug($"Received event {tuple}");
        };

        foreach (var value in listIn)
        {
            await Task.Delay(5);
            throttle.Add(value);
        }

        //s_logger.ConditionalDebug("Waiting 1 second for more output");
        await Task.Delay(1000);
        listOut.Should().NotBeEmpty();
        //s_logger.Info($"listOut.Count={listOut.Count} vs listIn.Count={listIn.Count}");
        listOut.Count.Should().BeLessThanOrEqualTo(listIn.Count);
    }

    [Fact(Skip = "Timing-sensitive test - can be flaky on different systems")]
    public async Task DelayBeforeSendTests()
    {
        const int NumValues = 10;
        using var throttle = new Throttle<IdDateTime>(true, 10, true);
        var listIn = new List<IdDateTime>();
        var listOut = new List<IdDateTime>();

        var timestamp = DateTime.MinValue;
        for (var i = 0; i < NumValues; i++)
        {
            var value = new IdDateTime { Id = 10 * (i + 1), Timestamp = timestamp };
            listIn.Add(value);
            timestamp = timestamp.AddTicks(1);
        }
        throttle.ThrottledOutput += (sender, tuple) =>
        {
            var id = tuple.Id;
            var timestampOut = tuple.Timestamp;
            listOut.Add(tuple);
            //s_logger.ConditionalDebug($"Received event {tuple}");
        };

        foreach (var value in listIn)
        {
            await Task.Delay(5);
            throttle.Add(value);
        }

        //s_logger.ConditionalDebug("Waiting 1 second for more output");
        await Task.Delay(1000);
        listOut.Should().NotBeEmpty();
        //s_logger.Info($"listOut.Count={listOut.Count} vs listIn.Count={listIn.Count}");
        listOut.Count.Should().BeLessThan(listIn.Count);
    }
}