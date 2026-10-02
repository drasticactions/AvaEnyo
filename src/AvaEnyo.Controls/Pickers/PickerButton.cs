using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace AvaEnyo.Controls;

/// <summary>
/// The Onyx picker pill (<c>enyo.PickerButton</c>, <c>picker-pill.png</c>): a rounded translucent pill that darkens
/// while pressed and turns blue (<c>:focus-open</c>) while its list is open.
/// </summary>
[PseudoClasses(":focus-open")]
public class PickerButton : Button
{
    /// <summary>Defines the <see cref="IsFocusOpen"/> property.</summary>
    public static readonly StyledProperty<bool> IsFocusOpenProperty =
        AvaloniaProperty.Register<PickerButton, bool>(nameof(IsFocusOpen));

    static PickerButton()
    {
        IsFocusOpenProperty.Changed.AddClassHandler<PickerButton>((b, e) => b.PseudoClasses.Set(":focus-open", e.GetNewValue<bool>()));
    }

    /// <summary>True while the list of the pill (or of its picker group) is open (Enyo <c>focus</c>).</summary>
    public bool IsFocusOpen
    {
        get => GetValue(IsFocusOpenProperty);
        set => SetValue(IsFocusOpenProperty, value);
    }
}
