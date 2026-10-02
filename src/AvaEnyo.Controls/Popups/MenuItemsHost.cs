using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using AvaEnyo.Controls.Primitives;

namespace AvaEnyo.Controls;

/// <summary>
/// The list inside a <see cref="Menu"/>: the top-level items in an <see cref="OrderedPanel"/>. Items that are not
/// <see cref="MenuItem"/>s get one, with the item as caption.
/// </summary>
public class MenuItemsHost : ItemsControl
{
    private static readonly FuncTemplate<Panel?> s_panel = new(() => new OrderedPanel());

    static MenuItemsHost()
    {
        ItemsPanelProperty.OverrideDefaultValue<MenuItemsHost>(s_panel);
    }

    /// <inheritdoc/>
    protected override System.Type StyleKeyOverride => typeof(ItemsControl);

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) =>
        NeedsContainer<MenuItem>(item, out recycleKey);

    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => new MenuItem();

    /// <inheritdoc/>
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is MenuItem menuItem)
        {
            menuItem.Depth = 0;
            if (!ReferenceEquals(menuItem, item) && !menuItem.IsSet(HeaderedItemsControl.HeaderProperty))
            {
                menuItem.Header = item;
            }
        }
    }
}
