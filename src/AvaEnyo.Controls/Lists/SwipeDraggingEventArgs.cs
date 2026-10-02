using Avalonia.Interactivity;

namespace AvaEnyo.Controls;

/// <summary>Data of <see cref="SwipeableItem.Dragging"/>: the row's offset from rest.</summary>
public sealed class SwipeDraggingEventArgs : RoutedEventArgs
{
    /// <summary>Initializes a new instance of the <see cref="SwipeDraggingEventArgs"/> class.</summary>
    public SwipeDraggingEventArgs(RoutedEvent routedEvent, object? source, double dx)
        : base(routedEvent, source)
    {
        Dx = dx;
    }

    /// <summary>The horizontal offset of the row, in px. 0 at rest.</summary>
    public double Dx { get; }
}
