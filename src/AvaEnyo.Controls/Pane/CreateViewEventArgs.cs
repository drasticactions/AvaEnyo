using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvaEnyo.Controls;

/// <summary>
/// Data of <see cref="Pane.CreateView"/> (Enyo <c>onCreateView</c>): a view was selected by a name no view has. Set
/// <see cref="View"/> to create it.
/// </summary>
public sealed class CreateViewEventArgs : RoutedEventArgs
{
    /// <summary>Initializes a new instance of the <see cref="CreateViewEventArgs"/> class.</summary>
    public CreateViewEventArgs(RoutedEvent routedEvent, object? source, string name)
        : base(routedEvent, source)
    {
        Name = name;
    }

    /// <summary>The requested view name.</summary>
    public string Name { get; }

    /// <summary>The view to add and select. Its <c>Name</c> is set to <see cref="Name"/> when empty.</summary>
    public Control? View { get; set; }
}
