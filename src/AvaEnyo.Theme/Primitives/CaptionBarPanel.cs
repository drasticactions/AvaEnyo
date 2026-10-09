using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace AvaEnyo.Primitives;

/// <summary>
/// Lays out a title bar by each child's <see cref="Layoutable.HorizontalAlignment"/>: <c>Left</c> children at the
/// left edge, <c>Right</c> children at the right edge, and the others centered on the whole width. A centered child
/// gets the width between the wider side and its mirror, so it never overlaps either side.
/// </summary>
public class CaptionBarPanel : Panel
{
    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        double left = 0, right = 0, height = 0;
        foreach (var child in Children)
        {
            if (child.HorizontalAlignment is HorizontalAlignment.Left or HorizontalAlignment.Right)
            {
                child.Measure(availableSize);
                if (child.HorizontalAlignment == HorizontalAlignment.Left)
                {
                    left = Math.Max(left, child.DesiredSize.Width);
                }
                else
                {
                    right = Math.Max(right, child.DesiredSize.Width);
                }

                height = Math.Max(height, child.DesiredSize.Height);
            }
        }

        var centerWidth = CenterWidth(availableSize.Width, left, right);
        double center = 0;
        foreach (var child in Children)
        {
            if (child.HorizontalAlignment is not (HorizontalAlignment.Left or HorizontalAlignment.Right))
            {
                child.Measure(availableSize.WithWidth(centerWidth));
                center = Math.Max(center, child.DesiredSize.Width);
                height = Math.Max(height, child.DesiredSize.Height);
            }
        }

        return new Size((2 * Math.Max(left, right)) + center, height);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        double left = 0, right = 0;
        foreach (var child in Children)
        {
            if (child.HorizontalAlignment == HorizontalAlignment.Left)
            {
                left = Math.Max(left, child.DesiredSize.Width);
            }
            else if (child.HorizontalAlignment == HorizontalAlignment.Right)
            {
                right = Math.Max(right, child.DesiredSize.Width);
            }
        }

        var centerWidth = CenterWidth(finalSize.Width, left, right);
        foreach (var child in Children)
        {
            var w = child.DesiredSize.Width;
            var x = child.HorizontalAlignment switch
            {
                HorizontalAlignment.Left => 0,
                HorizontalAlignment.Right => finalSize.Width - w,
                _ => (finalSize.Width - (w = Math.Min(w, centerWidth))) / 2,
            };
            child.Arrange(new Rect(x, 0, w, finalSize.Height));
        }

        return finalSize;
    }

    private static double CenterWidth(double width, double left, double right) =>
        double.IsInfinity(width) ? width : Math.Max(0, width - (2 * Math.Max(left, right)));
}
