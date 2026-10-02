using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace AvaEnyo.Controls.Primitives;

/// <summary>
/// The Enyo flex layout (the CSS3 flexible box of <c>enyo.FlexLayout</c>). Children without
/// <see cref="Flex.ValueProperty"/> get their natural size. A flexed child's main-axis basis is 0: it gets exactly
/// its share of the space left over, like a Grid star column, whatever its content wants.
/// </summary>
public abstract class FlexPanel : Panel
{
    /// <summary>Defines the <see cref="Pack"/> property.</summary>
    public static readonly StyledProperty<FlexPack> PackProperty =
        AvaloniaProperty.Register<FlexPanel, FlexPack>(nameof(Pack));

    /// <summary>Defines the <see cref="Align"/> property.</summary>
    public static readonly StyledProperty<FlexAlign> AlignProperty =
        AvaloniaProperty.Register<FlexPanel, FlexAlign>(nameof(Align), FlexAlign.Stretch);

    /// <summary>Defines the <see cref="Spacing"/> property.</summary>
    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<FlexPanel, double>(nameof(Spacing));

    static FlexPanel()
    {
        AffectsMeasure<FlexPanel>(PackProperty, AlignProperty, SpacingProperty);
    }

    /// <summary>Placement along the main axis. Applies when no child is flexed.</summary>
    public FlexPack Pack
    {
        get => GetValue(PackProperty);
        set => SetValue(PackProperty, value);
    }

    /// <summary>Placement across the main axis. Default <see cref="FlexAlign.Stretch"/>.</summary>
    public FlexAlign Align
    {
        get => GetValue(AlignProperty);
        set => SetValue(AlignProperty, value);
    }

    /// <summary>Space between visible children.</summary>
    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    /// <summary>The main axis.</summary>
    protected abstract Orientation Orientation { get; }

    /// <summary>
    /// True for a child placed outside the flex flow, like an absolutely positioned CSS element. It is measured at
    /// its natural size and arranged by <see cref="ArrangeOutOfFlow"/>. Default false for every child.
    /// </summary>
    protected virtual bool IsOutOfFlow(Control child) => false;

    /// <summary>Arranges a child that is outside the flex flow. Default: at the start, centered across.</summary>
    protected virtual void ArrangeOutOfFlow(Control child, Size finalSize)
    {
        var d = child.DesiredSize;
        child.Arrange(Horizontal
            ? new Rect(0, (finalSize.Height - d.Height) / 2, d.Width, d.Height)
            : new Rect((finalSize.Width - d.Width) / 2, 0, d.Width, d.Height));
    }

    /// <summary>
    /// Main-axis room kept free at the start of the flow, such as for an out-of-flow child pinned there. Flow items
    /// never start before it. Default 0.
    /// </summary>
    protected virtual double FlowStartInset => 0;

    private bool Horizontal => Orientation == Orientation.Horizontal;

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        var mainAvailable = Main(availableSize);
        var crossAvailable = Cross(availableSize);
        double fixedMain = 0, cross = 0, totalFlex = 0;
        var visible = 0;

        foreach (var child in Children)
        {
            if (!child.IsVisible)
            {
                continue;
            }

            if (IsOutOfFlow(child))
            {
                child.Measure(availableSize);
                cross = Math.Max(cross, Cross(child.DesiredSize));
                continue;
            }

            visible++;
            var flex = Flex.GetValue(child);
            if (flex > 0)
            {
                totalFlex += flex;
                continue;
            }

            child.Measure(Make(double.PositiveInfinity, crossAvailable));
            fixedMain += Main(child.DesiredSize);
            cross = Math.Max(cross, Cross(child.DesiredSize));
        }

