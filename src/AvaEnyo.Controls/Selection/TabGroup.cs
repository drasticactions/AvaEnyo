using Avalonia.Controls;

namespace AvaEnyo.Controls;

/// <summary>
/// A <see cref="RadioGroup"/> with the tab look (<c>enyo.TabGroup</c> and <c>enyo-tabbutton</c>): 18 px text and
/// the middle segment frame for every tab.
/// </summary>
public class TabGroup : RadioGroup
{
    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => new TabSegment();

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) =>
        NeedsContainer<RadioSegment>(item, out recycleKey);
}
