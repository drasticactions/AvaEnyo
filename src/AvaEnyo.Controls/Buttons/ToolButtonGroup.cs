using Avalonia.Controls;
using Avalonia.Controls.Templates;
using AvaEnyo.Controls.Primitives;

namespace AvaEnyo.Controls;

/// <summary>
/// A segmented group of tool buttons on a toolbar (<c>enyo.ToolButtonGroup</c>): the buttons sit side by side in an
/// <see cref="OrderedPanel"/> and share the segmented frame. The <c>dark</c> class is for light toolbars.
/// </summary>
public class ToolButtonGroup : ItemsControl
{
    private static readonly FuncTemplate<Panel?> s_panel = new(() => new OrderedPanel { Orientation = Avalonia.Layout.Orientation.Horizontal });

    static ToolButtonGroup()
    {
        ItemsPanelProperty.OverrideDefaultValue<ToolButtonGroup>(s_panel);
    }

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) =>
        NeedsContainer<Control>(item, out recycleKey);

    /// <inheritdoc/>
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is ToolButton button)
        {
            button.Classes.Set("grouped", true);
            button.Classes.Set("dark", Classes.Contains("dark"));
        }
    }

    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => new ToolButton();
}
