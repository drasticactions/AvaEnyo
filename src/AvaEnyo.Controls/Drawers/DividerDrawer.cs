using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Media.Transformation;

namespace AvaEnyo.Controls;

/// <summary>
/// A <see cref="Drawer"/> whose header is a <see cref="Divider"/> with the round collapse knob of
/// <c>divider-arrow.png</c> at its end (<c>enyo.DividerDrawer</c>). The knob's triangle turns down when open.
/// </summary>
[TemplatePart("PART_Arrow", typeof(Control))]
public class DividerDrawer : Drawer
{
    private Control? _arrow;

    static DividerDrawer()
    {
        IsOpenProperty.Changed.AddClassHandler<DividerDrawer>((d, _) => d.UpdateArrow());
    }

    /// <summary>Defines the <see cref="Caption"/> property.</summary>
    public static readonly StyledProperty<string?> CaptionProperty =
        AvaloniaProperty.Register<DividerDrawer, string?>(nameof(Caption));

    /// <summary>Defines the <see cref="Icon"/> property.</summary>
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<DividerDrawer, object?>(nameof(Icon));

    /// <summary>The caption of the divider header.</summary>
    public string? Caption
    {
        get => GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }

    /// <summary>An icon before the caption.</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // The knob lives in the divider's content, which style selectors on this template do not reach.
        _arrow = e.NameScope.Find<Control>("PART_Arrow");
        UpdateArrow();
    }

    private void UpdateArrow()
    {
        if (_arrow is not null)
        {
            _arrow.RenderTransform = TransformOperations.Parse(IsOpen ? "rotate(90deg)" : "rotate(0deg)");
        }
    }
}
