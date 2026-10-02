using System;
using Avalonia;
using Avalonia.Input;

namespace AvaEnyo.Primitives;

/// <summary>The state of a <see cref="DragTracker"/> drag.</summary>
public sealed class DragTrackerEventArgs : EventArgs
{
    internal DragTrackerEventArgs(PointerEventArgs? pointer, Vector delta, Vector step, Vector velocity, Point position)
    {
        Pointer = pointer;
        Delta = delta;
        Step = step;
        Velocity = velocity;
        Position = position;
    }

    /// <summary>The pointer event, when there is one.</summary>
    public PointerEventArgs? Pointer { get; }

    /// <summary>The movement since the press, in top-level coordinates.</summary>
    public Vector Delta { get; }

    /// <summary>The movement since the previous event.</summary>
    public Vector Step { get; }

    /// <summary>The release velocity in px/ms (only on completion).</summary>
    public Vector Velocity { get; }

    /// <summary>The pointer position in top-level coordinates.</summary>
    public Point Position { get; }

    /// <summary>Set by a <see cref="DragTracker.Pressed"/> handler to take the pointer at once (Enyo's tap-to-stop).</summary>
    public bool Capture { get; set; }
}
