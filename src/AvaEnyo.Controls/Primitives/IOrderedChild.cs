namespace AvaEnyo.Controls.Primitives;

/// <summary>
/// A control that wants to know its place in an <see cref="OrderedPanel"/>. The panel calls
/// <see cref="SetOrderPosition"/> in addition to setting the pseudo-classes.
/// </summary>
internal interface IOrderedChild
{
    void SetOrderPosition(OrderPosition position);
}
