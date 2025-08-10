namespace BruSoftware.SharedServices;

public static class MessageBoxWarn
{
    public static void Show(string text, bool nonModal = false)
    {
        if (GlobalsShared.IsUnitTesting)
        {
            return;
        }
        var messageDialog = ServiceLocator.GetServiceOrNull<IMessageDialog>();
        messageDialog?.Show(text, "", DialogStyle.Warning, DialogButtons.Ok, nonModal);
    }

    public static void Show(string text, string caption, bool nonModal = false)
    {
        if (GlobalsShared.IsUnitTesting)
        {
            return;
        }
        var messageDialog = ServiceLocator.GetServiceOrNull<IMessageDialog>();
        messageDialog?.Show(text, "", DialogStyle.Warning, DialogButtons.Ok, nonModal);
    }
}