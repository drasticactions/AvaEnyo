using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace AvaEnyo.Controls;

/// <summary>
/// A key of a <see cref="NumericKeypad"/> (the telephony <c>DialpadButton</c>): a large digit with a small line of
/// letters under it, or an icon such as backspace.
/// </summary>
[PseudoClasses(":has-icon")]
public class NumericKeypadButton : Button
{
    /// <summary>Defines the <see cref="Key"/> property.</summary>
    public static readonly StyledProperty<string> KeyProperty =
        AvaloniaProperty.Register<NumericKeypadButton, string>(nameof(Key), string.Empty);

    /// <summary>Defines the <see cref="Letters"/> property.</summary>
    public static readonly StyledProperty<string?> LettersProperty =
        AvaloniaProperty.Register<NumericKeypadButton, string?>(nameof(Letters));

    /// <summary>Defines the <see cref="ShowLetters"/> property.</summary>
    public static readonly StyledProperty<bool> ShowLettersProperty =
        AvaloniaProperty.Register<NumericKeypadButton, bool>(nameof(ShowLetters), true);

    /// <summary>Defines the <see cref="Icon"/> property.</summary>
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<NumericKeypadButton, object?>(nameof(Icon));

    static NumericKeypadButton()
    {
        IconProperty.Changed.AddClassHandler<NumericKeypadButton>((b, e) => b.PseudoClasses.Set(":has-icon", e.NewValue is not null));
    }

    /// <summary>The key this button presses.</summary>
    public string Key
    {
        get => GetValue(KeyProperty);
        set => SetValue(KeyProperty, value);
    }

    /// <summary>The letters under the digit, such as <c>ABC</c>.</summary>
    public string? Letters
    {
        get => GetValue(LettersProperty);
        set => SetValue(LettersProperty, value);
    }

    /// <summary>True to show the letters line; it keeps its height when hidden by empty letters.</summary>
    public bool ShowLetters
    {
        get => GetValue(ShowLettersProperty);
        set => SetValue(ShowLettersProperty, value);
    }

    /// <summary>An icon drawn instead of the digit (a geometry, image or control).</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    internal bool HoldFired { get; set; }
}
