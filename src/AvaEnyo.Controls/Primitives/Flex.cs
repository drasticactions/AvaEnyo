using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace AvaEnyo.Controls.Primitives;

/// <summary>
/// The Enyo <c>flex</c> attached property. A child of an <see cref="HFlexBox"/> or <see cref="VFlexBox"/> with a
/// value above 0 shares the space left over by the other children, in proportion to its value.
/// </summary>
public static class Flex
{
    /// <summary>Defines the <c>Value</c> attached property.</summary>
    public static readonly AttachedProperty<double> ValueProperty =
        AvaloniaProperty.RegisterAttached<Control, double>("Value", typeof(Flex), 0d, validate: v => v >= 0 && !double.IsNaN(v));

    static Flex()
    {
        ValueProperty.Changed.AddClassHandler<Control>((c, _) => (c.Parent as Layoutable)?.InvalidateMeasure());
    }

    /// <summary>Gets the flex value.</summary>
    public static double GetValue(Control element) => element.GetValue(ValueProperty);

    /// <summary>Sets the flex value. 0 (default) keeps the child at its natural size.</summary>
    public static void SetValue(Control element, double value) => element.SetValue(ValueProperty, value);
}
