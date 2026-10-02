using System;
using Avalonia;
using Avalonia.Controls;

namespace AvaEnyo.Controls;

/// <summary>
/// The Help item of an <see cref="AppMenu"/> (<c>enyo.HelpMenu</c>). Clicking it opens <see cref="Target"/> with the
/// system launcher (Enyo opened the help app or the browser).
/// </summary>
public class HelpMenu : AppMenuItem
{
    /// <summary>Defines the <see cref="Target"/> property.</summary>
    public static readonly StyledProperty<Uri?> TargetProperty =
        AvaloniaProperty.Register<HelpMenu, Uri?>(nameof(Target));

    /// <summary>Initializes a new instance of the <see cref="HelpMenu"/> class.</summary>
    public HelpMenu()
    {
        Header = "Help";
    }

    /// <summary>The help page to open.</summary>
    public Uri? Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnClick()
    {
        base.OnClick();
        if (Target is { } target && TopLevel.GetTopLevel(this)?.Launcher is { } launcher)
        {
            _ = launcher.LaunchUriAsync(target);
        }
    }
}
