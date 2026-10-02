using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AvaEnyo.Controls;

/// <summary>
/// A token in a <see cref="TokenInput"/> (Enyo <c>ContactAtom</c>): a rounded pill, blue with white text while
/// <see cref="IsSelected"/>. Delete or Backspace on a focused chip raises <see cref="Remove"/>.
/// </summary>
[PseudoClasses(":selected")]
public class TokenChip : Button
{
    /// <summary>Defines the <see cref="IsSelected"/> property.</summary>
    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<TokenChip, bool>(nameof(IsSelected));

    /// <summary>Defines the <see cref="Remove"/> event.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> RemoveEvent =
        RoutedEvent.Register<TokenChip, RoutedEventArgs>(nameof(Remove), RoutingStrategies.Bubble);

    static TokenChip()
    {
        IsSelectedProperty.Changed.AddClassHandler<TokenChip>((c, e) => c.PseudoClasses.Set(":selected", e.GetNewValue<bool>()));
    }

    /// <summary>True when the chip is selected; Backspace in the text box then removes it.</summary>
    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>Raised when the chip asks to be removed.</summary>
    public event EventHandler<RoutedEventArgs>? Remove
    {
        add => AddHandler(RemoveEvent, value);
        remove => RemoveHandler(RemoveEvent, value);
    }

    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!e.Handled && (e.Key == Key.Delete || e.Key == Key.Back))
        {
            e.Handled = true;
            RaiseEvent(new RoutedEventArgs(RemoveEvent, this));
            return;
        }

        base.OnKeyDown(e);
    }
}
