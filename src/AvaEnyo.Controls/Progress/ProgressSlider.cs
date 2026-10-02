using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace AvaEnyo.Controls;

/// <summary>
/// A slider whose track is also a progress bar (<c>enyo.ProgressSlider</c>), as in a media player: the blue bar shows
/// <see cref="BarValue"/> and a lighter secondary bar shows <see cref="AltBarValue"/> (a buffered amount), while the
/// handle sets <c>Value</c>. With <see cref="LockBar"/> the bar follows the handle.
/// </summary>
[TemplatePart("PART_Track", typeof(Avalonia.Controls.Primitives.Track), IsRequired = true)]
[TemplatePart("PART_Bar", typeof(Control))]
[TemplatePart("PART_AltBar", typeof(Control))]
public class ProgressSlider : Slider
{
    /// <summary>Defines the <see cref="BarValue"/> property.</summary>
    public static readonly StyledProperty<double> BarValueProperty =
        AvaloniaProperty.Register<ProgressSlider, double>(nameof(BarValue));

    /// <summary>Defines the <see cref="AltBarValue"/> property.</summary>
    public static readonly StyledProperty<double> AltBarValueProperty =
        AvaloniaProperty.Register<ProgressSlider, double>(nameof(AltBarValue));

    /// <summary>Defines the <see cref="LockBar"/> property.</summary>
    public static readonly StyledProperty<bool> LockBarProperty =
        AvaloniaProperty.Register<ProgressSlider, bool>(nameof(LockBar));

    /// <summary>Defines the <see cref="BarMinimum"/> property.</summary>
    public static readonly StyledProperty<double> BarMinimumProperty =
        AvaloniaProperty.Register<ProgressSlider, double>(nameof(BarMinimum));

    /// <summary>Defines the <see cref="BarMaximum"/> property.</summary>
    public static readonly StyledProperty<double> BarMaximumProperty =
        AvaloniaProperty.Register<ProgressSlider, double>(nameof(BarMaximum), 100);

    /// <summary>Defines the <see cref="BarRatio"/> property.</summary>
    public static readonly DirectProperty<ProgressSlider, double> BarRatioProperty =
        AvaloniaProperty.RegisterDirect<ProgressSlider, double>(nameof(BarRatio), o => o.BarRatio);

    /// <summary>Defines the <see cref="AltBarRatio"/> property.</summary>
    public static readonly DirectProperty<ProgressSlider, double> AltBarRatioProperty =
        AvaloniaProperty.RegisterDirect<ProgressSlider, double>(nameof(AltBarRatio), o => o.AltBarRatio);

    private double _barRatio;
    private double _altBarRatio;

    static ProgressSlider()
    {
        foreach (var p in new AvaloniaProperty[] { BarValueProperty, AltBarValueProperty, LockBarProperty, BarMinimumProperty, BarMaximumProperty, ValueProperty, MinimumProperty, MaximumProperty })
        {
            p.Changed.AddClassHandler<ProgressSlider>((s, _) => s.UpdateRatios());
        }
    }

    /// <summary>Initializes a new instance of the <see cref="ProgressSlider"/> class.</summary>
    public ProgressSlider()
    {
        UpdateRatios();
    }

    /// <summary>The value the blue bar shows, between <see cref="BarMinimum"/> and <see cref="BarMaximum"/>.</summary>
    public double BarValue
    {
        get => GetValue(BarValueProperty);
        set => SetValue(BarValueProperty, value);
    }

    /// <summary>The value the secondary bar shows (a buffered amount), on the bar's range.</summary>
    public double AltBarValue
    {
        get => GetValue(AltBarValueProperty);
        set => SetValue(AltBarValueProperty, value);
    }

    /// <summary>True: the bar follows the handle (<c>Value</c>) instead of <see cref="BarValue"/>.</summary>
    public bool LockBar
    {
        get => GetValue(LockBarProperty);
        set => SetValue(LockBarProperty, value);
    }

    /// <summary>The bar's minimum. Default 0.</summary>
    public double BarMinimum
    {
        get => GetValue(BarMinimumProperty);
        set => SetValue(BarMinimumProperty, value);
    }

    /// <summary>The bar's maximum. Default 100.</summary>
    public double BarMaximum
    {
        get => GetValue(BarMaximumProperty);
        set => SetValue(BarMaximumProperty, value);
    }

    /// <summary>The filled fraction of the blue bar.</summary>
    public double BarRatio
    {
        get => _barRatio;
        private set => SetAndRaise(BarRatioProperty, ref _barRatio, value);
    }

    /// <summary>The filled fraction of the secondary bar.</summary>
    public double AltBarRatio
    {
        get => _altBarRatio;
        private set => SetAndRaise(AltBarRatioProperty, ref _altBarRatio, value);
    }

    private static double Ratio(double value, double min, double max) =>
        max > min ? Math.Clamp((value - min) / (max - min), 0, 1) : 0;

    private void UpdateRatios()
    {
        BarRatio = LockBar ? Ratio(Value, Minimum, Maximum) : Ratio(BarValue, BarMinimum, BarMaximum);
        AltBarRatio = LockBar ? Ratio(AltBarValue, Minimum, Maximum) : Ratio(AltBarValue, BarMinimum, BarMaximum);
    }
}
