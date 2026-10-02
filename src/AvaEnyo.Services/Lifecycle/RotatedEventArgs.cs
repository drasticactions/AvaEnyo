using System;

namespace AvaEnyo.Services;

/// <summary>Data of <see cref="IApplicationEvents.Rotated"/>.</summary>
public sealed class RotatedEventArgs : EventArgs
{
    /// <summary>Initializes a new instance of the <see cref="RotatedEventArgs"/> class.</summary>
    public RotatedEventArgs(DisplayOrientation orientation)
    {
        Orientation = orientation;
    }

    /// <summary>The new orientation.</summary>
    public DisplayOrientation Orientation { get; }
}
