using Avalonia.Controls;

namespace AvaEnyo.Controls.Primitives;

/// <summary>
/// The items panel of a <see cref="Toolbar"/>: a centered <see cref="HFlexBox"/> that pins a
/// <see cref="GrabButton"/> to its left edge outside the flow, as Onyx positioned the grab button absolutely.
/// </summary>
public class ToolbarPanel : HFlexBox
{
    /// <summary>Initializes a new instance of the <see cref="ToolbarPanel"/> class.</summary>
    public ToolbarPanel()
    {
        Pack = FlexPack.Center;
        Align = FlexAlign.Center;
    }

    /// <inheritdoc/>
    protected override bool IsOutOfFlow(Control child) => child is GrabButton;

    /// <inheritdoc/>
    /// <remarks>The grab button's width, so left-packed items (a toolbar with a spacer) do not cover it.</remarks>
    protected override double FlowStartInset
    {
        get
        {
            foreach (var child in Children)
            {
                if (child is GrabButton { IsVisible: true } grab)
                {
                    return grab.DesiredSize.Width;
                }
            }

            return 0;
        }
    }
}
