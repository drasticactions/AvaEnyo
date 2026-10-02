namespace AvaEnyo.Controls;

/// <summary>
/// A bottom <see cref="Toaster"/> in the <c>toaster-dialog.png</c> frame (<c>enyo.Dialog</c>): a light panel with
/// rounded top corners that slides up from the bottom edge.
/// </summary>
public class Dialog : Toaster
{
    /// <inheritdoc/>
    protected override PopupPresenter CreatePresenter() => new DialogPresenter();
}
