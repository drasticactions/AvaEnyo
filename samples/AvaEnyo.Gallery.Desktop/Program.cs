using System;
using Avalonia;
using AvaEnyo.Samples;

namespace AvaEnyo.Gallery.Desktop;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        if (Array.IndexOf(args, "--list-pages") >= 0)
        {
            // One page name per line, for review scripts.
            foreach (var page in GalleryPages.All)
            {
                Console.WriteLine(page.Name);
            }

            return;
        }

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
