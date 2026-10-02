using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;

namespace AvaEnyo.Controls.Tests;

public static partial class TestHost
{
    public static readonly string[] Variants = { "Light", "Dark" };
    public static readonly EnyoDensity[] Densities = { EnyoDensity.Default, EnyoDensity.Compact };

    public static ThemeVariant ToVariant(string name) => name == "Dark" ? ThemeVariant.Dark : ThemeVariant.Light;

    public static Window Show(Control control, double width = 600, double height = 400, string variant = "Light", EnyoDensity density = EnyoDensity.Default)
    {
        TestApplication.Instance.Theme.Density = density;
        var window = new Window
        {
            RequestedThemeVariant = ToVariant(variant),
            Content = control,
            Width = width,
            Height = height,
        };
        window.Show();
        Settle(window);
        return window;
    }

    /// <summary>Runs layout and the dispatcher until both are quiet.</summary>
    public static void Settle(Window window)
    {
        for (var i = 0; i < 4; i++)
        {
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
        }
    }

    /// <summary>Runs the dispatcher loop (timers included) for <paramref name="ms"/> or until <paramref name="until"/>.</summary>
    public static void Pump(int ms, Func<bool>? until = null)
    {
        var start = Environment.TickCount64;
        while (Environment.TickCount64 - start < ms && until?.Invoke() != true)
        {
            using var cts = new System.Threading.CancellationTokenSource(5);
            Dispatcher.UIThread.MainLoop(cts.Token);
        }
    }

    /// <summary>Renders the window's next frame.</summary>
    public static WriteableBitmap Render(Window window)
    {
        Settle(window);
        return window.CaptureRenderedFrame() ?? throw new InvalidOperationException("No frame was rendered.");
    }

    /// <summary>The color of the pixel at <paramref name="p"/>, in window coordinates.</summary>
    public static Color PixelAt(WriteableBitmap frame, Point p)
    {
        var x = (int)Math.Floor(p.X * frame.Dpi.X / 96);
        var y = (int)Math.Floor(p.Y * frame.Dpi.Y / 96);
        using var buffer = frame.Lock();
        var bytes = new byte[4];
        System.Runtime.InteropServices.Marshal.Copy(buffer.Address + (y * buffer.RowBytes) + (x * 4), bytes, 0, 4);
        return buffer.Format == PixelFormat.Rgba8888
            ? Color.FromArgb(bytes[3], bytes[0], bytes[1], bytes[2])
            : Color.FromArgb(bytes[3], bytes[2], bytes[1], bytes[0]);
    }

    public static IReadOnlyList<string> KeysReferencedBy(string controlFileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "ThemeXaml", controlFileName + ".axaml");
        if (!File.Exists(path))
        {
            return Array.Empty<string>();
        }

        return ResourceRef().Matches(File.ReadAllText(path)).Select(m => m.Groups["key"].Value).Distinct().ToList();
    }

    [GeneratedRegex(@"\{DynamicResource\s+(?<key>[A-Za-z0-9_.]+)\s*\}")]
    private static partial Regex ResourceRef();
}
