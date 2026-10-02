using Avalonia;

namespace AvaEnyo.Controls;

/// <summary>The presenter of a <see cref="ModalDialog"/>: the <c>dialog.png</c> frame with the caption row.</summary>
public class ModalDialogPresenter : PopupPresenter
{
    /// <summary>Defines the <see cref="Caption"/> property.</summary>
    public static readonly StyledProperty<string?> CaptionProperty =
        AvaloniaProperty.Register<ModalDialogPresenter, string?>(nameof(Caption));

    /// <summary>The dialog caption, mirrored from the dialog.</summary>
    public string? Caption
    {
        get => GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }
}
