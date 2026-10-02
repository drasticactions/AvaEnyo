using Avalonia.Interactivity;

namespace AvaEnyo.Controls;

/// <summary>
/// The presenter of a <see cref="Menu"/>: the <c>menu-background.png</c> frame around a scrolling list. A tap on a
/// leaf <see cref="MenuItem"/> closes the menu with that item's value.
/// </summary>
public class MenuPresenter : PopupPresenter
{
    static MenuPresenter()
    {
        MenuItem.ClickEvent.AddClassHandler<MenuPresenter>((p, e) => p.OnItemClick(e));
    }

    /// <summary>Called when a leaf item inside is tapped. Closes the owning popup with the item's value.</summary>
    protected virtual void OnItemClick(RoutedEventArgs e)
    {
        if (e.Source is MenuItem item && Owner is { } owner)
        {
            owner.CloseCore(PopupCloseReason.Handle, item.EffectiveValue);
        }
    }
}
