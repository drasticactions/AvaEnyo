using Avalonia.Interactivity;

namespace AvaEnyo.Controls;

/// <summary>Data of <see cref="VirtualList.Reordering"/>. Set <c>Cancel</c> to leave the item where it was.</summary>
public sealed class ReorderingEventArgs : CancelRoutedEventArgs
{
    /// <summary>Initializes a new instance of the <see cref="ReorderingEventArgs"/> class.</summary>
    public ReorderingEventArgs(RoutedEvent routedEvent, object? source, int from, int to)
        : base(routedEvent, source)
    {
        From = from;
        To = to;
    }

    /// <summary>The index the item was lifted from.</summary>
    public int From { get; }

    /// <summary>The index the item was dropped at.</summary>
    public int To { get; }
}
