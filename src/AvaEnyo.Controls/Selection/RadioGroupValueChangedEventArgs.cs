using Avalonia.Interactivity;

namespace AvaEnyo.Controls;

/// <summary>Data of <see cref="RadioGroup.ValueChanged"/>.</summary>
public sealed class RadioGroupValueChangedEventArgs : RoutedEventArgs
{
    /// <summary>Initializes a new instance of the <see cref="RadioGroupValueChangedEventArgs"/> class.</summary>
    public RadioGroupValueChangedEventArgs(RoutedEvent routedEvent, object? source, object? oldValue, object? newValue)
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
