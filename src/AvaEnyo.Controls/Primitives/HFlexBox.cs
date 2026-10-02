using Avalonia.Layout;

namespace AvaEnyo.Controls.Primitives;

/// <summary>A <see cref="FlexPanel"/> that lays its children out left to right (Enyo <c>HFlexBox</c>).</summary>
public class HFlexBox : FlexPanel
{
    /// <inheritdoc/>
    protected override Orientation Orientation => Orientation.Horizontal;
}
