using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AvaEnyo.Controls.Primitives;

/// <summary>Data of the <see cref="EnyoGestures.DragStartedEvent"/> (Enyo <c>dragstart</c>).</summary>
public sealed class DragStartedEventArgs : RoutedEventArgs
{
    /// <summary>Initializes a new instance of the <see cref="DragStartedEventArgs"/> class.</summary>
    public DragStartedEventArgs(RoutedEvent routedEvent, object? source, Vector delta, bool isHorizontal, bool isLockable, PointerType pointerType)
        : base(routedEvent, source)
    {
        Delta = delta;
        IsHorizontal = isHorizontal;
        IsLockable = isLockable;
        PointerType = pointerType;
    }

    /// <summary>The movement since the press.</summary>
    public Vector Delta { get; }

    /// <summary>True when the drag is mostly horizontal.</summary>
    public bool IsHorizontal { get; }

    /// <summary>True when the drag is within 22.5° of its main axis.</summary>
    public bool IsLockable { get; }

    /// <summary>The pointer that drags.</summary>
    public PointerType PointerType { get; }
}
