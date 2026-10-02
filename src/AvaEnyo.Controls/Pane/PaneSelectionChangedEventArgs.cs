using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvaEnyo.Controls;

/// <summary>Data of <see cref="Pane.SelectionChanged"/> (Enyo <c>onSelectView</c>).</summary>
public sealed class PaneSelectionChangedEventArgs : RoutedEventArgs
{
    /// <summary>Initializes a new instance of the <see cref="PaneSelectionChangedEventArgs"/> class.</summary>
    public PaneSelectionChangedEventArgs(RoutedEvent routedEvent, object? source, Control? view, Control? previousView)
        : base(routedEvent, source)
    {
        View = view;
        PreviousView = previousView;
    }

    /// <summary>The view now selected.</summary>
    public Control? View { get; }

    /// <summary>The view selected before.</summary>
    public Control? PreviousView { get; }
}
