namespace BruSoftware.SharedServices;

public static class MessageBoxYesNo
{
    public static bool IsYes(string text, string caption = "")
    {
        if (GlobalsShared.IsUnitTesting)
        {
            // In unit tests, we assume "No" is the answer.
            return false;
        }

        var messageDialog = ServiceLocator.GetServiceOrNull<IMessageDialog>();
        if (messageDialog == null)
        {
            // If no message dialog service is available, assume "No" is the answer.
            return false;
        }

        var dialogResult = messageDialog.IsYes(text, caption);
        return dialogResult;
    }
}