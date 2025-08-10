using System;
using System.Threading;
using System.Threading.Tasks;

namespace BruSoftware.SharedServices;

/// <summary>
/// Thanks to https://cpratt.co/async-tips-tricks/
/// </summary>
public static class AsyncHelper
{
    private static readonly TaskFactory _TaskFactory = new(CancellationToken.None,
        TaskCreationOptions.None,
        TaskContinuationOptions.None,
        TaskScheduler.Default);

    public static TResult RunSync<TResult>(Func<Task<TResult>> func)
    {
        return _TaskFactory
            .StartNew(func)
            .Unwrap()
            .GetAwaiter()
            .GetResult();
    }

    public static void RunSync(Func<Task> func)
    {
        _TaskFactory
            .StartNew(func)
            .Unwrap()
            .GetAwaiter()
            .GetResult();
    }
}