        var spacing = Math.Max(0, visible - 1) * Spacing;
        var inset = FlowStartInset;
        mainAvailable = Math.Max(0, mainAvailable - inset);
        double flexMain = 0;
        if (totalFlex > 0)
        {
            var leftover = double.IsInfinity(mainAvailable) ? double.NaN : Math.Max(0, mainAvailable - fixedMain - spacing);
            foreach (var child in Children)
            {
                var flex = Flex.GetValue(child);
                if (!child.IsVisible || flex <= 0 || IsOutOfFlow(child))
                {
                    continue;
                }

                if (double.IsNaN(leftover))
                {
                    // Unbounded main axis: nothing is left over to share, so the child takes its natural size.
                    child.Measure(Make(double.PositiveInfinity, crossAvailable));
                    flexMain += Main(child.DesiredSize);
                }
                else
                {
                    var share = leftover * flex / totalFlex;
                    child.Measure(Make(share, crossAvailable));
                    flexMain += share;
                }

                cross = Math.Max(cross, Cross(child.DesiredSize));
            }
        }

        return Make(inset + fixedMain + flexMain + spacing, cross);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var mainFinal = Main(finalSize);
        var crossFinal = Cross(finalSize);
        double fixedMain = 0, totalFlex = 0;
        var visible = 0;
        foreach (var child in Children)
        {
            if (!child.IsVisible || IsOutOfFlow(child))
            {
                continue;
            }

            visible++;
            var flex = Flex.GetValue(child);
            if (flex > 0)
            {
                totalFlex += flex;
            }
            else
            {
                fixedMain += Main(child.DesiredSize);
            }
        }

        var spacing = Spacing;
        var inset = FlowStartInset;
        var leftover = Math.Max(0, mainFinal - inset - fixedMain - (Math.Max(0, visible - 1) * spacing));
        double position = inset;
        var gap = spacing;
        if (totalFlex <= 0 && visible > 0)
        {
            switch (Pack)
            {
                case FlexPack.Center:
                    // Centered on the whole panel, unless that would reach into the inset.
                    position = Math.Max(inset, (inset + leftover) / 2);
                    break;
                case FlexPack.End:
                    position = inset + leftover;
                    break;
                case FlexPack.Justify when visible > 1:
                    gap = spacing + (leftover / (visible - 1));
                    break;
            }
        }

        var baseline = Align == FlexAlign.Baseline && Horizontal ? MaxBaseline() : 0;
        foreach (var child in Children)
        {
            if (!child.IsVisible)
            {
                continue;
            }

            if (IsOutOfFlow(child))
            {
                ArrangeOutOfFlow(child, finalSize);
                continue;
            }

            var flex = Flex.GetValue(child);
            var main = flex > 0 ? leftover * flex / totalFlex : Main(child.DesiredSize);
            var childCross = Cross(child.DesiredSize);
            double crossPos;
            double crossSize;
            switch (Align)
            {
                case FlexAlign.Start:
                    crossPos = 0;
                    crossSize = childCross;
                    break;
                case FlexAlign.Center:
                    crossPos = (crossFinal - childCross) / 2;
                    crossSize = childCross;
                    break;
                case FlexAlign.End:
                    crossPos = crossFinal - childCross;
                    crossSize = childCross;
                    break;
                case FlexAlign.Baseline:
                    crossPos = Horizontal ? baseline - Baseline(child) : 0;
                    crossSize = childCross;
                    break;
                default:
                    crossPos = 0;
                    crossSize = crossFinal;
                    break;
            }

            var rect = Horizontal
                ? new Rect(position, crossPos, main, crossSize)
                : new Rect(crossPos, position, crossSize, main);
            child.Arrange(rect);
            position += main + gap;
        }

        return finalSize;
    }

    private double MaxBaseline()
    {
        double max = 0;
        foreach (var child in Children)
        {
            if (child.IsVisible)
            {
                max = Math.Max(max, Baseline(child));
            }
        }

        return max;
    }

    private static double Baseline(Control child) => child is TextBlock t
        ? t.Margin.Top + t.Padding.Top + (t.TextLayout?.TextLines.Count > 0 ? t.TextLayout.TextLines[0].Baseline : 0)
        : 0;

    private double Main(Size s) => Horizontal ? s.Width : s.Height;

    private double Cross(Size s) => Horizontal ? s.Height : s.Width;

    private Size Make(double main, double cross) => Horizontal ? new Size(main, cross) : new Size(cross, main);
}
