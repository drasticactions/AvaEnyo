using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace AvaEnyo.Controls.Primitives;

/// <summary>
/// The view holders of a <see cref="Carousel"/>: every visible child is as large as the strip and they sit side by side
/// on the main axis, shifted by <see cref="Offset"/>. Hidden holders take no place.
/// </summary>
public class CarouselStrip : Panel
{
    /// <summary>Defines the <see cref="Offset"/> property.</summary>
    public static readonly StyledProperty<double> OffsetProperty =
        AvaloniaProperty.Register<CarouselStrip, double>(nameof(Offset));

    /// <summary>Defines the <see cref="Orientation"/> property.</summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<CarouselStrip, Orientation>(nameof(Orientation));

    static CarouselStrip()
    {
        ClipToBoundsProperty.OverrideDefaultValue<CarouselStrip>(true);
        AffectsArrange<CarouselStrip>(OffsetProperty, OrientationProperty);
    }

    /// <summary>How far the holders are shifted toward the start, in pixels.</summary>
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
        var size = default(Size);
        foreach (var child in Children)
        {
            child.Measure(availableSize);
            size = new Size(Math.Max(size.Width, child.DesiredSize.Width), Math.Max(size.Height, child.DesiredSize.Height));
        }

        return new Size(
            double.IsInfinity(availableSize.Width) ? size.Width : availableSize.Width,
            double.IsInfinity(availableSize.Height) ? size.Height : availableSize.Height);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var k = 0;
        var horizontal = Orientation == Orientation.Horizontal;
        foreach (var child in Children)
        {
            if (!child.IsVisible)
            {
                continue;
            }

            var rect = horizontal
                ? new Rect((k * finalSize.Width) - Offset, 0, finalSize.Width, finalSize.Height)
                : new Rect(0, (k * finalSize.Height) - Offset, finalSize.Width, finalSize.Height);
            child.Arrange(rect);
            k++;
        }

        return finalSize;
    }
}
