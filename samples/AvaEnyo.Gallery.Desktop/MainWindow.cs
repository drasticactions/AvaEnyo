using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using AvaEnyo.Animations;
using AvaEnyo.Samples;

namespace AvaEnyo.Gallery.Desktop;

/// <summary>Hosts <see cref="MainView"/> and applies the command-line switches, including <c>--screenshot</c>.</summary>
public sealed class MainWindow : Window
{
    public MainWindow()
    {
        var o = LaunchOptions.Current;
        Title = "AvaEnyo Gallery";
        Width = o.Width;
        Height = o.Height;
        if (o.Variant is { } v)
        {
            Application.Current!.RequestedThemeVariant = v.Equals("Dark", System.StringComparison.OrdinalIgnoreCase) ? ThemeVariant.Dark : ThemeVariant.Light;
        }

        App.Theme.Density = o.Density;
        if (o.TimeScale is { } scale)
        {
            EnyoAnimations.TimeScale = scale;
        }

        if (o.DisableAnimations)
        {
            EnyoAnimations.IsEnabled = false;
        }

        var view = new MainView();
        Content = view;
        ScreenshotHelper.Frame(this);
        if (o.Page is { } page)
        {
            view.TryNavigate(page);
        }

        if (o.Screenshot is { } path)
        {
            Opened += async (_, _) => await ScreenshotHelper.CaptureAndCloseAsync(this, path, o.DelayMs);
        }
    }
}
