using System;
using Avalonia;
using Avalonia.Controls;

namespace AvaEnyo.Controls.Primitives;

/// <summary>
/// A <see cref="Border"/> that takes <see cref="Ratio"/> of the width it is offered: the filled part of a progress
/// slider or progress button. Align it to the left of its track.
/// </summary>
public class ProgressFill : Border
{
    /// <summary>Defines the <see cref="Ratio"/> property.</summary>
    public static readonly StyledProperty<double> RatioProperty =
        AvaloniaProperty.Register<ProgressFill, double>(nameof(Ratio));

    static ProgressFill()
    {
        AffectsMeasure<ProgressFill>(RatioProperty);
    }

    /// <summary>The filled fraction, 0 to 1.</summary>
    public double Ratio
    {
        get => GetValue(RatioProperty);
        set => SetValue(RatioProperty, value);
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        var size = base.MeasureOverride(availableSize);
        var ratio = double.IsNaN(Ratio) ? 0 : Math.Clamp(Ratio, 0, 1);
        var width = double.IsInfinity(availableSize.Width) ? size.Width : availableSize.Width * ratio;
        return new Size(width, size.Height);
    }
}
