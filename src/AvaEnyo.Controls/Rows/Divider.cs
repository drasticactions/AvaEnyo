using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;

namespace AvaEnyo.Controls;

/// <summary>
/// The Enyo section divider: a bold blue caption followed by a 4 px blue rule that fills the rest of the row
/// (<c>enyo.Divider</c>). The caption truncates before the rule shrinks. An optional icon sits before the caption.
/// </summary>
[TemplatePart("PART_Caption", typeof(TextBlock))]
[TemplatePart("PART_Rule", typeof(Border))]
public class Divider : TemplatedControl
{
    /// <summary>Defines the <see cref="Caption"/> property.</summary>
    public static readonly StyledProperty<string?> CaptionProperty =
        AvaloniaProperty.Register<Divider, string?>(nameof(Caption));

    /// <summary>Defines the <see cref="Icon"/> property.</summary>
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<Divider, object?>(nameof(Icon));

    /// <summary>Defines the <see cref="Content"/> property.</summary>
    public static readonly StyledProperty<object?> ContentProperty =
        ContentControl.ContentProperty.AddOwner<Divider>();

    /// <summary>The caption text.</summary>
    public string? Caption
    {
        get => GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }

    /// <summary>An icon (an image, geometry or control) before the caption.</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Content placed after the rule, at the right end (Enyo's divider client).</summary>
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }
}
