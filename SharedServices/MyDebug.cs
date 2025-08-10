using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using NLog;

namespace BruSoftware.SharedServices;

public static class MyDebug
{
#if DEBUG
    private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();
    private static readonly object s_lock = new();

    public static void Assert(bool condition, string message = null,
        [CallerMemberName] string callingMethod = "", [CallerFilePath] string callingFilePath = "", [CallerLineNumber] int callingFileLineNumber = 0)
    {
        lock (s_lock)
        {
            if (condition)
            {
                return;
            }
            if (string.IsNullOrEmpty(message))
            {
                message = $"Failed {nameof(MyDebug)}.{nameof(Assert)} {Environment.StackTrace}";
            }
            s_logger.Error(message + $" Caller: {callingMethod} F:{callingFilePath} L:{callingFileLineNumber}");
            Debugger.Break();
            //Debug.Assert(condition, message); // do the normal assert, launch debugger when not in the IDE
            throw new SharedServicesException(message);
        }
    }

#else // NO-OP for Release
    public static void Assert(bool condition, string message = null,
        [CallerMemberName] string callingMethod = "", [CallerFilePath] string callingFilePath = "", [CallerLineNumber] int callingFileLineNumber = 0)
    {
    }
#endif
}