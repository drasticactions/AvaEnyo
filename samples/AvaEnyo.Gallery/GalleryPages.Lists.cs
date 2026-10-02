using System.Collections.Generic;
using AvaEnyo.Gallery.Pages.Lists;

namespace AvaEnyo.Gallery;

public static partial class GalleryPages
{
    static partial void AddLists(List<GalleryPage> pages)
    {
        const string g = "Lists";
        pages.Add(new(g, "Virtual List", "500 rows with labeled or alphabetical dividers", () => new VirtualListPage(), "src/Lists/VirtualListPage.axaml", Scrolls: false,
            Notes: "A VirtualList realizes only the visible rows. A row whose divider key differs from the row above shows a divider in its own template, as Enyo apps did in onSetupRow, so dividers survive recycling."));
        pages.Add(new(g, "Swipeable Items", "Swipe to delete, with the inline confirm prompt", () => new SwipeableItemsPage(), "src/Lists/SwipeableItemsPage.axaml", Scrolls: false,
            Notes: "A horizontal drag moves the row 1:1. Past 35 % of the width it raises Swiped and shows Cancel / Delete. Only one row confirms at a time; vertical drags still scroll the list."));
        pages.Add(new(g, "Reorder", "Hold a row, then drag it to a new place", () => new ReorderPage(), "src/Lists/ReorderPage.axaml", Scrolls: false,
            Notes: "An AvaEnyo extension: Enyo had no list reordering. A 200 ms hold lifts the row; neighbors slide away; Reordering can cancel the move."));
        pages.Add(new(g, "Paged List", "1,000 rows loaded ten at a time", () => new PagedListPage(), "src/Lists/PagedListPage.axaml", Scrolls: false,
            Notes: "With an IPagedItemsSource the list asks for pages around the visible rows: page size 10, look-ahead 2 pages, as Enyo's Buffer did, and discards pages that scroll far away."));
        pages.Add(new(g, "Selectable Grid", "A virtualized photo grid with toggle selection", () => new SelectableGridPage(), "src/Lists/SelectableGridPage.axaml", Scrolls: false,
            Notes: "Layout=Grid uses a virtualizing uniform grid. Toggle selection is the List/SelectableVirtualGrid sample of Enyo."));
    }
}
