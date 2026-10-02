using Avalonia.Interactivity;

namespace AvaEnyo.Controls;

/// <summary>Data of <see cref="VirtualList.AcquirePage"/> and <see cref="VirtualList.DiscardPage"/>.</summary>
public sealed class PageEventArgs : RoutedEventArgs
{
    /// <summary>Initializes a new instance of the <see cref="PageEventArgs"/> class.</summary>
    public PageEventArgs(RoutedEvent routedEvent, object? source, int page)
        : base(routedEvent, source)
    {
        Page = page;
    }

    /// <summary>The page number, from 0.</summary>
    public int Page { get; }

    /// <summary>
    /// Set by an <c>AcquirePage</c> handler to false when the page does not exist (past the end), so the list stops
    /// asking for later pages. True by default.
    /// </summary>
    public bool Exists { get; set; } = true;
}
