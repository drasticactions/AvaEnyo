namespace AvaEnyo.Controls.Primitives;

/// <summary>Placement of the children across the main axis of a flex box (Enyo <c>align</c>).</summary>
public enum FlexAlign
{
    /// <summary>At the start of the cross axis.</summary>
    Start,

    /// <summary>Centered on the cross axis.</summary>
    Center,

    /// <summary>At the end of the cross axis.</summary>
    End,

    /// <summary>
    /// On a shared text baseline (horizontal boxes). A child without text is placed at the start.
    /// </summary>
    Baseline,

    /// <summary>Stretched across the box (the default).</summary>
    Stretch,
}
