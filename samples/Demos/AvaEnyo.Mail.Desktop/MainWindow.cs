using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using AvaEnyo.Animations;
using AvaEnyo.Samples;

namespace AvaEnyo.Mail.Desktop;

/// <summary>Hosts <see cref="MainView"/> and applies the command-line switches, including <c>--view</c> and <c>--screenshot</c>.</summary>
public sealed class MainWindow : Window
{
    public MainWindow()
    {
        var o = LaunchOptions.Current;
        Title = "Mail";
        Width = o.Width;
        Height = o.Height;
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
            // Screenshots show end states, not frames of a slide.
            EnyoAnimations.IsEnabled = false;
        }

        var view = new MainView();
        Content = view;
        ScreenshotHelper.Frame(this);
        Opened += async (_, _) =>
        {
            if (o.View is { } name)
            {
                // The folders and messages load from the bus first.
                await Task.Delay(200);
                view.ShowView(name);
            }

            if (o.Screenshot is { } path)
            {
                await ScreenshotHelper.CaptureAndCloseAsync(this, path, o.DelayMs);
            }
        };
    }
}
