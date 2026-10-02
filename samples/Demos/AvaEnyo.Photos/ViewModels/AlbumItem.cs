using Avalonia.Media.Imaging;
using AvaEnyo.Samples;

namespace AvaEnyo.Photos.ViewModels;

/// <summary>An album row: cover, name and photo count.</summary>
public sealed class AlbumItem(PhotoAlbum album)
{
    public PhotoAlbum Album { get; } = album;

    public string Name => Album.Name;

    public string CountText => Album.Count == 1 ? "1 photo" : $"{Album.Count} photos";

    public Bitmap Cover => SampleAssets.Thumbnail(Album.CoverIndex, 160);
}
