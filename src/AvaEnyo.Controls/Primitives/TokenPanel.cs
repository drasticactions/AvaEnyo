using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;

namespace AvaEnyo.Controls.Primitives;

/// <summary>
/// The wrapping row of a <see cref="TokenInput"/>. Children wrap left to right; <see cref="FillChild"/> (the text box)
/// takes the rest of its row. With <see cref="IsCollapsed"/> only the first row shows, and <see cref="Overflow"/>
/// (the "+N" button) is placed at its end when children are hidden; <see cref="HiddenCount"/> says how many.
/// </summary>
public class TokenPanel : Panel
{
    /// <summary>Defines the <see cref="IsCollapsed"/> property.</summary>
    public static readonly StyledProperty<bool> IsCollapsedProperty =
        AvaloniaProperty.Register<TokenPanel, bool>(nameof(IsCollapsed));

    /// <summary>Defines the <see cref="FillChild"/> property.</summary>
    public static readonly StyledProperty<Control?> FillChildProperty =
        AvaloniaProperty.Register<TokenPanel, Control?>(nameof(FillChild));

    /// <summary>Defines the <see cref="Overflow"/> property.</summary>
    public static readonly StyledProperty<Control?> OverflowProperty =
        AvaloniaProperty.Register<TokenPanel, Control?>(nameof(Overflow));

    /// <summary>Defines the <see cref="HiddenCount"/> property.</summary>
    public static readonly DirectProperty<TokenPanel, int> HiddenCountProperty =
        AvaloniaProperty.RegisterDirect<TokenPanel, int>(nameof(HiddenCount), o => o.HiddenCount);

    private int _hiddenCount;

    static TokenPanel()
    {
        AffectsMeasure<TokenPanel>(IsCollapsedProperty, FillChildProperty, OverflowProperty);
    }

    /// <summary>True to show only the first row.</summary>
    public bool IsCollapsed
    {
        get => GetValue(IsCollapsedProperty);
        set => SetValue(IsCollapsedProperty, value);
    }

    /// <summary>The child that stretches to the end of its row.</summary>
    public Control? FillChild
    {
        get => GetValue(FillChildProperty);
        set => SetValue(FillChildProperty, value);
    }

    /// <summary>The child shown at the end of the first row when collapsing hides children.</summary>
    public Control? Overflow
    {
        get => GetValue(OverflowProperty);
        set => SetValue(OverflowProperty, value);
    }

    /// <summary>The number of children, not counting the fill child, hidden by collapsing.</summary>
    public int HiddenCount
    {
        get => _hiddenCount;
        private set => SetAndRaise(HiddenCountProperty, ref _hiddenCount, value);
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        var child = new Size(availableSize.Width, double.PositiveInfinity);
        foreach (var c in Children)
        {
            c.Measure(child);
        }

        var size = Layout(availableSize.Width, arrange: false, out var hidden);
        HiddenCount = hidden;
        return size;
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        Layout(finalSize.Width, arrange: true, out var hidden);
        HiddenCount = hidden;
        return finalSize;
    }

    private Size Layout(double width, bool arrange, out int hidden)
    {
        var overflow = Overflow;
        var items = new List<Control>();
        foreach (var c in Children)
        {
            if (c != overflow && c.IsVisible)
            {
                items.Add(c);
            }
        }

        if (double.IsInfinity(width))
        {
            width = 0;
            foreach (var c in items)
            {
                width += c.DesiredSize.Width;
            }
        }

        double x = 0, y = 0, rowHeight = 0, maxRight = 0;
        hidden = 0;
        var collapsed = IsCollapsed;
        var overflowWidth = overflow?.DesiredSize.Width ?? 0;
        var full = false;
        for (var i = 0; i < items.Count; i++)
        {
            var c = items[i];
            var size = c.DesiredSize;
            if (collapsed)
            {
                // Room for the "+N" button is kept unless this is the last child.
                var reserve = i < items.Count - 1 ? overflowWidth : 0;
                if (full || (x > 0 && x + size.Width + reserve > width))
                {
                    full = true;
                    if (c != FillChild)
                    {
                        hidden++;
                    }

                    if (arrange)
                    {
                        c.Arrange(default);
                    }

                    continue;
                }
            }
            else if (x > 0 && x + size.Width > width)
            {
                x = 0;
                y += rowHeight;
                rowHeight = 0;
            }

            var w = c == FillChild ? Math.Max(size.Width, width - x) : size.Width;
            if (arrange)
            {
                c.Arrange(new Rect(x, y, w, size.Height));
            }

            x += w;
            maxRight = Math.Max(maxRight, x);
            rowHeight = Math.Max(rowHeight, size.Height);
        }

        if (overflow is not null)
        {
            if (collapsed && hidden > 0)
            {
                if (arrange)
                {
                    overflow.Arrange(new Rect(Math.Min(x, Math.Max(0, width - overflowWidth)), y, overflowWidth, overflow.DesiredSize.Height));
                }

                rowHeight = Math.Max(rowHeight, overflow.DesiredSize.Height);
                maxRight = Math.Max(maxRight, x + overflowWidth);
            }
            else if (arrange)
            {
                overflow.Arrange(default);
            }
        }

        return new Size(Math.Min(maxRight, width), y + rowHeight);
    }
}
