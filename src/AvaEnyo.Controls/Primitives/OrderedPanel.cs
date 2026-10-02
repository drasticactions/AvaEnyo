using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;

namespace AvaEnyo.Controls.Primitives;

/// <summary>
/// A <see cref="StackPanel"/> that marks each visible child <c>:first</c>, <c>:middle</c>, <c>:last</c> or
/// <c>:single</c> (Enyo's <c>OrderedContainer</c>). Hidden children are skipped, and the marks update when a child
/// is shown or hidden. Group, RowGroup, RadioGroup, the tool button groups, TabGroup and Menu use it.
/// </summary>
public class OrderedPanel : StackPanel
{
    private readonly List<Control> _visible = new();

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        UpdateOrder();
        return base.MeasureOverride(availableSize);
    }

    /// <summary>Recomputes the positions of the children.</summary>
    public void UpdateOrder()
    {
        _visible.Clear();
        foreach (var child in Children)
        {
            if (child.IsVisible)
            {
                _visible.Add(child);
            }
            else
            {
                Order.SetPosition(child, OrderPosition.None);
            }
        }

        for (var i = 0; i < _visible.Count; i++)
        {
            var position = _visible.Count == 1 ? OrderPosition.Single
                : i == 0 ? OrderPosition.First
                : i == _visible.Count - 1 ? OrderPosition.Last
                : OrderPosition.Middle;
            Order.SetPosition(_visible[i], position);
        }
    }
}
