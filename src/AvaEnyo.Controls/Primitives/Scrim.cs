using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using AvaEnyo.Animations;

namespace AvaEnyo.Controls.Primitives;

/// <summary>
/// The Enyo scrim (<c>enyo.Scrim</c>): black at opacity 0.65 that fades in and out over 500 ms. Popups share one
/// scrim below the topmost modal popup; an app can also show one over a region with <see cref="IsOpen"/>. The
/// <c>transparent</c> class gives the invisible scrim that only blocks input.
/// </summary>
public class Scrim : TemplatedControl
{
    /// <summary>Defines the <see cref="IsOpen"/> property.</summary>
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<Scrim, bool>(nameof(IsOpen));

    static Scrim()
    {
        IsOpenProperty.Changed.AddClassHandler<Scrim>((s, e) => s.OnIsOpenChanged(e.GetNewValue<bool>()));
        IsVisibleProperty.OverrideDefaultValue<Scrim>(false);
    }

    /// <summary>True to show the scrim.</summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    private void OnIsOpenChanged(bool open)
    {
        if (Classes.Contains("transparent"))
        {
            IsVisible = open;
            return;
        }

        if (open)
        {
            _ = EnyoAnimations.ScrimShow(this);
        }
        else
        {
            _ = EnyoAnimations.ScrimHide(this);
        }
    }
}
