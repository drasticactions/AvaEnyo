using System;
using Avalonia;
using AvaEnyo.Samples;

namespace AvaEnyo.Mail.Desktop;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        LaunchOptions.Parse(args);
        App.DesktopWindowFactory = () => new MainWindow();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .UseWaylandWithFallback()
#pragma warning disable AVALONIA_WAYLAND_FORCE_CSD
            // Draw the Enyo decorations even where the compositor offers its own.
            .With(new WaylandPlatformOptions { ForceDrawnDecorations = LaunchOptions.Current.UseEnyoDecorations })
#pragma warning restore AVALONIA_WAYLAND_FORCE_CSD
            .UseSkia()
            .WithAvaEnyoFonts()
            .LogToTrace();
}
