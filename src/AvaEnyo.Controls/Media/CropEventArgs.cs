using Avalonia;
using Avalonia.Interactivity;

namespace AvaEnyo.Controls;

/// <summary>Data of <see cref="CroppableImage.CropRequested"/>: the crop in source image units (Enyo <c>onCrop</c>).</summary>
public sealed class CropEventArgs : RoutedEventArgs
{
    /// <summary>Initializes a new instance of the <see cref="CropEventArgs"/> class.</summary>
    public CropEventArgs(RoutedEvent routedEvent, object? source, Rect cropRect, double zoom)
        : base(routedEvent, source)
    {
        CropRect = cropRect;
        Zoom = zoom;
    }

    /// <summary>The part of the source image inside the crop frame.</summary>
    public Rect CropRect { get; }

    /// <summary>The zoom at the time of the crop.</summary>
    public double Zoom { get; }
}
