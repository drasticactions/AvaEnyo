using System.Collections;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Metadata;

namespace AvaEnyo.Controls;

/// <summary>
/// The Onyx pop-up menu (<c>enyo.Menu</c>): a modal popup in the <c>menu-background.png</c> frame whose rows are
/// <see cref="MenuItem"/>s in a scrolling body with arrow fades. Tapping a leaf item closes the menu with reason
/// <see cref="PopupCloseReason.Handle"/> and the item's value. Opening the menu closes every open sub-drawer first
/// (<see cref="AutoCloseSubItems"/>). In XAML use <c>enyo:Menu</c>; the short name also exists in Avalonia.
/// </summary>
public class Menu : PopupBase
{
    /// <summary>Defines the <see cref="ItemsSource"/> property.</summary>
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<Menu, IEnumerable?>(nameof(ItemsSource));

    /// <summary>Defines the <see cref="AutoCloseSubItems"/> property.</summary>
    public static readonly StyledProperty<bool> AutoCloseSubItemsProperty =
        AvaloniaProperty.Register<Menu, bool>(nameof(AutoCloseSubItems), true);

    private MenuItemsHost? _host;

    static Menu()
    {
        IsModalProperty.OverrideDefaultValue<Menu>(true);
        ScrimWhenModalProperty.OverrideDefaultValue<Menu>(false);
        ItemsSourceProperty.Changed.AddClassHandler<Menu>((m, _) => m.SyncHost());
    }

    /// <summary>Initializes a new instance of the <see cref="Menu"/> class.</summary>
    public Menu()
    {
        // A modal menu still closes on an outside click, as Enyo's menus do.
        DismissWithClick = true;
    }

    /// <summary>The rows: <see cref="MenuItem"/>s, or any objects shown as captions.</summary>
    [Content]
    public AvaloniaList<object> Items { get; } = new();

    /// <summary>Rows from a collection; when set, used instead of <see cref="Items"/>.</summary>
    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>True (default): opening the menu closes the drawers of nested items.</summary>
    public bool AutoCloseSubItems
    {
        get => GetValue(AutoCloseSubItemsProperty);
        set => SetValue(AutoCloseSubItemsProperty, value);
    }

    /// <summary>The menu item whose value equals <paramref name="value"/>, or null (Enyo <c>fetchItemByValue</c>).</summary>
    public MenuItem? FetchItemByValue(object? value)
    {
        foreach (var item in ItemsSource ?? Items)
        {
            if (item is MenuItem m && Equals(m.EffectiveValue, value))
            {
                return m;
            }
        }

        return null;
    }

    /// <inheritdoc/>
    protected override PopupPresenter CreatePresenter()
    {
        if (AutoCloseSubItems)
        {
            foreach (var item in ItemsSource ?? Items)
            {
                (item as MenuItem)?.CloseAll();
            }
        }

        SyncHost();
        Content = _host;
        return new MenuPresenter();
    }

    private void SyncHost()
    {
        _host ??= new MenuItemsHost();
        _host.ItemsSource = ItemsSource ?? Items;
    }
}
