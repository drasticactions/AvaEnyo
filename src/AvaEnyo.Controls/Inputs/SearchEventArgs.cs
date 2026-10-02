using Avalonia.Interactivity;

namespace AvaEnyo.Controls;

/// <summary>Data of <see cref="Search.CommittedEvent"/>: the search text.</summary>
public sealed class SearchEventArgs : RoutedEventArgs
{
    /// <summary>Initializes a new instance of the <see cref="SearchEventArgs"/> class.</summary>
    public SearchEventArgs(RoutedEvent routedEvent, object? source, string text)
        : base(routedEvent, source)
    {
        Text = text;
    }

    /// <summary>The text to search for.</summary>
    public string Text { get; }
}
