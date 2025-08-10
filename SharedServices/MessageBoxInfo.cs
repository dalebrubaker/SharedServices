namespace BruSoftware.SharedServices;

public static class MessageBoxInfo
{
    public static void Show(string text, bool nonModal = false)
    {
        var messageDialog = ServiceLocator.GetServiceOrNull<IMessageDialog>();
        messageDialog?.Show(text, "", DialogStyle.Info, DialogButtons.Ok, nonModal);
    }

    public static void Show(string text, string caption, bool nonModal = false)
    {
        var messageDialog = ServiceLocator.GetServiceOrNull<IMessageDialog>();
        messageDialog?.Show(text, caption, DialogStyle.Info, DialogButtons.Ok, nonModal);
    }
}