using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using AvaEnyo.Animations;
using AvaEnyo.Samples;

namespace AvaEnyo.Photos.Desktop;

/// <summary>Hosts <see cref="MainView"/> and applies the command-line switches, including <c>--view</c> and <c>--screenshot</c>.</summary>
public sealed class MainWindow : Window
{
    public MainWindow()
    {
        var o = LaunchOptions.Current;
        Title = "Photos";
        Width = o.Width;
        Height = o.Height;
        EnyoWindow.SetAppName(this, "Photos");
        EnyoWindow.SetUseEnyoDecorations(this, o.UseEnyoDecorations);
        if (o.Variant is { } v)
        {
            Application.Current!.RequestedThemeVariant = v.Equals("Dark", StringComparison.OrdinalIgnoreCase) ? ThemeVariant.Dark : ThemeVariant.Light;
        }

        App.Theme.Density = o.Density;
        if (o.TimeScale is { } scale)
        {
            EnyoAnimations.TimeScale = scale;
        }

        if (o.DisableAnimations || o.Screenshot is not null)
        {
            // Screenshots show end states, not a frame in the middle of a slide.
            EnyoAnimations.IsEnabled = false;
        }

        var view = new MainView();
        Content = view;
        ScreenshotHelper.Frame(this);
        if (o.View is { } name)
        {
            Opened += (_, _) => view.ShowView(name);
        }

        if (o.Screenshot is { } path)
        {
            Opened += async (_, _) => await ScreenshotHelper.CaptureAndCloseAsync(this, path, o.DelayMs);
        }
    }
}
