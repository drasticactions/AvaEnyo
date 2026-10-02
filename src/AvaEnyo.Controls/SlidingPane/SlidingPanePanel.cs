using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;

namespace AvaEnyo.Controls;

/// <summary>
/// The template part of a <see cref="SlidingPane"/> that draws its views: each at its natural left edge, at its
/// natural or fitted width; the slide is a translation on the view. Later views draw over earlier ones.
/// </summary>
public class SlidingPanePanel : Control
{
    private readonly List<SlidingView> _views = new();

    internal SlidingPane? Pane { get; set; }

    internal void SetViews(IEnumerable<SlidingView> views)
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
        // Measure each view at the width it will be arranged at.
        if (!double.IsInfinity(availableSize.Width))
        {
            Pane?.ComputeNaturalLayout(availableSize);
        }

        var height = 0d;
        foreach (var v in _views)
        {
            var w = v.FitWidth ?? (double.IsNaN(v.DeclaredWidth) ? (v.NaturalWidth > 0 ? v.NaturalWidth : availableSize.Width) : v.DeclaredWidth);
            v.Measure(new Size(w, availableSize.Height));
            height = Math.Max(height, v.DesiredSize.Height);
        }

        var width = double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width;
        return new Size(width, double.IsInfinity(availableSize.Height) ? height : availableSize.Height);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        Pane?.ComputeNaturalLayout(finalSize);
        foreach (var v in _views)
        {
            if (!v.IsVisible)
            {
                continue;
            }

            var w = SlidingPane.ArrangedWidth(v);
            v.Arrange(new Rect(v.NaturalLeft, 0, Math.Max(0, w), finalSize.Height));
        }

        return finalSize;
    }
}
