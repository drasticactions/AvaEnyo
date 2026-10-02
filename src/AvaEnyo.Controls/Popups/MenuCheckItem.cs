using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace AvaEnyo.Controls;

/// <summary>
/// A <see cref="MenuItem"/> with a check mark on the right while <see cref="IsChecked"/> (<c>enyo.MenuCheckItem</c>,
/// <c>checkmark.png</c>). Tapping it toggles the check.
/// </summary>
[PseudoClasses(":checked")]
public class MenuCheckItem : MenuItem
{
    /// <summary>Defines the <see cref="IsChecked"/> property.</summary>
    public static readonly StyledProperty<bool> IsCheckedProperty =
        AvaloniaProperty.Register<MenuCheckItem, bool>(nameof(IsChecked), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    static MenuCheckItem()
    {
        IsCheckedProperty.Changed.AddClassHandler<MenuCheckItem>((m, e) => m.PseudoClasses.Set(":checked", e.GetNewValue<bool>()));
    }

    /// <summary>True when the item shows its check mark.</summary>
    public bool IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnClick()
    {
        SetCurrentValue(IsCheckedProperty, !IsChecked);
        base.OnClick();
    }
}
