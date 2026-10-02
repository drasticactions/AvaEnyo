using Avalonia;
using Avalonia.Input;

namespace AvaEnyo.Primitives;

/// <summary>What a drag looks like when it passes the drag threshold (Enyo's <c>dragstart</c> event).</summary>
/// <param name="Delta">The movement since the press.</param>
/// <param name="IsHorizontal">True when the horizontal movement is larger.</param>
/// <param name="IsLockable">True when the off-axis ratio is under <see cref="ThemeSettings.LockRatio"/>.</param>
/// <param name="PointerType">The pointer that drags.</param>
/// <param name="Source">The element that was pressed.</param>
public readonly record struct DragStartInfo(Vector Delta, bool IsHorizontal, bool IsLockable, PointerType PointerType, object? Source)
{
    /// <summary>True when the drag is mostly vertical.</summary>
    public bool IsVertical => !IsHorizontal;
}
