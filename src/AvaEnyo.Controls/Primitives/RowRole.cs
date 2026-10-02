namespace AvaEnyo.Controls.Primitives;

/// <summary>How a child of a <see cref="RowPanel"/> takes horizontal space.</summary>
public enum RowRole
{
    /// <summary>Its natural width, from the left.</summary>
    Fixed,

    /// <summary>Its natural width, but it gives up space first when the row is too narrow (a truncating caption).</summary>
    Shrink,

    /// <summary>The space left over, never less than its MinWidth (a divider rule).</summary>
    Fill,

    /// <summary>Its natural width, at the right end.</summary>
    End,
}
