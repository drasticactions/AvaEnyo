using System;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace AvaEnyo.Samples;

/// <summary>The generated placeholder photos and avatars (tools/gen-images.cs).</summary>
public static class SampleAssets
{
    public const int PhotoCount = 60;
    public const int AvatarCount = 24;

    private static readonly Dictionary<string, Bitmap> s_cache = new();

    public static Uri PhotoUri(int index) => new($"avares://AvaEnyo.Samples.Common/Assets/Photos/photo-{(index % PhotoCount) + 1:00}.jpg");

    public static Uri AvatarUri(int index) => new($"avares://AvaEnyo.Samples.Common/Assets/Avatars/avatar-{(index % AvatarCount) + 1:00}.png");

    /// <summary>The photo at <paramref name="index"/> (wraps around), cached.</summary>
    public static Bitmap Photo(int index) => Load(PhotoUri(index));

    /// <summary>A thumbnail of the photo, decoded at <paramref name="width"/> px wide.</summary>
    public static Bitmap Thumbnail(int index, int width = 240)
    {
        var uri = PhotoUri(index);
        var key = uri + "#" + width;
        if (!s_cache.TryGetValue(key, out var bitmap))
        {
            using var stream = AssetLoader.Open(uri);
            bitmap = Bitmap.DecodeToWidth(stream, width);
            s_cache[key] = bitmap;
        }

        return bitmap;
    }

    /// <summary>The avatar at <paramref name="index"/> (wraps around), cached.</summary>
    public static Bitmap Avatar(int index) => Load(AvatarUri(index));

    private static Bitmap Load(Uri uri)
    {
        var key = uri.ToString();
        if (!s_cache.TryGetValue(key, out var bitmap))
        {
            using var stream = AssetLoader.Open(uri);
            bitmap = new Bitmap(stream);
            s_cache[key] = bitmap;
        }

        return bitmap;
    }
}
