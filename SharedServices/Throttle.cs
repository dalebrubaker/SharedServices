using System;
using System.Threading;
using System.Threading.Tasks;
using BruSoftware.SharedServices.ExtensionMethods;
using NLog;

namespace BruSoftware.SharedServices;

public class Throttle<T> : IDisposable where T : class
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

    private readonly bool _isDelayBeforeSend;
    private readonly int _delayMs;
    private readonly bool _ignoreDuplicates;
    private T _receivedValue;

    // ReSharper disable once IdentifierTypo
    private ManualResetEventSlim _mres = new(true);

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="isDelayBeforeSend">
    /// <c>true</c> means to ignore added values for <see cref="_delayMs" /> milliseconds after the previous one was added.
    /// So the first one added is NOT handled (thrown in <see cref="ThrottledOutput" />) for <see cref="_delayMs" /> milliseconds.
    /// <c>false</c> means to ignore added values for <see cref="_delayMs" /> milliseconds after the previous one was added.
    /// So the first one is handled (thrown in <see cref="ThrottledOutput" />) and then after <see cref="_delayMs" /> milliseconds
    /// the most recent one added, if any, is handled.
    /// </param>
    /// <param name="delayMs">
    /// The delay in milliseconds during which this throttle
    /// will ignore incoming events, returning only the latest one via _callback
    /// </param>
    /// <param name="ignoreDuplicates">ignore a duplicate to the last item received</param>
    /// <exception cref="ArgumentException"></exception>
    public Throttle(bool isDelayBeforeSend, int delayMs, bool ignoreDuplicates)
    {
        if (delayMs <= 0)
        {
            throw new ArgumentException("must not be negative", nameof(delayMs));
        }
        _isDelayBeforeSend = isDelayBeforeSend;
        _delayMs = delayMs;
        _ignoreDuplicates = ignoreDuplicates;
        Task.Factory.StartNew(ThrottleLoop, TaskCreationOptions.LongRunning).Forget();
    }

    /// <summary>
    /// Add an value into this <see cref="Throttle{T}" />.
    /// </summary>
    /// <param name="value"></param>
    public void Add(T value)
    {
        //s_logger.ConditionalDebug($"Received {value} in {this}");
        if (_ignoreDuplicates && value.Equals(_receivedValue))
        {
            //s_logger.ConditionalDebug($"Ignoring duplicate {value} at {DateTime.UtcNow} in {this}");
            return;
        }
        Interlocked.Exchange(ref _receivedValue, value);
        //s_logger.ConditionalDebug($"Add Did InterlockedExchange and set _receivedValue={_receivedValue} to value={value}");
        _mres.Set();
    }

    private void ThrottleLoop()
    {
        while (true)
        {
            if (_isDelayBeforeSend)
            {
                //s_logger.ConditionalDebug($"Before looking for a value, ThrottleLoop will sleep for {_delayMs}");
                Thread.Sleep(_delayMs);
            }

            // get the last _receivedValue and set it to null
            var value = Interlocked.Exchange(ref _receivedValue, null);
            //s_logger.ConditionalDebug($"ThrottleLoop Did InterlockedExchange and got value={value}, _receivedValue={_receivedValue}");
            if (value != null)
            {
                //s_logger.ConditionalDebug($"Calling OnThrottleOutput({value})");
                OnThrottleOutput(value);
            }
            else
            {
                // No value was added since we last checked
                // Block this loop until another value is added
                _mres?.Reset();
                //s_logger.ConditionalDebug("Blocking until a value is Added");
                _mres?.Wait();
                //s_logger.ConditionalDebug("No longer blocking");
            }
            if (!_isDelayBeforeSend)
            {
                //s_logger.ConditionalDebug($"After looking for a value, ThrottleLoop will sleep for {_delayMs}");
                Thread.Sleep(_delayMs);
            }
        }
    }

    public event EventHandler<T> ThrottledOutput;

    private void OnThrottleOutput(T args)
    {
        var tmp = ThrottledOutput;
        // if (tmp != null)
        // {
        //     s_logger.ConditionalDebug($"Sending event {args}");
        // }
        // else
        // {
        //     s_logger.ConditionalDebug("ThrottledOutput is null");
        //         
        // }
        tmp?.Invoke(this, args);
        if (tmp != null)
        {
            //s_logger.ConditionalDebug($"Sent event {args}");
        }
    }

    public void Dispose()
    {
        _mres = null;
        _mres?.Dispose();
        GC.SuppressFinalize(this);
    }
}