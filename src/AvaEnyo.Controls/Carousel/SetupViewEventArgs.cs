using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvaEnyo.Controls;

/// <summary>
/// Data of <see cref="VirtualCarousel.SetupViewRequested"/> (Enyo <c>onSetupView</c>): fill <see cref="View"/> for
/// <see cref="Index"/>, set <see cref="Accepted"/> to true when the index exists, and mark the event handled.
/// </summary>
public sealed class SetupViewEventArgs : RoutedEventArgs
{
    /// <summary>Initializes a new instance of the <see cref="SetupViewEventArgs"/> class.</summary>
    public SetupViewEventArgs(RoutedEvent routedEvent, object? source, Control view, int index)
        : base(routedEvent, source)
    {
        View = view;
        Index = index;
    }

    /// <summary>The reused view to update.</summary>
    public Control View { get; }

    /// <summary>The view index to show.</summary>
    public int Index { get; }

    /// <summary>True when the view should be shown; false at an end.</summary>
    public bool Accepted { get; set; }
}
