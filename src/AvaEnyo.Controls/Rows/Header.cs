using Avalonia;
using Avalonia.Controls;

namespace AvaEnyo.Controls;

/// <summary>
/// The Enyo header band (<c>enyo.Header</c>): light grey #ccc with a 2 px light top edge and a darker bottom edge,
/// at least 50 px tall, content padded 13/12 px. The <c>dark</c> class gives the toolbar look with white text.
/// </summary>
public class Header : ContentControl
{
    /// <summary>Defines the <see cref="Icon"/> property.</summary>
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<Header, object?>(nameof(Icon));

    /// <summary>An icon before the content, 10 px from it.</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
}
