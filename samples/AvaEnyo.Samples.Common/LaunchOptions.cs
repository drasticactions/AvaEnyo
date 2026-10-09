using System;
using System.Collections.Generic;
using System.Globalization;

namespace AvaEnyo.Samples;

/// <summary>
/// The command-line switches of every Desktop sample head: <c>--variant Light|Dark --density Default|Compact
/// --size WxH --screenshot out.png --disable-animations --time-scale N --delay ms --native-decorations</c>, plus <c>--page Name</c> (gallery) and
/// <c>--view name</c> (demos). With <c>--screenshot</c> the head renders one frame to a PNG and exits.
/// </summary>
public sealed class LaunchOptions
{
    public static LaunchOptions Current { get; private set; } = new();

    public string? Variant { get; init; }

    public EnyoDensity Density { get; init; } = EnyoDensity.Default;

    public double Width { get; init; } = 1024;

    public double Height { get; init; } = 768;

    public string? Screenshot { get; init; }

    public bool DisableAnimations { get; init; }

    /// <summary>Animations run this many times slower; null leaves the default.</summary>
    public double? TimeScale { get; init; }

    public int DelayMs { get; init; } = 800;

    public string? Page { get; init; }

    public string? View { get; init; }

    /// <summary>True keeps the system title bar instead of the Enyo decorations.</summary>
    public bool NativeDecorations { get; init; }

    /// <summary>True when the window draws the Enyo decorations: not with <c>--native-decorations</c> or <c>--screenshot</c>.</summary>
    public bool UseEnyoDecorations => !NativeDecorations && Screenshot is null;

    public static LaunchOptions Parse(IReadOnlyList<string> args)
    {
        string? variant = null, shot = null, page = null, view = null;
        var density = EnyoDensity.Default;
        double w = 1024, h = 768;
        var delay = 800;
        var disable = false;
        var native = false;
        double? timeScale = null;
        for (var i = 0; i < args.Count; i++)
        {
            var hasValue = i + 1 < args.Count;
            switch (args[i])
            {
                case "--variant" when hasValue: variant = args[++i]; break;
                case "--density" when hasValue: density = Enum.Parse<EnyoDensity>(args[++i], ignoreCase: true); break;
                case "--size" when hasValue:
                    var parts = args[++i].Split('x');
                    w = double.Parse(parts[0], CultureInfo.InvariantCulture);
                    h = double.Parse(parts[1], CultureInfo.InvariantCulture);
                    break;
                case "--screenshot" when hasValue: shot = args[++i]; break;
                case "--disable-animations": disable = true; break;
                case "--time-scale" when hasValue: timeScale = double.Parse(args[++i], CultureInfo.InvariantCulture); break;
                case "--delay" when hasValue: delay = int.Parse(args[++i], CultureInfo.InvariantCulture); break;
                case "--page" when hasValue: page = args[++i]; break;
                case "--view" when hasValue: view = args[++i]; break;
                case "--native-decorations": native = true; break;
            }
        }

        Current = new LaunchOptions { Variant = variant, Density = density, Width = w, Height = h, Screenshot = shot, DisableAnimations = disable, TimeScale = timeScale, DelayMs = delay, Page = page, View = view, NativeDecorations = native };
        return Current;
    }
}
