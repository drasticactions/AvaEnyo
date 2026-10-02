using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace AvaEnyo.Controls;

/// <summary>
/// A header that stays pinned at the top of the nearest <see cref="ScrollViewer"/> while its section scrolls
/// (<c>enyo.FloatingHeader</c>). It moves down with the scroll offset until the end of its parent pushes it out.
/// </summary>
public class FloatingHeader : ContentControl
{
    private ScrollViewer? _viewer;

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _viewer = this.FindAncestorOfType<ScrollViewer>();
        if (_viewer is not null)
        {
            _viewer.ScrollChanged += OnScrollChanged;
        }
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_viewer is not null)
        {
            _viewer.ScrollChanged -= OnScrollChanged;
        }

        _viewer = null;
        base.OnDetachedFromVisualTree(e);
    }

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e) => Update();

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = base.ArrangeOverride(finalSize);
        Update();
        return size;
    }

    private void Update()
    {
        if (_viewer is null || this.GetVisualParent() is not Visual parent)
        {
            return;
        }

        var top = this.TranslatePoint(default, _viewer);
        if (top is null)
        {
            return;
        }

        var current = RenderTransform is Avalonia.Media.TranslateTransform t ? t.Y : 0;
        var natural = top.Value.Y - current;
        var shift = 0d;
        if (natural < 0)
        {
            var parentBottom = parent.TranslatePoint(new Point(0, parent.Bounds.Height), _viewer)?.Y ?? 0;
            shift = System.Math.Min(-natural, System.Math.Max(0, parentBottom - natural - Bounds.Height));
        }

        if (shift == 0)
        {
            ClearValue(RenderTransformProperty);
            ClearValue(ZIndexProperty);
        }
        else
        {
            RenderTransform = new Avalonia.Media.TranslateTransform(0, shift);
            ZIndex = 1;
        }
    }
}
