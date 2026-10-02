#!/usr/bin/env -S dotnet run
#:package SkiaSharp@3.119.4
#:package SkiaSharp.NativeAssets.Linux.NoDependencies@3.119.4
#:property ManagePackageVersionsCentrally=false
#:property TreatWarningsAsErrors=false
#:property PublishAot=false
#:property ImplicitUsings=enable
#:property SignAssembly=false
#:property IsAotCompatible=false
#:property EnableTrimAnalyzer=false

// Reads the Onyx sprite PNGs of Enyo 1.0 and reports the measured colors that the vector frames in
// AvaEnyo.Theme reproduce. A developer tool: it never runs in CI and its output is not committed.
//
//   dotnet run tools/sample-sprites.cs [ENYO_ROOT] [OUT_DIR]
//
// ENYO_ROOT defaults to the ENYO_ROOT environment variable, then /tmp.
// OUT_DIR defaults to artifacts/sprites. The report is sprites.md and sprites.json, plus contact-sheet.png
// (every sprite at 4x).

using System.Globalization;
using System.Text;
using System.Text.Json;
using SkiaSharp;

var root = args.Length > 0 ? args[0] : Environment.GetEnvironmentVariable("ENYO_ROOT") ?? "/tmp";
var outDir = args.Length > 1 ? args[1] : Path.Combine("artifacts", "sprites");
var images = Path.Combine(root, "framework", "source", "palm", "themes", "Onyx", "images");
if (!Directory.Exists(images))
{
    Console.Error.WriteLine($"No Onyx images at {images}. Pass ENYO_ROOT as the first argument.");
    return 1;
}

Directory.CreateDirectory(outDir);

// name, slice insets from the CSS (top, right, bottom, left), state row height (0 = one state).
var table = new (string Name, int[] Slice, int StateHeight)[]
{
    ("button-up", [4, 4, 4, 4], 0),
    ("button-down", [4, 4, 4, 4], 0),
    ("button", [4, 4, 4, 4], 0),
    ("button-toolbar", [4, 4, 4, 4], 0),
    ("toolbar", [2, 4, 2, 4], 0),
    ("toolbar-light", [2, 4, 2, 4], 0),
    ("header", [2, 0, 2, 0], 0),
    ("group-unlabeled", [14, 14, 14, 14], 0),
    ("group-labeled", [36, 14, 14, 14], 0),
    ("dialog-group-unlabeled", [14, 14, 14, 14], 0),
    ("dialog-group-labeled", [36, 14, 14, 14], 0),
    ("input-focus", [14, 14, 14, 14], 0),
    ("input-default", [14, 14, 14, 14], 0),
    ("input-default-focus", [14, 14, 14, 14], 0),
    ("input-first-focus", [14, 14, 2, 14], 0),
    ("input-middle-focus", [14, 14, 14, 14], 0),
    ("input-last-focus", [2, 14, 14, 14], 0),
    ("input-tool", [6, 6, 6, 6], 0),
    ("input-tool-focus", [6, 6, 6, 6], 0),
    ("roundedbox", [6, 20, 6, 20], 0),
    ("checkbox", [0, 0, 0, 0], 32),
    ("toggle-button", [0, 0, 0, 0], 32),
    ("radiobutton", [0, 16, 0, 16], 37),
    ("slider-handle", [0, 0, 0, 0], 40),
    ("slider-track", [0, 4, 0, 4], 0),
    ("slider-progress", [0, 4, 0, 4], 0),
    ("slider-progress-secondary", [0, 4, 0, 4], 0),
    ("progress-bar", [0, 4, 0, 4], 0),
    ("progress-bar-inner", [0, 4, 0, 4], 0),
    ("progress-item", [12, 0, 12, 0], 0),
    ("progress-item-inner", [0, 8, 0, 0], 0),
    ("progress-button", [4, 4, 4, 4], 0),
    ("progress-button-cancel", [0, 0, 0, 0], 32),
    ("progress-pill", [0, 0, 0, 0], 0),
    ("popup", [24, 24, 24, 24], 0),
    ("popupselect-background", [23, 23, 23, 23], 0),
    ("menu-background", [20, 20, 20, 20], 0),
    ("dialog", [42, 24, 24, 24], 0),
    ("toaster-dialog", [32, 24, 32, 24], 0),
    ("appmenu", [12, 24, 20, 24], 0),
    ("appmenu-divider", [0, 0, 2, 0], 0),
    ("appmenu-highlight", [0, 0, 0, 0], 0),
    ("confirmprompt-background", [20, 0, 20, 0], 0),
    ("picker-pill", [10, 10, 10, 10], 0),
    ("picker-pill-downstate", [10, 10, 10, 10], 0),
    ("picker-pill-focus", [10, 10, 10, 10], 0),
    ("picker-pill-focus-highlight", [10, 10, 10, 10], 0),
    ("picker-pill-text-input", [10, 10, 10, 10], 0),
    ("item-highlight", [0, 0, 0, 0], 0),
    ("divider", [0, 0, 0, 0], 0),
    ("divider-arrow", [0, 0, 0, 0], 40),
    ("menuitem-arrow", [0, 0, 0, 0], 24),
    ("menu-arrow", [0, 0, 0, 0], 0),
    ("menu-fade-arrow-up", [0, 0, 0, 0], 0),
    ("menu-fade-arrow-down", [0, 0, 0, 0], 0),
    ("sliding-shadow", [0, 0, 0, 0], 0),
    ("hash", [0, 0, 0, 0], 0),
    ("grabbutton", [0, 0, 0, 0], 0),
    ("checkmark", [0, 0, 0, 0], 0),
    ("search-input-search", [0, 0, 0, 0], 0),
    ("search-input-cancel", [0, 0, 0, 0], 0),
    ("spinner", [0, 0, 0, 0], 0),
    ("spinner-large", [0, 0, 0, 0], 0),
    ("filter-highlight", [0, 0, 0, 0], 0),
};

