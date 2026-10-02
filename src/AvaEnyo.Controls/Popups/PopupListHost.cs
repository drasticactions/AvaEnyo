using System;
using Avalonia.Controls;

namespace AvaEnyo.Controls;

/// <summary>
/// The list inside a <see cref="PopupListBase"/>: each item gets a <see cref="PopupListItem"/>; the owner's selected
/// index marks one row selected.
/// </summary>
public class PopupListHost : ItemsControl
{
    internal PopupListBase? Owner { get; set; }

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(ItemsControl);

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) =>
        NeedsContainer<PopupListItem>(item, out recycleKey);

    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => new PopupListItem();

    /// <inheritdoc/>
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is PopupListItem row)
        {
            row.IsSelected = Owner is { } o && o.SelectedIndex == index;
        }
    }

    /// <summary>Re-marks the selected row.</summary>
    internal void UpdateSelection(int selected)
    {
        for (var i = 0; i < ItemCount; i++)
        {
            if (ContainerFromIndex(i) is PopupListItem row)
            {
                row.IsSelected = i == selected;
            }
        }
    }
}
