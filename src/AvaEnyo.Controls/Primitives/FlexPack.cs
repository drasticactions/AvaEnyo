namespace AvaEnyo.Controls.Primitives;

/// <summary>Placement of the children along the main axis of a flex box (Enyo <c>pack</c>).</summary>
public enum FlexPack
{
    /// <summary>At the start (the default).</summary>
    Start,

    /// <summary>Centered.</summary>
    Center,

    /// <summary>At the end.</summary>
    End,

    /// <summary>Spread out: the first child at the start, the last at the end, equal gaps between.</summary>
    Justify,
}
