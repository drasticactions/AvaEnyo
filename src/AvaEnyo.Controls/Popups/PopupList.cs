using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace AvaEnyo.Controls;

/// <summary>
/// A long list of choices in a pop-up menu (<c>enyo.PopupList</c>): rows are virtualized, so only the visible ones
/// exist.
/// </summary>
public class PopupList : PopupListBase
{
    /// <inheritdoc/>
    protected override ITemplate<Panel?> CreateItemsPanel() => new FuncTemplate<Panel?>(() => new VirtualizingStackPanel());
}
