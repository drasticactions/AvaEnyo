using Avalonia.Controls;

namespace AvaEnyo.Theme.Tests;

public static partial class ThemedControls
{
    static partial void AddPages()
    {
        Add("CommandBar", () => new CommandBar
        {
            PrimaryCommands =
            {
                new CommandBarButton { Label = "Add", Icon = new PathIcon { Data = Avalonia.Media.StreamGeometry.Parse("M0,0 L10,0 10,10 Z") } },
                new CommandBarToggleButton { Label = "Fav", IsChecked = true },
                new CommandBarSeparator(),
                new CommandBarButton { Label = "Del" },
            },
            SecondaryCommands = { new CommandBarButton { Label = "More" } },
        },
            ["PART_ContentPresenter", "PART_OverflowButton", "PART_OverflowPopup", "PART_OverflowPresenter", "PART_PrimaryCommands", "PART_Background", "PART_Sheen"]);
        Add("ContentPage", () => new ContentPage { Content = "x" }, ["PART_TopCommandBar", "PART_BottomCommandBar", "PART_ContentPresenter"]);
        Add("NavigationPage", () =>
        {
            var n = new NavigationPage();
            _ = n.PushAsync(new ContentPage { Header = "Root", Content = "x" });
            _ = n.PushAsync(new ContentPage { Header = "Page", Content = "y" });
            return n;
        },
            ["PART_BackButton", "PART_BackButtonContentPresenter", "PART_BackButtonDefaultIcon", "PART_BottomCommandBar", "PART_ContentHost",
             "PART_ModalBackPresenter", "PART_ModalPresenter", "PART_NavBarShadow", "PART_NavigationBar", "PART_PageBackPresenter",
             "PART_PagePresenter", "PART_TopCommandBar", "PART_Header"], transparent: true);
        Add("DrawerPage", () => new DrawerPage { Drawer = new ContentPage { Content = "d" }, Content = new ContentPage { Content = "x" } },
            ["PART_SplitView", "PART_ContentPresenter", "PART_TopBar",
             "PART_PaneButton", "PART_Backdrop"]);
        Add("TabbedPage", () => new TabbedPage { Pages = new[] { new ContentPage { Header = "a", Content = "x" }, new ContentPage { Header = "b" } } },
            ["PART_TabControl", "PART_ItemsPresenter", "PART_SelectedContentHost", "PART_SelectedContentHost2"]);
        Add("CarouselPage", () => new CarouselPage { Pages = new[] { new ContentPage { Content = "x" }, new ContentPage { Content = "y" } } },
            ["PART_Carousel", "PART_Pips"]);
    }
}
