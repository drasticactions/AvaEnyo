using Avalonia;
using Avalonia.Controls;

namespace AvaEnyo.Controls;

/// <summary>
/// The <c>IsHandle</c> attached property (Enyo <c>slidingHandler</c>). True marks a control, such as a
/// <see cref="GrabButton"/>, as a drag handle of its <see cref="SlidingView"/>: a drag on it moves the view even when
/// <see cref="SlidingView.DragAnywhere"/> is off, and a tap on it toggles the view. False stops dragging from that
/// subtree.
/// </summary>
public static class Sliding
{
    /// <summary>Defines the <c>IsHandle</c> attached property.</summary>
    public static readonly AttachedProperty<bool?> IsHandleProperty =
        AvaloniaProperty.RegisterAttached<Control, bool?>("IsHandle", typeof(Sliding));

    /// <summary>Gets <c>IsHandle</c>.</summary>
    public static bool? GetIsHandle(Control element) => element.GetValue(IsHandleProperty);

    /// <summary>Sets <c>IsHandle</c>.</summary>
    public static void SetIsHandle(Control element, bool? value) => element.SetValue(IsHandleProperty, value);
}
