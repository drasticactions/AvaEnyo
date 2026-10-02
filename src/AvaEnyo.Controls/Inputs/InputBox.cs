using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace AvaEnyo.Controls;

/// <summary>
/// An input-looking frame around any content (<c>enyo.InputBox</c>), for a text box with accessories such as an icon
/// or a button. It shows the focus frame while any descendant has focus, or always with
/// <see cref="AlwaysLooksFocused"/>. Classes <c>rounded</c> and <c>tool</c> match the text box variants.
/// </summary>
[PseudoClasses(":looks-focused")]
public class InputBox : ContentControl
{
    /// <summary>Defines the <see cref="AlwaysLooksFocused"/> property.</summary>
    public static readonly StyledProperty<bool> AlwaysLooksFocusedProperty =
        AvaloniaProperty.Register<InputBox, bool>(nameof(AlwaysLooksFocused));

    static InputBox()
    {
        AlwaysLooksFocusedProperty.Changed.AddClassHandler<InputBox>((b, _) => b.UpdateLook());
        IsKeyboardFocusWithinProperty.Changed.AddClassHandler<InputBox>((b, _) => b.UpdateLook());
    }

    /// <summary>True to show the focus frame even without focus.</summary>
    public bool AlwaysLooksFocused
    {
        get => GetValue(AlwaysLooksFocusedProperty);
        set => SetValue(AlwaysLooksFocusedProperty, value);
    }

    private void UpdateLook() => PseudoClasses.Set(":looks-focused", AlwaysLooksFocused || IsKeyboardFocusWithin);
}
