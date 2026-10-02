using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace AvaEnyo.Controls.Primitives;

/// <summary>
/// The viewport of a <see cref="SnapScroller"/>: it measures its child without a limit on the main axis, stretches it
/// on the cross axis, clips it, and shifts it by <see cref="Offset"/>.
/// </summary>
public class SnapStrip : Decorator
{
    /// <summary>Defines the <see cref="Offset"/> property.</summary>
    public static readonly StyledProperty<double> OffsetProperty =
        AvaloniaProperty.Register<SnapStrip, double>(nameof(Offset));

    /// <summary>Defines the <see cref="Orientation"/> property.</summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<SnapStrip, Orientation>(nameof(Orientation));

    static SnapStrip()
    {
        ClipToBoundsProperty.OverrideDefaultValue<SnapStrip>(true);
        AffectsArrange<SnapStrip>(OffsetProperty);
        AffectsMeasure<SnapStrip>(OrientationProperty);
    }

    /// <summary>How far the child is shifted toward the start, in pixels.</summary>
    public double Offset
    {
        get => GetValue(OffsetProperty);
        set => SetValue(OffsetProperty, value);
    }

    /// <summary>The main axis.</summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        if (Child is null)
        {
            return default;
        }

        var horizontal = Orientation == Orientation.Horizontal;
        Child.Measure(horizontal ? new Size(double.PositiveInfinity, availableSize.Height) : new Size(availableSize.Width, double.PositiveInfinity));
        var d = Child.DesiredSize;
        return new Size(
            horizontal ? Math.Min(d.Width, availableSize.Width) : d.Width,
            horizontal ? d.Height : Math.Min(d.Height, availableSize.Height));
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Child is { } child)
        {
            var d = child.DesiredSize;
            var rect = Orientation == Orientation.Horizontal
                ? new Rect(-Offset, 0, Math.Max(d.Width, finalSize.Width), finalSize.Height)
                : new Rect(0, -Offset, finalSize.Width, Math.Max(d.Height, finalSize.Height));
            child.Arrange(rect);
        }

        return finalSize;
    }
}
