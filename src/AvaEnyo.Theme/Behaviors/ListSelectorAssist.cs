using Avalonia;
using Avalonia.Controls;

namespace AvaEnyo;

/// <summary>
/// Attached properties for the Enyo <c>ListSelector</c> look of a <see cref="ComboBox"/>: an optional label in front of
/// the value, and an option to hide the arrow.
/// </summary>
public static class ListSelectorAssist
{
    /// <summary>Defines the <c>Label</c> attached property.</summary>
    public static readonly AttachedProperty<string?> LabelProperty =
        AvaloniaProperty.RegisterAttached<ComboBox, string?>("Label", typeof(ListSelectorAssist));

    /// <summary>Defines the <c>HideArrow</c> attached property.</summary>
    public static readonly AttachedProperty<bool> HideArrowProperty =
        AvaloniaProperty.RegisterAttached<ComboBox, bool>("HideArrow", typeof(ListSelectorAssist));

    /// <summary>Gets <c>Label</c>.</summary>
    public static string? GetLabel(ComboBox element) => element.GetValue(LabelProperty);

    /// <summary>Sets <c>Label</c>: text shown before the value, in the Enyo label style.</summary>
    public static void SetLabel(ComboBox element, string? value) => element.SetValue(LabelProperty, value);

    /// <summary>Gets <c>HideArrow</c>.</summary>
    public static bool GetHideArrow(ComboBox element) => element.GetValue(HideArrowProperty);

    /// <summary>Sets <c>HideArrow</c>: true hides the drop-down arrow.</summary>
    public static void SetHideArrow(ComboBox element, bool value) => element.SetValue(HideArrowProperty, value);
}
