using System.Collections.Generic;
using System.Linq;

namespace AvaEnyo.Gallery;

/// <summary>
/// The registry of sample pages, in index order. Each group adds its pages in its own file
/// (<c>GalleryPages.&lt;Group&gt;.cs</c>).
/// </summary>
public static partial class GalleryPages
{
    /// <summary>The index sections, in order (StyleMatters' groups first).</summary>
    public static readonly string[] Groups =
    [
        "View Basics", "Dividers", "Page Containers", "Popups", "Widgets", "Lists", "Media", "Forms", "Stock Controls",
        "Theme", "Motion", "All Controls",
    ];

    private static List<GalleryPage>? s_all;

    /// <summary>Every page, grouped in <see cref="Groups"/> order.</summary>
    public static IReadOnlyList<GalleryPage> All
    {
        get
        {
            if (s_all is null)
            {
                var list = new List<GalleryPage>();
                AddViewBasics(list);
                AddDividers(list);
                AddPageContainers(list);
                AddPopups(list);
                AddWidgets(list);
                AddLists(list);
                AddMedia(list);
                AddForms(list);
                AddStockControls(list);
                AddTheme(list);
                AddMotion(list);
                AddAllControls(list);
                s_all = list.OrderBy(p => System.Array.IndexOf(Groups, p.Group)).ToList();
            }

            return s_all;
        }
    }

    /// <summary>The page named <paramref name="name"/> (case-insensitive, spaces ignored), or null.</summary>
    public static GalleryPage? Find(string name)
    {
        var key = Normalize(name);
        return All.FirstOrDefault(p => Normalize(p.Name) == key);
    }

    private static string Normalize(string s) => s.Replace(" ", string.Empty).Replace("&", string.Empty).ToLowerInvariant();

    static partial void AddViewBasics(List<GalleryPage> pages);

    static partial void AddDividers(List<GalleryPage> pages);

    static partial void AddPageContainers(List<GalleryPage> pages);

    static partial void AddPopups(List<GalleryPage> pages);

    static partial void AddWidgets(List<GalleryPage> pages);

    static partial void AddLists(List<GalleryPage> pages);

    static partial void AddMedia(List<GalleryPage> pages);

    static partial void AddForms(List<GalleryPage> pages);

    static partial void AddStockControls(List<GalleryPage> pages);

    static partial void AddTheme(List<GalleryPage> pages);

    static partial void AddMotion(List<GalleryPage> pages);

    static partial void AddAllControls(List<GalleryPage> pages);
}
