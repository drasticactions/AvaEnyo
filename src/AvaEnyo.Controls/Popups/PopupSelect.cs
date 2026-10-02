using Avalonia.Controls;
using Avalonia.Controls.Templates;
using AvaEnyo.Controls.Primitives;

namespace AvaEnyo.Controls;

/// <summary>
/// A short list of choices in a pop-up menu (<c>enyo.PopupSelect</c>). Every row is created; for long lists use
/// <see cref="PopupList"/>.
/// </summary>
public class PopupSelect : PopupListBase
{
    /// <inheritdoc/>
    protected override ITemplate<Panel?> CreateItemsPanel() => new FuncTemplate<Panel?>(() => new OrderedPanel());
}
