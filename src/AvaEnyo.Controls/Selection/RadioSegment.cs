using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace AvaEnyo.Controls;

/// <summary>
/// One segment of a <see cref="RadioGroup"/> (Enyo <c>RadioButton</c> in a <c>RadioGroup</c>): drawn from
/// <c>radiobutton.png</c>, light when up and blue when checked, with first, middle, last and single shapes.
/// </summary>
public class RadioSegment : RadioButton, ISelectable
{
    /// <summary>Defines the <see cref="IsSelected"/> property.</summary>
    public static readonly StyledProperty<bool> IsSelectedProperty =
        SelectingItemsControl.IsSelectedProperty.AddOwner<RadioSegment>();

    /// <summary>Defines the <see cref="Value"/> property.</summary>
    public static readonly StyledProperty<object?> ValueProperty =
        AvaloniaProperty.Register<RadioSegment, object?>(nameof(Value));

    static RadioSegment()
    {
        IsSelectedProperty.Changed.AddClassHandler<RadioSegment>((s, e) =>
        {
            var selected = e.GetNewValue<bool>();
            if (s.IsChecked != selected)
            {
                s.SetCurrentValue(IsCheckedProperty, selected);
            }
        });
        IsCheckedProperty.Changed.AddClassHandler<RadioSegment>((s, e) =>
        {
            var check = e.GetNewValue<bool?>() == true;
            if (s.IsSelected != check)
            {
                s.SetCurrentValue(IsSelectedProperty, check);
                s.RaiseEvent(new RoutedEventArgs(SelectingItemsControl.IsSelectedChangedEvent, s));
            }
        });
    }

    /// <summary>True when the segment is the selected one.</summary>
    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>The value the group reports when this segment is selected. Null means its index.</summary>
    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
}
