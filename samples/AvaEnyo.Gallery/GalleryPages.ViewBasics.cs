using System.Collections.Generic;
using AvaEnyo.Gallery.Pages.ViewBasics;

namespace AvaEnyo.Gallery;

public static partial class GalleryPages
{
    static partial void AddViewBasics(List<GalleryPage> pages)
    {
        const string g = "View Basics";
        pages.Add(new(g, "Toolbars", "Dark, light and tinted toolbars with tool buttons", () => new ToolbarsPage(), "src/ViewBasics/ToolbarsPage.axaml",
            Notes: "A Toolbar centers its items on a flex row. A Spacer takes the free space. Background tints the band."));
        pages.Add(new(g, "Page Headers", "Header bands, dark headers and the page header", () => new PageHeadersPage(), "src/ViewBasics/PageHeadersPage.axaml"));
        pages.Add(new(g, "Scrims", "A dimming scrim over a region, and the transparent input blocker", () => new ScrimsPage(), "src/ViewBasics/ScrimsPage.axaml",
            Notes: "A scrim is black at opacity 0.65 and fades over 500 ms. The transparent class blocks input without drawing. Popups share one scrim below the topmost modal popup."));
    }
}
