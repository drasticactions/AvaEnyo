using Avalonia;
using Avalonia.Controls;

namespace AvaEnyo.Controls;

/// <summary>
/// The Onyx popup (<c>enyo.Popup</c>): content in the 24 px <c>popup.png</c> frame, at least 300 px wide. In XAML
/// use <c>enyo:Popup</c>; in C# the name also exists in Avalonia, so qualify it.
/// </summary>
public class Popup : PopupBase
{
    /// <summary>
    /// Defines the <c>CloseValue</c> attached property. Clicking a button inside an open popup that has it closes the
    /// popup with reason <see cref="PopupCloseReason.Handle"/> and this value.
    /// </summary>
    public static readonly AttachedProperty<object?> CloseValueProperty =
        AvaloniaProperty.RegisterAttached<Popup, Control, object?>("CloseValue");

    /// <summary>Gets <c>CloseValue</c>.</summary>
    public static object? GetCloseValue(Control element) => element.GetValue(CloseValueProperty);

    /// <summary>Sets <c>CloseValue</c>.</summary>
    public static void SetCloseValue(Control element, object? value) => element.SetValue(CloseValueProperty, value);

    /// <inheritdoc/>
    protected override PopupPresenter CreatePresenter() => new OnyxPopupPresenter();
}
