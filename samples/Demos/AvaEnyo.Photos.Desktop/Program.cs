using System;
using Avalonia;
using AvaEnyo.Samples;

namespace AvaEnyo.Photos.Desktop;

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
            .UseSkia()
            .WithAvaEnyoFonts()
            .LogToTrace();
}
