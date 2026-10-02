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

// Renders the license-free placeholder photos and avatars of the AvaEnyo samples. The output is deterministic: the
// same seed gives the same images. Run from the repository root:
//
//   dotnet run tools/gen-images.cs [OUT_DIR]
//
// OUT_DIR defaults to samples/AvaEnyo.Samples.Common/Assets.

using SkiaSharp;

var outDir = args.Length > 0 ? args[0] : Path.Combine("samples", "AvaEnyo.Samples.Common", "Assets");
var photos = Path.Combine(outDir, "Photos");
var avatars = Path.Combine(outDir, "Avatars");
Directory.CreateDirectory(photos);
Directory.CreateDirectory(avatars);

// Landscape-like palettes: sky, horizon, ground.
var palettes = new (SKColor Sky, SKColor Mid, SKColor Ground, SKColor Accent)[]
{
    (new(0x2b, 0x5f, 0x9e), new(0xf2, 0xb5, 0x6b), new(0x3d, 0x55, 0x2c), new(0xff, 0xe2, 0x9a)),
    (new(0x15, 0x2a, 0x4a), new(0x6c, 0x4a, 0x8e), new(0x1c, 0x1c, 0x2e), new(0xf5, 0xd0, 0x6b)),
    (new(0x87, 0xc5, 0xea), new(0xd8, 0xee, 0xf7), new(0x5a, 0x8f, 0x4a), new(0xff, 0xff, 0xff)),
    (new(0xe8, 0x7a, 0x4c), new(0xf7, 0xc4, 0x8b), new(0x6b, 0x3a, 0x2a), new(0xff, 0xf0, 0xc8)),
    (new(0x40, 0x8a, 0x9c), new(0x9b, 0xd1, 0xc8), new(0x2e, 0x5e, 0x6b), new(0xf0, 0xf8, 0xf4)),
    (new(0x5b, 0x6e, 0x7d), new(0xa7, 0xb3, 0xbc), new(0x33, 0x3d, 0x45), new(0xe6, 0xe9, 0xec)),
};

for (var i = 0; i < 60; i++)
{
    var rng = new Random(1000 + i);
    var (sky, mid, ground, accent) = palettes[i % palettes.Length];
    var w = 1024;
    var h = i % 7 == 3 ? 1024 : 768;
    using var surface = SKSurface.Create(new SKImageInfo(w, h));
    var c = surface.Canvas;
    c.Clear(mid);
    var horizon = (float)(h * (0.45 + (rng.NextDouble() * 0.2)));
    using (var skyPaint = new SKPaint { Shader = SKShader.CreateLinearGradient(new SKPoint(0, 0), new SKPoint(0, horizon), [sky, mid], SKShaderTileMode.Clamp) })
    {
        c.DrawRect(0, 0, w, horizon, skyPaint);
    }

    // A sun or moon.
    using (var sun = new SKPaint { Color = accent.WithAlpha(230), IsAntialias = true })
    {
        var r = 30 + rng.Next(60);
        c.DrawCircle(rng.Next(w), (float)(horizon * (0.25 + (rng.NextDouble() * 0.5))), r, sun);
    }

    // Two or three ranges of hills.
    for (var layer = 0; layer < 3; layer++)
    {
        var shade = Lerp(mid, ground, (layer + 1) / 3f);
        using var path = new SKPath();
        var baseY = horizon + (layer * h * 0.08f);
        path.MoveTo(0, h);
        path.LineTo(0, baseY);
        var x = 0f;
        while (x < w)
        {
            var step = 80 + rng.Next(160);
            var peak = baseY - (20 + rng.Next(120)) + (layer * 10);
            path.QuadTo(x + (step / 2f), peak, x + step, baseY + rng.Next(30) - 15);
            x += step;
        }

        path.LineTo(w, h);
        path.Close();
        using var paint = new SKPaint { Color = shade, IsAntialias = true };
        c.DrawPath(path, paint);
    }

    // Foreground band.
    using (var fg = new SKPaint { Shader = SKShader.CreateLinearGradient(new SKPoint(0, h * 0.8f), new SKPoint(0, h), [ground, Darken(ground, 0.5f)], SKShaderTileMode.Clamp) })
    {
        c.DrawRect(0, h * 0.8f, w, h * 0.2f, fg);
    }

    using var image = surface.Snapshot();
    using var data = image.Encode(SKEncodedImageFormat.Jpeg, 82);
    File.WriteAllBytes(Path.Combine(photos, $"photo-{i + 1:00}.jpg"), data.ToArray());
}

var names = new[]
{
    "Ada Lovelace", "Brian Kernighan", "Carol Shaw", "Dennis Ritchie", "Edsger Dijkstra", "Frances Allen",
    "Grace Hopper", "Hedy Lamarr", "Ivan Sutherland", "Jean Bartik", "Ken Thompson", "Lynn Conway",
    "Margaret Hamilton", "Niklaus Wirth", "Olivia Chen", "Paul Graham", "Radia Perlman", "Sophie Wilson",
    "Tim Berners-Lee", "Ursula Martin", "Vint Cerf", "Wendy Hall", "Xavier Leroy", "Yukihiro Matsumoto",
};
var colors = new SKColor[] { new(0x20, 0x71, 0xbb), new(0x2a, 0xa1, 0x00), new(0xbe, 0x00, 0x03), new(0x6c, 0x4a, 0x8e), new(0xe5, 0xb9, 0x00), new(0x40, 0x8a, 0x9c), new(0x4b, 0x4b, 0x4b), new(0xe8, 0x7a, 0x4c) };
using var typeface = SKTypeface.FromFamilyName("Source Sans 3", SKFontStyle.Bold) ?? SKTypeface.Default;
for (var i = 0; i < names.Length; i++)
{
    const int size = 96;
    using var surface = SKSurface.Create(new SKImageInfo(size, size));
    var c = surface.Canvas;
    c.Clear(SKColors.Transparent);
    using var paint = new SKPaint { Color = colors[i % colors.Length], IsAntialias = true };
    c.DrawCircle(size / 2f, size / 2f, size / 2f, paint);
    var initials = string.Concat(names[i].Split(' ').Select(p => p[0]));
    using var font = new SKFont(typeface, 40);
    using var text = new SKPaint { Color = SKColors.White, IsAntialias = true };
    var width = font.MeasureText(initials, text);
    var metrics = font.Metrics;
    c.DrawText(initials, (size - width) / 2, (size / 2f) - ((metrics.Ascent + metrics.Descent) / 2), font, text);
    using var image = surface.Snapshot();
    using var data = image.Encode(SKEncodedImageFormat.Png, 100);
    File.WriteAllBytes(Path.Combine(avatars, $"avatar-{i + 1:00}.png"), data.ToArray());
}

Console.WriteLine($"wrote 60 photos to {photos} and {names.Length} avatars to {avatars}");
return 0;

static SKColor Lerp(SKColor a, SKColor b, float t) => new(
    (byte)(a.Red + ((b.Red - a.Red) * t)),
    (byte)(a.Green + ((b.Green - a.Green) * t)),
    (byte)(a.Blue + ((b.Blue - a.Blue) * t)));

static SKColor Darken(SKColor c, float f) => new((byte)(c.Red * f), (byte)(c.Green * f), (byte)(c.Blue * f));
