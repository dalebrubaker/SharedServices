using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BruSoftware.SharedServices;
using Xunit;
using Xunit.Abstractions;

namespace BruSoftware.SharedServicesTests;

public class ClockTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ClockTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact(Skip = "Not Used")]
    public async Task StartTest()
    {
        const int Delay = 100;
        const int Speed = 2;
        var startNow = DateTime.UtcNow;
        var sw = Stopwatch.StartNew();
        var clock = new Clock();
        clock.Start(startNow, Speed);
        await Task.Delay(Delay);
        var actualDelayMsecs = sw.ElapsedMilliseconds;
        var now = clock.UtcNow;
        var expectedMinimumNow = startNow.AddMilliseconds(Speed * Delay).AddMilliseconds(-2); // allow 2 msec error
        var diff = now - startNow;
        var elapsed = sw.ElapsedMilliseconds;
        _testOutputHelper.WriteLine(
            $"Clocked delayed diff.TotalMilliseconds={diff.TotalMilliseconds} ms during {actualDelayMsecs} ms, elapsed={elapsed}");
        Assert.True(now >= expectedMinimumNow,
            $"now={now:yyyyMMdd.HHmmss.fffffff} < expectedMinimumNow={expectedMinimumNow:yyyyMMdd.HHmmss.fffffff}");
    }

    [Fact(Skip = "Flaky")]
    public async Task PauseTest()
    {
        const int Delay = 100;
        const int Speed = 2;
        var startNow = DateTime.UtcNow;
        var sw = Stopwatch.StartNew();
        var clock = new Clock();
        clock.Start(startNow, Speed);
        await Task.Delay(Delay);
        var actualDelayMsecs = sw.ElapsedMilliseconds;
        clock.PauseToggle();
        await Task.Delay(Delay * 2);
        clock.PauseToggle();
        var actualDelayToRestartMsecs = sw.ElapsedMilliseconds;
        var now = clock.UtcNow;
        var expectedMinimumNowWithoutPause = startNow.AddMilliseconds(Speed * Delay);
        var diff = now - startNow;
        Assert.True(now >= expectedMinimumNowWithoutPause,
            $"now={now:O} should be >= expectedMinimumNowWithoutPause={expectedMinimumNowWithoutPause:O}");
        var elapsed = sw.ElapsedMilliseconds;
        var msg =
            $"Clocked delayed diff.TotalMilliseconds={diff.TotalMilliseconds} ms during actualDelayToRestartMsecs={actualDelayToRestartMsecs} ms, elapsed={elapsed}";
        _testOutputHelper.WriteLine(msg);
        Assert.True(diff.TotalMilliseconds <= elapsed, msg);
    }

    // [Fact]
    // public void DebugJenkinsEnvironment()
    // {
    //     _testOutputHelper.WriteLine($"Current User: {Environment.UserName}");
    //     _testOutputHelper.WriteLine($"User Domain: {Environment.UserDomainName}");
    //     _testOutputHelper.WriteLine($"AppData Path: {Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}");
    //     _testOutputHelper.WriteLine($"Current Directory: {Directory.GetCurrentDirectory()}");
    //
    //     var configPath = Path.Combine(
    //         Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    //         "BruSoftware",
    //         "BruTrader.settings.json"
    //     );
    //     _testOutputHelper.WriteLine($"Looking for config at: {configPath}");
    //     _testOutputHelper.WriteLine($"Config exists: {File.Exists(configPath)}");
    // }
}