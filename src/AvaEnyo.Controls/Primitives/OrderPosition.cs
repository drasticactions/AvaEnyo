namespace AvaEnyo.Controls.Primitives;

/// <summary>The place of a child among the visible children of an <see cref="OrderedPanel"/>.</summary>
public enum OrderPosition
{
    /// <summary>Not in an ordered panel, or hidden.</summary>
    None,

    /// <summary>The first of several (<c>:first</c>).</summary>
    First,

    /// <summary>Neither first nor last (<c>:middle</c>).</summary>
    Middle,

    /// <summary>The last of several (<c>:last</c>).</summary>
    Last,

    /// <summary>The only visible child (<c>:single</c>).</summary>
    Single,
}
