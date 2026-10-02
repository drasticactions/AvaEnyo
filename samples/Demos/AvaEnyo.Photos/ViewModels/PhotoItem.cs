using System;
using Avalonia.Media.Imaging;
using AvaEnyo.Samples;

namespace AvaEnyo.Photos.ViewModels;

/// <summary>One photo of an album: an index into the placeholder photos.</summary>
public sealed class PhotoItem(int index, int position)
{
    public int Index { get; } = index;

    /// <summary>The place of the photo in its album.</summary>
    public int Position { get; } = position;

    public Bitmap Thumbnail => SampleAssets.Thumbnail(Index, 240);

    public Uri Uri => SampleAssets.PhotoUri(Index);
}
