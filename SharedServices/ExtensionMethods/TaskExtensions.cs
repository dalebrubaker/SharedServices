using System;
using System.Threading.Tasks;
using NLog;

namespace BruSoftware.SharedServices.ExtensionMethods;

public static class TaskExtensions
{
    private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Thanks to https://www.meziantou.net/fire-and-forget-a-task-in-dotnet.htm
    /// </summary>
    /// <param name="task"></param>
    public static void Forget(this Task task)
    {
        // note: this code is inspired by a tweet from Ben Adams: https://twitter.com/ben_a_adams/status/1045060828700037125
        // Only care about tasks that may fault (not completed) or are faulted,
        // so fast-path for SuccessfullyCompleted and Canceled tasks.
        if (!task.IsCompleted || task.IsFaulted)
        {
            // use "_" (Discard operation) to remove the warning IDE0058: Because this call is not awaited, execution of the current method continues before the call is completed
            // https://docs.microsoft.com/en-us/dotnet/csharp/discards#a-standalone-discard
            _ = ForgetAwaited(task);
        }

        // Allocate the async/await state machine only when needed for performance reason.
        // More info about the state machine: https://blogs.msdn.microsoft.com/seteplia/2017/11/30/dissecting-the-async-methods-in-c/?WT.mc_id=DT-MVP-5003978
        static async Task ForgetAwaited(Task task)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (AggregateException ae)
            {
                var msg = $"Fatal Task.Forget()Exception {ae}";
                ae.Flatten().Handle(ex =>
                {
                    msg += $" {ex.Message}";
                    return true;
                });
                s_logger.Error("{Msg}", msg);
            }
        }
    }
}