using Avalonia.Interactivity;

namespace AvaEnyo.Controls;

/// <summary>
/// Data of <see cref="TokenInput.Atomizing"/>: text is about to become a token. Set <see cref="Token"/> to store a
/// different object, or <c>Cancel</c> to keep the text in the box.
/// </summary>
public sealed class AtomizingEventArgs : CancelRoutedEventArgs
{
    /// <summary>Initializes a new instance of the <see cref="AtomizingEventArgs"/> class.</summary>
    public AtomizingEventArgs(RoutedEvent routedEvent, object? source, string text)
        : base(routedEvent, source)
    {
        Text = text;
        Token = text;
    }

    /// <summary>The trimmed text to atomize.</summary>
    public string Text { get; }

    /// <summary>The object added to <see cref="TokenInput.Tokens"/>. Defaults to <see cref="Text"/>.</summary>
    public object? Token { get; set; }
}
