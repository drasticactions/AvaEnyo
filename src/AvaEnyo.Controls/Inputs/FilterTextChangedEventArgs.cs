using Avalonia.Interactivity;

namespace AvaEnyo.Controls;

/// <summary>Data of <see cref="TokenInput.FilterTextChanged"/>: the text to filter suggestions by.</summary>
public sealed class FilterTextChangedEventArgs : RoutedEventArgs
{
    /// <summary>Initializes a new instance of the <see cref="FilterTextChangedEventArgs"/> class.</summary>
    public FilterTextChangedEventArgs(RoutedEvent routedEvent, object? source, string text)
        : base(routedEvent, source)
    {
        Text = text;
    }

    /// <summary>The filter text.</summary>
    public string Text { get; }
}