var md = new StringBuilder();
md.AppendLine("# Onyx sprite report").AppendLine();
md.AppendLine($"Source: `{images}`").AppendLine();
var json = new List<object>();
var sheet = new List<(string Name, SKBitmap Bitmap)>();

foreach (var (name, slice, stateHeight) in table)
{
    var path = Path.Combine(images, name + ".png");
    if (!File.Exists(path))
    {
        md.AppendLine($"## {name}\n\nmissing\n");
        continue;
    }

    using var data = SKData.Create(path);
    var bmp = SKBitmap.Decode(data);
    sheet.Add((name, bmp));
    var states = stateHeight > 0 ? Math.Max(1, bmp.Height / stateHeight) : 1;
    var h = stateHeight > 0 ? stateHeight : bmp.Height;
    md.AppendLine($"## {name}").AppendLine();
    md.AppendLine($"{bmp.Width}×{bmp.Height}, slice {string.Join(" ", slice)}, {states} state(s)").AppendLine();
    var stateList = new List<object>();
    for (var s = 0; s < states; s++)
    {
        var y0 = s * h;
        var cx = bmp.Width / 2;
        var cy = y0 + (h / 2);
        var column = Enumerable.Range(y0, Math.Min(h, bmp.Height - y0)).Select(y => Hex(bmp.GetPixel(cx, y))).ToArray();
        var row = Enumerable.Range(0, bmp.Width).Select(x => Hex(bmp.GetPixel(x, cy))).ToArray();
        var center = Hex(bmp.GetPixel(Math.Clamp(slice[3] + 1, 0, bmp.Width - 1) + Math.Max(0, (bmp.Width - slice[1] - slice[3]) / 2 - 1), Math.Clamp(cy, 0, bmp.Height - 1)));
        var radius = CornerRadius(bmp, y0, h);
        if (states > 1)
        {
            md.AppendLine($"### state {s} (rows {y0}..{y0 + h - 1})").AppendLine();
        }

        md.AppendLine($"- center fill: `{center}`");
        md.AppendLine($"- corner radius (alpha < 0x40 run on the top-left diagonal): {radius}");
        md.AppendLine($"- center column (top → bottom): {string.Join(" ", column.Select(c => $"`{c}`"))}");
        md.AppendLine($"- center row (left → right): {string.Join(" ", row.Select(c => $"`{c}`"))}");
        md.AppendLine();
        stateList.Add(new { state = s, center, radius, column, row });
    }

    json.Add(new { name, width = bmp.Width, height = bmp.Height, slice, states = stateList });
}

