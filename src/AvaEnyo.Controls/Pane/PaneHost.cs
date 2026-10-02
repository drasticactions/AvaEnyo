using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;

namespace AvaEnyo.Controls;

/// <summary>
/// The template part of a <see cref="Pane"/> that draws its views: every view is a visual child sized to the host,
/// and only the views the pane shows are visible. The views stay logical children of the pane.
/// </summary>
public class PaneHost : Control
{
    private readonly List<Control> _views = new();

    /// <summary>Puts <paramref name="views"/> in the host, in order.</summary>
    internal void SetViews(IEnumerable<Control> views)
    {
        foreach (var v in _views)
        {
            VisualChildren.Remove(v);
        }

        _views.Clear();
        _views.AddRange(views);
        foreach (var v in _views)
        {
            VisualChildren.Add(v);
        }

        InvalidateMeasure();
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        var size = default(Size);
        foreach (var v in _views)
        {
            v.Measure(availableSize);
            if (v.IsVisible)
            {
                size = new Size(System.Math.Max(size.Width, v.DesiredSize.Width), System.Math.Max(size.Height, v.DesiredSize.Height));
            }
        }

        return size;
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (var v in _views)
        {
            v.Arrange(new Rect(finalSize));
        }

        return finalSize;
    }
}
