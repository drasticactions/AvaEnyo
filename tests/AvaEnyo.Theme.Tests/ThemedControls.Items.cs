using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;

namespace AvaEnyo.Theme.Tests;

public static partial class ThemedControls
{
    static partial void AddItems()
    {
        Add("ListBox", () => new ListBox { ItemsSource = new[] { "a", "b", "c" } },
            ["PART_ScrollViewer", "PART_ItemsPresenter"], transparent: true);
        Add("ListBoxItem", () => new ListBox { Items = { new ListBoxItem { Content = "x" }, new ListBoxItem { Content = "y" } } },
            ["PART_LayoutRoot", "PART_HeldBorder", "PART_ContentPresenter"], transparent: true);
        Add("ComboBox", () => new ComboBox { ItemsSource = new[] { "a", "b" }, SelectedIndex = 0 },
            ["PART_Popup", "PART_ItemsPresenter", "PART_EditableTextBox", "PART_Label", "DropDownGlyph", "PopupBorder"]);
        Add("ComboBoxItem", () => new ComboBoxItem { Content = "x" }, ["PART_ContentPresenter", "PART_HeldBorder"]);
        Add("ItemsControl", () => new ItemsControl { ItemsSource = new[] { "a" } }, ["PART_ItemsPresenter"], transparent: true);
        Add("HeaderedContentControl", () => new HeaderedContentControl { Header = "h", Content = "c" },
            ["PART_HeaderPresenter", "PART_ContentPresenter"], transparent: true);
        Add("GroupBox", () => new GroupBox { Header = "h", Content = "c" },
            ["PART_HeaderPresenter", "PART_ContentPresenter", "PART_Frame", "PART_Highlight"]);
        Add("Expander", () => new Expander { Header = "h", Content = "c" },
            ["PART_ContentPresenter", "ExpanderHeader", "ExpanderContent", "ExpandCollapseChevron"]);
        Add("Separator", () => new Separator());
        Add("TreeView", () => new TreeView { ItemsSource = new[] { "a" } }, ["PART_ItemsPresenter"]);
        Add("TreeViewItem", () => new TreeView { Items = { new TreeViewItem { Header = "h", ItemsSource = new[] { "child" }, IsExpanded = true } } },
            ["PART_LayoutRoot", "PART_Header", "PART_HeaderPresenter", "PART_ItemsPresenter", "PART_ExpandCollapseChevron"]);
        Add("TabControl", () => new TabControl { ItemsSource = new[] { new TabItem { Header = "a", Content = "x" }, new TabItem { Header = "b" } } },
            ["PART_ItemsPresenter", "PART_SelectedContentHost", "PART_SelectedContentHost2"], transparent: true);
        Add("TabItem", () => new TabControl { Items = { new TabItem { Header = "h", Content = "c" }, new TabItem { Header = "i" } } },
            ["PART_LayoutRoot", "PART_ContentPresenter"], transparent: true);
        Add("TabStrip", () => new TabStrip { ItemsSource = new[] { "a", "b" } }, ["PART_ItemsPresenter"], transparent: true);
        Add("TabStripItem", () => new TabStripItem { Content = "x" }, ["PART_LayoutRoot", "PART_ContentPresenter"]);
        Add("GridSplitter", () => new GridSplitter(), ["PART_Line"], transparent: true);
        Add("SplitView", () => new SplitView { Pane = new Border(), Content = new Border(), IsPaneOpen = true },
            ["PART_PaneRoot", "PART_PanePresenter", "PART_ContentPresenter", "PART_PaneShadow"], transparent: true);
        Add("Carousel", () => new Carousel { ItemsSource = new[] { "a", "b" } }, ["PART_ScrollViewer", "PART_ItemsPresenter"], transparent: true);
        Add("TransitioningContentControl", () => new TransitioningContentControl { Content = "x" },
            ["PART_ContentPresenter", "PART_ContentPresenter2"], transparent: true);
        Add("NotificationCard", () => new NotificationCard { Content = "x" },
            ["PART_LayoutTransformControl", "PART_HeaderBar", "PART_ContentPresenter"]);
        Add("WindowNotificationManager", () => new WindowNotificationManager(), ["PART_Items"], transparent: true);
    }
}