File.WriteAllText(Path.Combine(outDir, "sprites.md"), md.ToString());
File.WriteAllText(Path.Combine(outDir, "sprites.json"), JsonSerializer.Serialize(json, new JsonSerializerOptions { WriteIndented = true }));
WriteContactSheet(Path.Combine(outDir, "contact-sheet.png"), sheet);
Console.WriteLine($"wrote {outDir}/sprites.md, sprites.json, contact-sheet.png ({sheet.Count} sprites)");
return 0;

static string Hex(SKColor c) => c.Alpha == 255
    ? $"#{c.Red:x2}{c.Green:x2}{c.Blue:x2}"
    : $"#{c.Red:x2}{c.Green:x2}{c.Blue:x2}@{(c.Alpha / 255.0).ToString("0.00", CultureInfo.InvariantCulture)}";

static int CornerRadius(SKBitmap bmp, int y0, int h)
{
    var n = Math.Min(bmp.Width, h) / 2;
    var r = 0;
    for (var i = 0; i < n; i++)
    {
        if (bmp.GetPixel(i, y0 + i).Alpha < 0x40)
        {
            r++;
        }
        else
        {
            break;
        }
    }

    // A diagonal run of r transparent pixels corresponds to a radius of about r / (1 − 1/√2).
    return (int)Math.Round(r / (1 - (1 / Math.Sqrt(2))));
}

static void WriteContactSheet(string path, List<(string Name, SKBitmap Bitmap)> sheet)
{
    const int scale = 4;
    const int pad = 16;
    const int label = 18;
    var width = 1600;
    int x = pad, y = pad, rowH = 0;
    var placements = new List<(string Name, SKBitmap Bitmap, int X, int Y)>();
    foreach (var (name, bmp) in sheet)
    {
        var w = Math.Min(bmp.Width * scale, width - (2 * pad));
        var hh = bmp.Height * scale * w / (bmp.Width * scale);
        if (x + w > width - pad)
        {
            x = pad;
            y += rowH + pad + label;
            rowH = 0;
        }

        placements.Add((name, bmp, x, y + label));
        x += w + pad;
        rowH = Math.Max(rowH, Math.Min(hh, 1200));
    }

    var height = y + rowH + pad + label;
    using var surface = SKSurface.Create(new SKImageInfo(width, height));
    var canvas = surface.Canvas;
    canvas.Clear(new SKColor(0xd8, 0xd8, 0xd8));
    using var font = new SKFont(SKTypeface.Default, 12);
    using var paint = new SKPaint { Color = SKColors.Black, IsAntialias = true };
    foreach (var (name, bmp, px, py) in placements)
    {
        var w = Math.Min(bmp.Width * scale, width - (2 * pad));
        var hh = Math.Min(bmp.Height * scale * w / (bmp.Width * scale), 1200);
        canvas.DrawText(name, px, py - 4, font, paint);
        using var image = SKImage.FromBitmap(bmp);
        canvas.DrawImage(image, new SKRect(px, py, px + w, py + hh), new SKSamplingOptions(SKFilterMode.Nearest));
    }

    using var snapshot = surface.Snapshot();
    using var png = snapshot.Encode(SKEncodedImageFormat.Png, 100);
    File.WriteAllBytes(path, png.ToArray());
}
