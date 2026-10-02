using Avalonia.Layout;

namespace AvaEnyo.Controls.Primitives;

/// <summary>A <see cref="FlexPanel"/> that lays its children out top to bottom (Enyo <c>VFlexBox</c>).</summary>
public class VFlexBox : FlexPanel
{
    /// <inheritdoc/>
    protected override Orientation Orientation => Orientation.Vertical;
}
