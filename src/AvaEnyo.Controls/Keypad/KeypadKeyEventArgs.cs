using Avalonia.Interactivity;

namespace AvaEnyo.Controls;

/// <summary>Data of <see cref="NumericKeypad.KeyPressed"/>.</summary>
public sealed class KeypadKeyEventArgs : RoutedEventArgs
{
    /// <summary>Initializes a new instance of the <see cref="KeypadKeyEventArgs"/> class.</summary>
    public KeypadKeyEventArgs(RoutedEvent routedEvent, object? source, string key)
        : base(routedEvent, source)
    {
        Key = key;
    }

    /// <summary>
    /// The key: a digit, <c>*</c>, <c>#</c>, <c>+</c> (0 held in Phone mode), or <see cref="NumericKeypad.BackspaceKey"/>.
    /// </summary>
    public string Key { get; }
}
