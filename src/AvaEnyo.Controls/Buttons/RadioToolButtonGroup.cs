using Avalonia.Controls;
using Avalonia.Controls.Templates;
using AvaEnyo.Controls.Primitives;

namespace AvaEnyo.Controls;

/// <summary>
/// A segmented group of tool buttons where one is selected (<c>enyo.RadioToolButtonGroup</c>). Items become
/// <see cref="RadioToolButton"/>s; <see cref="Avalonia.Controls.Primitives.SelectingItemsControl.SelectedIndex"/>
/// follows the pressed one.
/// </summary>
public class RadioToolButtonGroup : RadioGroup
{
    private static readonly FuncTemplate<Panel?> s_panel = new(() => new OrderedPanel { Orientation = Avalonia.Layout.Orientation.Horizontal });

    static RadioToolButtonGroup()
    {
        ItemsPanelProperty.OverrideDefaultValue<RadioToolButtonGroup>(s_panel);
    }

    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => new RadioToolButton();

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) =>
        NeedsContainer<RadioToolButton>(item, out recycleKey);
}
