using AvaEnyo.Controls;

namespace AvaEnyo.Mail.Views;

/// <summary>
/// A ModalDialog sized for writing mail: wider than the 320 px palmstyle dialog, the width of a TouchPad sheet.
/// </summary>
public sealed class ComposeSheet : ModalDialog
{
    /// <summary>The sheet width.</summary>
    public double SheetWidth { get; set; } = 560;

    /// <inheritdoc/>
    protected override void OnPresenterAttached(PopupPresenter presenter)
    {
        base.OnPresenterAttached(presenter);
        presenter.Width = SheetWidth;
    }
}
