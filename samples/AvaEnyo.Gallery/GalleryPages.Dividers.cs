using System.Collections.Generic;
using AvaEnyo.Gallery.Pages.Dividers;

namespace AvaEnyo.Gallery;

public static partial class GalleryPages
{
    static partial void AddDividers(List<GalleryPage> pages)
    {
        const string g = "Dividers";
        pages.Add(new(g, "Labeled", "Section captions with the 4 px blue rule", () => new LabeledPage(), "src/Dividers/LabeledPage.axaml",
            Notes: "A divider is a bold 12 pt blue caption, then a 4 px rule that fills the row. The caption truncates before the rule shrinks."));
        pages.Add(new(g, "Alphabetical", "Single-letter dividers for long lists", () => new AlphabeticalPage(), "src/Dividers/AlphabeticalPage.axaml",
            Notes: "An AlphaDivider is 30 px tall: the letter sits at the left and the rule starts 30 px in."));
        pages.Add(new(g, "Collapsible", "Divider drawers that open and close", () => new CollapsiblePage(), "src/Dividers/CollapsiblePage.axaml",
            Notes: "A DividerDrawer opens over 250 ms and closes over 100 ms with a cubic ease-out. The round knob turns with the state."));
        pages.Add(new(g, "Nested & Truncating", "Long captions, icons and drawers inside drawers", () => new NestedPage(), "src/Dividers/NestedPage.axaml"));
    }
}
