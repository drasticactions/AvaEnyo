using System.Collections.Generic;

namespace AvaEnyo.Samples;

/// <summary>A fake photo album: indexes into the placeholder photos.</summary>
public sealed record PhotoAlbum(string Name, IReadOnlyList<int> Photos)
{
    public int Count => Photos.Count;

    public int CoverIndex => Photos.Count > 0 ? Photos[0] : 0;
}
