using Avalonia.Interactivity;

namespace AvaEnyo.Controls;

/// <summary>
/// Data of <see cref="Carousel.GetLeft"/> and <see cref="Carousel.GetRight"/> (Enyo <c>onGetLeft</c>/<c>onGetRight</c>).
/// Set <see cref="View"/> to the item for that side, or leave it null at an end.
/// </summary>
public sealed class CarouselViewRequestedEventArgs : RoutedEventArgs
{
    /// <summary>Initializes a new instance of the <see cref="CarouselViewRequestedEventArgs"/> class.</summary>
    public CarouselViewRequestedEventArgs(RoutedEvent routedEvent, object? source, bool isSnap)
        : base(routedEvent, source)
    {
        IsSnap = isSnap;
    }

    /// <summary>True when the request follows a snap toward that side, so the app can move its own index.</summary>
    public bool IsSnap { get; }

    /// <summary>The item (or control) for that side; null means there is none.</summary>
    public object? View { get; set; }
}
