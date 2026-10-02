using Avalonia;
using Avalonia.Controls;

namespace AvaEnyo.Controls;

/// <summary>
/// The visible part of a <see cref="PopupBase"/>: it lives in the overlay layer of the window while the popup is
/// open, and shows the popup's content in the popup's frame. Each popup type has its own presenter theme.
/// </summary>
public class PopupPresenter : ContentControl
{
    /// <summary>Defines the <see cref="Owner"/> property.</summary>
    public static readonly DirectProperty<PopupPresenter, PopupBase?> OwnerProperty =
        AvaloniaProperty.RegisterDirect<PopupPresenter, PopupBase?>(nameof(Owner), o => o.Owner);

    private PopupBase? _owner;

    /// <summary>The popup this presenter shows.</summary>
    public PopupBase? Owner
    {
        get => _owner;
        internal set => SetAndRaise(OwnerProperty, ref _owner, value);
    }
}
