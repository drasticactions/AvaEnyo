using Avalonia.Interactivity;

namespace AvaEnyo.Controls;

/// <summary>Data of the <c>ValueChanged</c> events of the pickers (Enyo <c>onChange</c>).</summary>
public sealed class PickerValueChangedEventArgs : RoutedEventArgs
{
    /// <summary>Initializes a new instance of the <see cref="PickerValueChangedEventArgs"/> class.</summary>
    public PickerValueChangedEventArgs(RoutedEvent routedEvent, object? source, object? oldValue, object? newValue)
        : base(routedEvent, source)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }

    /// <summary>The value before the change.</summary>
    public object? OldValue { get; }

    /// <summary>The value after the change.</summary>
    public object? NewValue { get; }
}
