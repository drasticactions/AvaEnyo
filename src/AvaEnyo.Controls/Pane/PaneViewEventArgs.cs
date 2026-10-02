using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvaEnyo.Controls;

/// <summary>Data of <see cref="Pane.ViewShown"/> and <see cref="Pane.ViewHidden"/>.</summary>
public sealed class PaneViewEventArgs : RoutedEventArgs
{
    /// <summary>Initializes a new instance of the <see cref="PaneViewEventArgs"/> class.</summary>
    public PaneViewEventArgs(RoutedEvent routedEvent, object? source, Control view)
        : base(routedEvent, source)
    {
        View = view;
    }

    /// <summary>The view shown or hidden.</summary>
    public Control View { get; }
}
