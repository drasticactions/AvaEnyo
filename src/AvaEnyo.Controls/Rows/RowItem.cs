namespace AvaEnyo.Controls;

/// <summary>
/// The row a <see cref="RowGroup"/> wraps around each child that is not already an <see cref="Item"/>
/// (Enyo <c>RowItem</c>). It takes the order pseudo-classes, so a text box inside gets the first, middle or last
/// focus frame.
/// </summary>
public class RowItem : Item
{
    /// <summary>Initializes a new instance of the <see cref="RowItem"/> class.</summary>
    public RowItem()
    {
        TapHighlight = false;
    }
}
