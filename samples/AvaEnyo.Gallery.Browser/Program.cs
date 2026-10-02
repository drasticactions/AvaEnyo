using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using AvaEnyo;
using AvaEnyo.Gallery;

internal static class Program
{
    private static Task Main(string[] args) =>
        BuildAvaloniaApp().WithAvaEnyoFonts().StartBrowserAppAsync("out");

    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>();
}
