using Avalonia.Interactivity;

namespace AvaEnyo.Controls;

/// <summary>Data of the <c>Snap</c> and <c>SnapFinished</c> events of <see cref="SnapScroller"/> and <see cref="Carousel"/>.</summary>
public sealed class SnapEventArgs : RoutedEventArgs
{
    /// <summary>Initializes a new instance of the <see cref="SnapEventArgs"/> class.</summary>
    public SnapEventArgs(RoutedEvent routedEvent, object? source, int index)
        : base(routedEvent, source)
    {
        Index = index;
    }

    /// <summary>The index snapped to.</summary>
    public int Index { get; }
}
