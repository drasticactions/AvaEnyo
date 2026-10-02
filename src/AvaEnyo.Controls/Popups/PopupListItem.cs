using System;

namespace AvaEnyo.Controls;

/// <summary>
/// A row of a <see cref="PopupSelect"/> or <see cref="PopupList"/>: an <see cref="Item"/> whose selected look is bold
/// text on white (<c>enyo-picker-item-selected</c>).
/// </summary>
public class PopupListItem : Item
{
    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(Item);
}
