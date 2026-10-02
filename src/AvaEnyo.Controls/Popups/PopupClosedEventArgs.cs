using Avalonia.Interactivity;

namespace AvaEnyo.Controls;

/// <summary>Data of <see cref="PopupBase.Closed"/>.</summary>
public sealed class PopupClosedEventArgs : RoutedEventArgs
{
    /// <summary>Initializes a new instance of the <see cref="PopupClosedEventArgs"/> class.</summary>
    public PopupClosedEventArgs(RoutedEvent routedEvent, object? source, PopupCloseReason reason, object? value)
        : base(routedEvent, source)
    {
        Reason = reason;
        Value = value;
    }

    /// <summary>Why the popup closed.</summary>
    public PopupCloseReason Reason { get; }

    /// <summary>The close value: the <c>CloseValue</c> of the control that closed it, or the value given to <c>Close</c>.</summary>
    public object? Value { get; }
}
