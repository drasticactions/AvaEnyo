using System.Collections.Generic;
using AvaEnyo.Gallery.Pages.Theme;

namespace AvaEnyo.Gallery;

public static partial class GalleryPages
{
    static partial void AddTheme(List<GalleryPage> pages)
    {
        const string g = "Theme";
        pages.Add(new(g, "Colors & Tokens", "The palette and brushes, light and dark side by side", () => new ColorsPage(), "src/Theme/ColorsPage.axaml",
            Notes: "The light palette comes from the Onyx CSS and sprites. The dark variant is invented: the body goes dark, the chrome a step deeper, the color variants stay."));
        pages.Add(new(g, "Typography", "Every text class on Source Sans 3", () => new TypographyPage(), "src/Theme/TypographyPage.axaml"));
        pages.Add(new(g, "Density", "Default and Compact metrics", () => new DensityPage(), "src/Theme/DensityPage.axaml",
            Notes: "Default matches Enyo (20 px base). Compact is a hand-tuned overlay of the metric keys only."));
    }
}
