using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace AvaEnyo.Controls;

/// <summary>
/// A button with a 32×32 icon left of an optional caption (<c>enyo.IconButton</c>). The icon may be an image, a
/// geometry or any control. <c>:pressed</c> shows the pressed icon when <see cref="PressedIcon"/> is set.
/// </summary>
[PseudoClasses(":has-pressed-icon")]
public class IconButton : Button
{
    /// <summary>Defines the <see cref="Icon"/> property.</summary>
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<IconButton, object?>(nameof(Icon));

    /// <summary>Defines the <see cref="PressedIcon"/> property.</summary>
    public static readonly StyledProperty<object?> PressedIconProperty =
        AvaloniaProperty.Register<IconButton, object?>(nameof(PressedIcon));

    static IconButton()
    {
        PressedIconProperty.Changed.AddClassHandler<IconButton>((b, e) => b.PseudoClasses.Set(":has-pressed-icon", e.NewValue is not null));
    }

    /// <summary>The icon: an <c>IImage</c>, a <c>Geometry</c> or a control.</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>An optional icon shown while pressed (Enyo's second sprite row).</summary>
    public object? PressedIcon
    {
        get => GetValue(PressedIconProperty);
        set => SetValue(PressedIconProperty, value);
    }
}
