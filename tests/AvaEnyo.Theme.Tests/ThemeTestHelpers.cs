using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;

namespace AvaEnyo.Theme.Tests;

public static partial class ThemeTestHelpers
{
    public static readonly string[] Variants = { "Light", "Dark" };
    public static readonly EnyoDensity[] Densities = { EnyoDensity.Default, EnyoDensity.Compact };

    public static ThemeVariant ToVariant(string name) => name == "Dark" ? ThemeVariant.Dark : ThemeVariant.Light;

    /// <summary>Hosts <paramref name="control"/> in a shown window under the given variant and density and lays it out.</summary>
    public static Window Host(Control control, string variant, EnyoDensity density, double width = 600, double height = 400)
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
        window.UpdateLayout();
        return window;
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

    /// <summary>All keys referenced with DynamicResource or StaticResource in a theme XAML file.</summary>
    public static IReadOnlyList<string> KeysReferencedBy(string xamlDirectory, string controlFileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, xamlDirectory, controlFileName + ".axaml");
        if (!File.Exists(path))
        {
            return Array.Empty<string>();
        }

        var text = File.ReadAllText(path);
        return ResourceRef().Matches(text)
            .Select(m => m.Groups["key"].Value)
            .Distinct()
            .OrderBy(k => k, StringComparer.Ordinal)
            .ToList();
    }

    [GeneratedRegex(@"\{DynamicResource\s+(?<key>[A-Za-z0-9_.]+)\s*\}")]
    private static partial Regex ResourceRef();
}
