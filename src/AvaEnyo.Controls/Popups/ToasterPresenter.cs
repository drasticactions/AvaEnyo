using System;
using Avalonia;
using AvaEnyo.Animations;

namespace AvaEnyo.Controls;

/// <summary>
/// The presenter of a <see cref="Toaster"/>. Against the top or bottom edge it takes the full width; against the
/// left or right edge, the full height.
/// </summary>
public class ToasterPresenter : PopupPresenter
{
    /// <summary>Defines the <see cref="Edge"/> property.</summary>
    public static readonly StyledProperty<ToasterEdge> EdgeProperty =
        AvaloniaProperty.Register<ToasterPresenter, ToasterEdge>(nameof(Edge));

    static ToasterPresenter()
    {
        AffectsMeasure<ToasterPresenter>(EdgeProperty);
    }

    /// <summary>The edge the toaster flies in from, mirrored from the toaster.</summary>
    public ToasterEdge Edge
    {
        get => GetValue(EdgeProperty);
        set => SetValue(EdgeProperty, value);
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        var size = base.MeasureOverride(availableSize);
        var vertical = Edge is ToasterEdge.Bottom or ToasterEdge.Top;
        if (vertical && !double.IsInfinity(availableSize.Width))
        {
            return new Size(Math.Max(size.Width, availableSize.Width), size.Height);
        }

        if (!vertical && !double.IsInfinity(availableSize.Height))
        {
            return new Size(size.Width, Math.Max(size.Height, availableSize.Height));
        }

        return size;
    }
}
