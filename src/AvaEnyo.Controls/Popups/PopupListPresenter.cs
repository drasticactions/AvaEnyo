using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AvaEnyo.Controls;

/// <summary>The presenter of a <see cref="PopupListBase"/>: the menu frame around the rows; a tapped row is picked.</summary>
public class PopupListPresenter : PopupPresenter
{
    static PopupListPresenter()
    {
        Item.ClickEvent.AddClassHandler<PopupListPresenter>((p, e) => p.OnRowClick(e));
    }

    private void OnRowClick(RoutedEventArgs e)
    {
        if (e.Source is PopupListItem row && Owner is PopupListBase list && row.FindAncestorOfType<PopupListHost>() is { } host)
        {
            e.Handled = true;
            list.Select(host.IndexFromContainer(row));
        }
    }
}
