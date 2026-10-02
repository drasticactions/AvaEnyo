using Avalonia.Controls;
using AvaEnyo.Controls.Primitives;

namespace AvaEnyo.Controls;

/// <summary>An empty element with <c>Flex.Value = 1</c>, to push toolbar items apart (<c>enyo.Spacer</c>).</summary>
public class Spacer : Control
{
    static Spacer()
    {
        Flex.ValueProperty.OverrideDefaultValue<Spacer>(1d);
        IsHitTestVisibleProperty.OverrideDefaultValue<Spacer>(false);
    }
}
