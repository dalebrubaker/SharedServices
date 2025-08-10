namespace BruSoftware.SharedServices;

public static class GlobalsShared
{
    public static bool IsUnitTesting { get; set; }

    public static bool IsUnitTestingOverride { get; set; }

    /// <summary>
    /// Used by backtester or any other place we don't want user interaction
    /// </summary>
    public static bool IsAutoMode
    {
        get => field || IsUnitTesting;
        set;
    }

    /// <summary>
    /// We are backtesting a strategy.
    /// </summary>
    public static bool IsBacktesting { get; set; }

    /// <summary>
    /// We are backtesting a strategy. We don't want base bar historical updates nor realtime
    /// </summary>
    public static bool IsBacktestNoBaseBarsUpdates { get; set; }

    /// <summary>
    /// The thread id of the UI thread
    /// </summary>
    public static int UIThreadId { get; set; }
}