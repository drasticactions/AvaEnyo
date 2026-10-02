namespace AvaEnyo.Controls;

/// <summary>How a <see cref="VirtualList"/> arranges its rows.</summary>
public enum VirtualListLayout
{
    /// <summary>One row per item, top to bottom (the default).</summary>
    List,

    /// <summary>A grid of equal cells (<see cref="Primitives.VirtualizingUniformGridPanel"/>).</summary>
    Grid,
}
