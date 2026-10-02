using Avalonia.Interactivity;

namespace AvaEnyo.Controls;

/// <summary>Data of <see cref="SlidingPane.SlideComplete"/>: the view selected when the slide ended.</summary>
public sealed class SlideCompleteEventArgs : RoutedEventArgs
{
    /// <summary>Initializes a new instance of the <see cref="SlideCompleteEventArgs"/> class.</summary>
    public SlideCompleteEventArgs(RoutedEvent routedEvent, object? source, SlidingView? view)
        : base(routedEvent, source)
    {
        View = view;
    }

    /// <summary>The selected view.</summary>
    public SlidingView? View { get; }
}
