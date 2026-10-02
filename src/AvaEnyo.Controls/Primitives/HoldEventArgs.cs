using System;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AvaEnyo.Controls.Primitives;

/// <summary>Data of the <see cref="EnyoGestures.HoldEvent"/> and <see cref="EnyoGestures.HoldPulseEvent"/>.</summary>
public sealed class HoldEventArgs : RoutedEventArgs
{
    /// <summary>Initializes a new instance of the <see cref="HoldEventArgs"/> class.</summary>
    public HoldEventArgs(RoutedEvent routedEvent, object? source, TimeSpan elapsed, Point position, PointerType pointerType)
        : base(routedEvent, source)
    {
        Elapsed = elapsed;
        Position = position;
        PointerType = pointerType;
    }

    /// <summary>Time since the press.</summary>
    public TimeSpan Elapsed { get; }

    /// <summary>The press position, in top-level coordinates.</summary>
    public Point Position { get; }

    /// <summary>The pointer that holds.</summary>
    public PointerType PointerType { get; }
}
