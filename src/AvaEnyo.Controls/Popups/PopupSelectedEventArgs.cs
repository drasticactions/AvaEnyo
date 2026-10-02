using Avalonia.Interactivity;

namespace AvaEnyo.Controls;

/// <summary>Data of <see cref="PopupListBase.Selected"/> (Enyo <c>onSelect</c>).</summary>
public sealed class PopupSelectedEventArgs : RoutedEventArgs
{
    /// <summary>Initializes a new instance of the <see cref="PopupSelectedEventArgs"/> class.</summary>
    public PopupSelectedEventArgs(RoutedEvent routedEvent, object? source, int index, object? item, object? previousItem)
        : base(routedEvent, source)
    {
        Index = index;
        Item = item;
        PreviousItem = previousItem;
    }

    /// <summary>The index of the chosen item.</summary>
    public int Index { get; }

    /// <summary>The chosen item.</summary>
    public object? Item { get; }

    /// <summary>The item selected before.</summary>
    public object? PreviousItem { get; }
}
