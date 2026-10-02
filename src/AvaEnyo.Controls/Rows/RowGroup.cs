using Avalonia.Controls;

namespace AvaEnyo.Controls;

/// <summary>
/// A <see cref="Group"/> of rows (<c>enyo.RowGroup</c>). A child that is not an <see cref="Item"/> is wrapped in a
/// <see cref="RowItem"/>, so every child is a row with order classes; a text box in a row gets the first, middle or
/// last focus frame.
/// </summary>
public class RowGroup : Group
{
    /// <summary>Shows the row at <paramref name="index"/>.</summary>
    public void ShowRow(int index) => SetRowVisible(index, true);

    /// <summary>Hides the row at <paramref name="index"/>; the remaining rows take the order classes.</summary>
    public void HideRow(int index) => SetRowVisible(index, false);

    /// <summary>The row control at <paramref name="index"/>, or null.</summary>
    public Item? RowAt(int index) => ContainerFromIndex(index) as Item;

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) =>
        NeedsContainer<Item>(item, out recycleKey);

    /// <inheritdoc/>
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);

        // .enyo-item > .enyo-input: a text box in a row pulls in over the row padding. The text box is a logical
        // child of the group, not of its row, so the class is set here instead of by a child selector.
        if (item is TextBox box)
        {
            box.Classes.Set("in-row", true);
        }
    }

    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => new RowItem();

    private void SetRowVisible(int index, bool visible)
    {
        if (ContainerFromIndex(index) is { } row)
        {
            row.IsVisible = visible;
        }
    }
}
