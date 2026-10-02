using Avalonia;
using Avalonia.Controls;

namespace AvaEnyo.Controls;

/// <summary>
/// A button that shows a <see cref="Spinner"/> next to its caption while <see cref="IsActive"/> is true
/// (<c>enyo.ActivityButton</c>), for actions such as Send that take a moment.
/// </summary>
public class ActivityButton : Button
{
    /// <summary>Defines the <see cref="IsActive"/> property.</summary>
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<ActivityButton, bool>(nameof(IsActive));

    /// <summary>True while the activity runs: the spinner shows and turns.</summary>
    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }
}
