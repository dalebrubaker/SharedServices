namespace BruSoftware.SharedServices;

public enum DialogStyle
{
    Info,
    Warning,
    Error,
    Question
}

public enum DialogButtons
{
    Ok,
    OkCancel,
    YesNo,
    YesNoCancel
}

public enum MessageDialogResult
{
    None,
    Ok,
    Cancel,
    Yes,
    No
}

public interface IMessageDialog
{
    /// <summary>Blocking (modal) dialog – returns when the user responds.</summary>
    MessageDialogResult Show(string text,
        string title = "",
        DialogStyle style = DialogStyle.Info,
        DialogButtons buttons = DialogButtons.Ok,
        bool nonModal = false);

    bool IsYes(string text, string caption);
}