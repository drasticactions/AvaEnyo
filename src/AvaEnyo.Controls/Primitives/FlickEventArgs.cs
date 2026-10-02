using Avalonia.Interactivity;

namespace AvaEnyo.Controls.Primitives;

/// <summary>Data of the <see cref="EnyoGestures.FlickEvent"/>: the release velocity in px/ms.</summary>
public sealed class FlickEventArgs : RoutedEventArgs
{
    /// <summary>Initializes a new instance of the <see cref="FlickEventArgs"/> class.</summary>
    public FlickEventArgs(RoutedEvent routedEvent, object? source, double xVelocity, double yVelocity)
        : base(routedEvent, source)
    {
        XVelocity = xVelocity;
        YVelocity = yVelocity;
    }

    /// <summary>Horizontal velocity in px/ms (positive to the right).</summary>
    public double XVelocity { get; }

    /// <summary>Vertical velocity in px/ms (positive downward).</summary>
    public double YVelocity { get; }
}
