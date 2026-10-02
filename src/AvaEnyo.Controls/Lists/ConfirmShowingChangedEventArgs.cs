using Avalonia.Interactivity;

namespace AvaEnyo.Controls;

/// <summary>Data of <see cref="SwipeableItem.ConfirmShowingChanged"/>.</summary>
public sealed class ConfirmShowingChangedEventArgs : RoutedEventArgs
{
    /// <summary>Initializes a new instance of the <see cref="ConfirmShowingChangedEventArgs"/> class.</summary>
    public ConfirmShowingChangedEventArgs(RoutedEvent routedEvent, object? source, bool isShowing, bool didAutoConfirm)
        : base(routedEvent, source)
    {
        IsShowing = isShowing;
        DidAutoConfirm = didAutoConfirm;
    }

    /// <summary>True when the confirm prompt is now shown.</summary>
    public bool IsShowing { get; }

    /// <summary>True when showing this prompt hid another row's prompt and that row auto-confirmed.</summary>
    public bool DidAutoConfirm { get; }
}
