using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;

namespace AvaEnyo.Controls.Primitives;

/// <summary>
/// The small arrow fades of an Enyo menu (<c>enyo.ScrollFades</c> with <c>menu-fade-arrow-up/down.png</c>): an
/// arrow at each edge of <see cref="Target"/> that the content can scroll toward. Place it over the scroller; it
/// does not take input.
/// </summary>
[TemplatePart("PART_FadeTop", typeof(Control))]
[TemplatePart("PART_FadeBottom", typeof(Control))]
[TemplatePart("PART_FadeLeft", typeof(Control))]
[TemplatePart("PART_FadeRight", typeof(Control))]
[PseudoClasses(":scroll-up", ":scroll-down", ":scroll-left", ":scroll-right")]
public class ScrollFades : TemplatedControl
{
    /// <summary>Defines the <see cref="Target"/> property.</summary>
    public static readonly StyledProperty<ScrollViewer?> TargetProperty =
        AvaloniaProperty.Register<ScrollFades, ScrollViewer?>(nameof(Target));

    private ScrollViewer? _subscribed;

    static ScrollFades()
    {
        IsHitTestVisibleProperty.OverrideDefaultValue<ScrollFades>(false);
        TargetProperty.Changed.AddClassHandler<ScrollFades>((f, _) => f.Resubscribe());
    }

    /// <summary>The scroller whose scroll state the fades show.</summary>
    public ScrollViewer? Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    /// <summary>Updates the fades from the target's offset, extent and viewport (Enyo <c>showHideFades</c>).</summary>
    public void Update()
    {
        var t = Target;
        if (t is null)
        {
            PseudoClasses.Set(":scroll-up", false);
            PseudoClasses.Set(":scroll-down", false);
            PseudoClasses.Set(":scroll-left", false);
            PseudoClasses.Set(":scroll-right", false);
            return;
        }

        const double e = 0.5;
        var maxX = System.Math.Max(0, t.Extent.Width - t.Viewport.Width);
        var maxY = System.Math.Max(0, t.Extent.Height - t.Viewport.Height);
        PseudoClasses.Set(":scroll-up", t.Offset.Y > e);
        PseudoClasses.Set(":scroll-down", t.Offset.Y < maxY - e);
        PseudoClasses.Set(":scroll-left", t.Offset.X > e);
        PseudoClasses.Set(":scroll-right", t.Offset.X < maxX - e);
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Resubscribe();
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Unsubscribe();
        base.OnDetachedFromVisualTree(e);
    }

    private void Resubscribe()
    {
        Unsubscribe();
        if (Target is { } t)
        {
            t.ScrollChanged += OnScrollChanged;
            _subscribed = t;
        }

        Update();
    }

    private void Unsubscribe()
    {
        if (_subscribed is not null)
        {
            _subscribed.ScrollChanged -= OnScrollChanged;
            _subscribed = null;
        }
    }

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e) => Update();
}
