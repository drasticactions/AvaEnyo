using Avalonia;
using Avalonia.Controls;

namespace AvaEnyo.Controls.Primitives;

/// <summary>
/// The <c>Position</c> attached property that an <see cref="OrderedPanel"/> sets on each child, and the matching
/// <c>:first</c>, <c>:middle</c>, <c>:last</c> and <c>:single</c> pseudo-classes (Enyo's <c>enyo-first</c> classes).
/// </summary>
public static class Order
{
    /// <summary>Defines the <c>Position</c> attached property.</summary>
    public static readonly AttachedProperty<OrderPosition> PositionProperty =
        AvaloniaProperty.RegisterAttached<Control, OrderPosition>("Position", typeof(Order));

    static Order()
    {
        PositionProperty.Changed.AddClassHandler<Control>((c, e) => Apply(c, e.GetNewValue<OrderPosition>()));
    }

    /// <summary>Gets the position.</summary>
    public static OrderPosition GetPosition(Control element) => element.GetValue(PositionProperty);

    /// <summary>Sets the position and the matching pseudo-class.</summary>
    public static void SetPosition(Control element, OrderPosition value) => element.SetValue(PositionProperty, value);

    private static void Apply(Control control, OrderPosition position)
    {
        var pc = (IPseudoClasses)control.Classes;
        pc.Set(":first", position == OrderPosition.First);
        pc.Set(":middle", position == OrderPosition.Middle);
        pc.Set(":last", position == OrderPosition.Last);
        pc.Set(":single", position == OrderPosition.Single);
        (control as IOrderedChild)?.SetOrderPosition(position);
    }
}
