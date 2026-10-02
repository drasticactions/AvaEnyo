using System.Collections.Generic;
using AvaEnyo.Gallery.Pages.AllControls;

namespace AvaEnyo.Gallery;

public static partial class GalleryPages
{
    static partial void AddAllControls(List<GalleryPage> pages)
    {
        pages.Add(new("All Controls", "All Controls", "The Onyx theme sample of Enyo 1.0, box by box", () => new AllControlsPage(), "src/AllControls/AllControlsPage.axaml", Scrolls: false,
            Notes: "A rebuild of support/examples/Theming/onyx: the same 21 captioned boxes in the same order, for review next to the real page (tools/enyo-reference/compare.sh AllControls)."));
    }
}
