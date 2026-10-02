using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace AvaEnyo.Controls;

/// <summary>
/// A toolbar button (<c>enyo.ToolButton</c>): transparent until pressed, then the <c>button-toolbar.png</c> frame.
/// Icon only, caption only, or both. With <see cref="IsToggle"/> it stays down while checked.
/// </summary>
[PseudoClasses(":captioned", ":checked")]
public class ToolButton : IconButton
{
    /// <summary>Defines the <see cref="IsToggle"/> property.</summary>
    public static readonly StyledProperty<bool> IsToggleProperty =
        AvaloniaProperty.Register<ToolButton, bool>(nameof(IsToggle));

    /// <summary>Defines the <see cref="IsChecked"/> property.</summary>
    public static readonly StyledProperty<bool> IsCheckedProperty =
        AvaloniaProperty.Register<ToolButton, bool>(nameof(IsChecked), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    static ToolButton()
    {
        ContentProperty.Changed.AddClassHandler<ToolButton>((b, _) => b.UpdatePseudoClasses());
        IsCheckedProperty.Changed.AddClassHandler<ToolButton>((b, _) => b.UpdatePseudoClasses());
    }

    /// <summary>True to make the button toggle <see cref="IsChecked"/> on click.</summary>
    public bool IsToggle
    {
        get => GetValue(IsToggleProperty);
        set => SetValue(IsToggleProperty, value);
    }

    /// <summary>True while a toggle tool button is down (Enyo <c>depressed</c>).</summary>
    public bool IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnClick()
    {
        if (IsToggle)
        {
            SetCurrentValue(IsCheckedProperty, !IsChecked);
        }

        base.OnClick();
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":captioned", Content is not null);
        PseudoClasses.Set(":checked", IsChecked);
    }
}
