using System.Collections.Generic;
using AvaEnyo.Gallery.Pages.StockControls;

namespace AvaEnyo.Gallery;

public static partial class GalleryPages
{
    static partial void AddStockControls(List<GalleryPage> pages)
    {
        const string g = "Stock Controls";
        pages.Add(new(g, "Calendar", "Calendar and CalendarDatePicker in the item and popup language", () => new CalendarPage(), "src/StockControls/CalendarPage.axaml", Scrolls: true, Notes: "Invented look: Enyo had no month calendar. Day buttons use the held gradient; the selected day is the blue segment."));
        pages.Add(new(g, "Numeric Inputs", "NumericUpDown and ButtonSpinner", () => new NumericPage(), "src/StockControls/NumericPage.axaml", Scrolls: true, Notes: null));
        pages.Add(new(g, "AutoCompleteBox", "Suggestions in an Onyx popup list", () => new AutoCompletePage(), "src/StockControls/AutoCompletePage.axaml", Scrolls: true, Notes: null));
        pages.Add(new(g, "TreeView", "Nested rows with chevrons", () => new TreeViewPage(), "src/StockControls/TreeViewPage.axaml", Scrolls: true, Notes: null));
        pages.Add(new(g, "Tabs", "TabControl and TabStrip as Onyx tab groups", () => new TabsPage(), "src/StockControls/TabsPage.axaml", Scrolls: true, Notes: "Stock tabs take the enyo-tabbutton look: 18 px text and the segmented frame."));
        pages.Add(new(g, "Expander & GroupBox", "Expanders as divider drawers; GroupBox as an Onyx group", () => new ExpanderPage(), "src/StockControls/ExpanderPage.axaml", Scrolls: true, Notes: "An Expander opens in 250 ms and closes in 100 ms with a cubic ease-out, like Enyo's Drawer."));
        pages.Add(new(g, "ListBox", "ListBox items with the Enyo item look", () => new ListBoxPage(), "src/StockControls/ListBoxPage.axaml", Scrolls: true, Notes: null));
        pages.Add(new(g, "Menus & Flyouts", "Menu, ContextMenu, flyouts and tool tips", () => new MenusPage(), "src/StockControls/MenusPage.axaml", Scrolls: true, Notes: "Stock menus use the menu-background.png frame; the tool tip is an invented small popup frame."));
        pages.Add(new(g, "SplitView", "A pane on the body color with the sliding shadow", () => new SplitViewPage(), "src/StockControls/SplitViewPage.axaml", Scrolls: false, Notes: null));
        pages.Add(new(g, "Page Framework", "NavigationPage, TabbedPage, DrawerPage and CarouselPage", () => new PagesPage(), "src/StockControls/PagesPage.axaml", Scrolls: false, Notes: "NavigationPage pushes with the Enyo pane fly-in; its bar is the Onyx header with a back button. Escape or back pops a page."));
        pages.Add(new(g, "CommandBar", "The command bar as an Onyx toolbar", () => new CommandBarPage(), "src/StockControls/CommandBarPage.axaml", Scrolls: true, Notes: null));
        pages.Add(new(g, "PipsPager", "Small round pips with chevrons", () => new PipsPagerPage(), "src/StockControls/PipsPagerPage.axaml", Scrolls: true, Notes: null));
        pages.Add(new(g, "RefreshContainer", "Pull to refresh", () => new RefreshPage(), "src/StockControls/RefreshPage.axaml", Scrolls: false, Notes: null));
        pages.Add(new(g, "Validation", "DataValidationErrors with the Enyo error color", () => new ValidationPage(), "src/StockControls/ValidationPage.axaml", Scrolls: true, Notes: null));
        pages.Add(new(g, "Button Variants", "Hyperlink, drop-down and split buttons", () => new ButtonVariantsPage(), "src/StockControls/ButtonVariantsPage.axaml", Scrolls: true, Notes: null));
        pages.Add(new(g, "Text & Icons", "Separator, Label, SelectableTextBlock and PathIcon", () => new TextAndIconsPage(), "src/StockControls/TextAndIconsPage.axaml", Scrolls: true, Notes: null));
        pages.Add(new(g, "TableView", "Rows with item edges and an Onyx column header", () => new TableViewPage(), "src/StockControls/TableViewPage.axaml", Scrolls: false, Notes: null));
        pages.Add(new(g, "Window Notifications", "Stock notifications in the dashboard look", () => new NotificationsPage(), "src/StockControls/NotificationsPage.axaml", Scrolls: true, Notes: "AvaEnyo.Services has the webOS dashboard service (InAppNotificationService); this page restyles Avalonia's own notification cards."));
    }
}
