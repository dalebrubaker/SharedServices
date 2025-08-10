using NLog;

namespace BruSoftware.SharedServices;

public static class MessageBoxError
{
    private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

    public static void Show(string text, bool nonModal = false)
    {
        if (GlobalsShared.IsUnitTesting)
        {
            return;
        }
        s_logger.Error(text);
        var messageDialog = ServiceLocator.GetServiceOrNull<IMessageDialog>();
        messageDialog?.Show(text, "", DialogStyle.Error, DialogButtons.Ok, nonModal);
    }

    public static void Show(string text, string caption, bool nonModal = false)
    {
        if (GlobalsShared.IsUnitTesting)
        {
            return;
        }
        s_logger.Error(text);
        var messageDialog = ServiceLocator.GetServiceOrNull<IMessageDialog>();
        messageDialog?.Show(text, caption, DialogStyle.Error, DialogButtons.Ok, nonModal);
    }
}