using Avalonia.Interactivity;

namespace AvaEnyo.Controls;

/// <summary>Data of <see cref="PopupBase.BeforeOpen"/>. Set <c>Cancel</c> to keep the popup closed.</summary>
public sealed class PopupOpeningEventArgs : CancelRoutedEventArgs
{
    /// <summary>Initializes a new instance of the <see cref="PopupOpeningEventArgs"/> class.</summary>
    public PopupOpeningEventArgs(RoutedEvent routedEvent, object? source, bool isFirstOpen)
        : base(routedEvent, source)
    {
        IsFirstOpen = isFirstOpen;
    }

    /// <summary>True the first time the popup opens, before its content exists.</summary>
    public bool IsFirstOpen { get; }
}
