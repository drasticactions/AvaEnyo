using System.Collections.Generic;
using AvaEnyo.Gallery.Pages.PageContainers;

namespace AvaEnyo.Gallery;

public static partial class GalleryPages
{
    static partial void AddPageContainers(List<GalleryPage> pages)
    {
        const string g = "Page Containers";
        pages.Add(new(g, "Groups", "Labeled and unlabeled groups, and a row group of inputs", () => new GroupsPage(), "src/PageContainers/GroupsPage.axaml",
            Notes: "A Group draws a 14 px embossed frame; a header adds the 36 px label band. Rows inside are rounded by position. A RowGroup wraps each child in a row, so text boxes get the first, middle and last focus frames. HideRow and ShowRow move the order classes."));
        pages.Add(new(g, "Drawers", "Drawers and divider drawers, animated or not", () => new DrawersPage(), "src/PageContainers/DrawersPage.axaml",
            Notes: "A Drawer opens over 250 ms and closes over 100 ms with a cubic ease-out, ending at auto height or collapsed. Tapping the header toggles it."));
        pages.Add(new(g, "Pane", "A view stack with fade, fly-in and history", () => new PanePage(), "src/PageContainers/PanePage.axaml", Scrolls: false,
            Notes: "A Pane shows one view at a time. Selecting plays the transition (300 ms cubic ease-out) and records the previous view, so Back returns to it (up to 40 entries). A lazy PaneView builds its content on first select; an unknown name raises CreateView."));
        pages.Add(new(g, "SlidingPane", "Views that slide over each other, with peek and dismiss", () => new SlidingPanePage(), "src/PageContainers/SlidingPanePage.axaml", Scrolls: false,
            Notes: "Selection slides 700 ms with a cubic ease-out. View 0 never moves. A drag pushes through the stack."));
        pages.Add(new(g, "SlidingPane Many", "Many sliding views: add, hide, show and the MultiView switch", () => new SlidingPaneManyPage(), "src/PageContainers/SlidingPaneManyPage.axaml", Scrolls: false,
            Notes: "From Enyo's Layouts/Sliding/many. The last showing view is fitted to the remaining width; a hidden view slides off the right edge. With MultiView off every view is full width."));
        pages.Add(new(g, "Carousel & VirtualCarousel", "Endless and bound carousels, and a snap scroller", () => new CarouselPage(), "src/PageContainers/CarouselPage.axaml",
            Notes: "A Carousel keeps three views and asks for the neighbors with GetLeft and GetRight; after each snap the views rotate so the shown one stays in the middle. Friction 0.75, spring 0.8. A VirtualCarousel reuses three views over a bound list and stops at its ends."));
        pages.Add(new(g, "Flex Layouts", "HFlexBox and VFlexBox: flex, pack and align", () => new FlexLayoutsPage(), "src/PageContainers/FlexLayoutsPage.axaml",
            Notes: "A flexed child's basis is 0: it gets exactly its share of the free space, whatever its content wants. Pack places unflexed children along the main axis, Align across it."));
    }
}
