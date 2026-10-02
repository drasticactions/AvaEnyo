using Avalonia.Interactivity;

namespace AvaEnyo.Controls;

/// <summary>Data of <see cref="SlidingView.Resized"/>: the width the view was fitted to, or null when cleared.</summary>
public sealed class SlidingViewResizedEventArgs : RoutedEventArgs
{
    /// <summary>Initializes a new instance of the <see cref="SlidingViewResizedEventArgs"/> class.</summary>
    public SlidingViewResizedEventArgs(RoutedEvent routedEvent, object? source, double? width)
        : base(routedEvent, source)
    {
        Width = width;
    }

    /// <summary>The fitted width, or null when the view went back to its natural width.</summary>
    public double? Width { get; }
}
