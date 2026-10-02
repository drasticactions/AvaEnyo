using System;
using Avalonia;

namespace AvaEnyo.Services;

/// <summary>Data of the on-screen keyboard events.</summary>
public sealed class KeyboardEventArgs : EventArgs
{
    /// <summary>Initializes a new instance of the <see cref="KeyboardEventArgs"/> class.</summary>
    public KeyboardEventArgs(Rect occludedRect)
    {
        OccludedRect = occludedRect;
    }

    /// <summary>The part of the window the keyboard covers, in window coordinates.</summary>
    public Rect OccludedRect { get; }
}
