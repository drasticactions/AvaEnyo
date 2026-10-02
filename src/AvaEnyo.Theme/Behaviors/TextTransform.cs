using System.Globalization;
using Avalonia;
using Avalonia.Controls;

namespace AvaEnyo;

/// <summary>
/// Uppercases the text of a <see cref="TextBlock"/> for Enyo's <c>text-transform: uppercase</c> label styles. The
/// shown text is replaced with <c>SetCurrentValue</c>, so a binding on <see cref="TextBlock.Text"/> stays and the
/// bound value is not changed.
/// </summary>
public static class TextTransform
{
    /// <summary>Defines the <c>Uppercase</c> attached property.</summary>
    public static readonly AttachedProperty<bool> UppercaseProperty =
        AvaloniaProperty.RegisterAttached<TextBlock, bool>("Uppercase", typeof(TextTransform));

    static TextTransform()
    {
        UppercaseProperty.Changed.AddClassHandler<TextBlock>((t, _) => Apply(t));
        TextBlock.TextProperty.Changed.AddClassHandler<TextBlock>((t, _) => Apply(t));
    }

    /// <summary>Gets <c>Uppercase</c>.</summary>
    public static bool GetUppercase(TextBlock element) => element.GetValue(UppercaseProperty);

    /// <summary>Sets <c>Uppercase</c>.</summary>
    public static void SetUppercase(TextBlock element, bool value) => element.SetValue(UppercaseProperty, value);

    private static void Apply(TextBlock text)
    {
        if (!text.GetValue(UppercaseProperty) || text.Text is not { Length: > 0 } s)
        {
            return;
        }

        var upper = s.ToUpper(CultureInfo.CurrentCulture);
        if (upper != s)
        {
            text.SetCurrentValue(TextBlock.TextProperty, upper);
        }
    }
}
