using Avalonia.Controls;

namespace AvaEnyo.Theme.Tests;

public static partial class ThemedControls
{
    static partial void AddPopups()
    {
        Add("Menu", () => new Menu { ItemsSource = new[] { new MenuItem { Header = "File", ItemsSource = new[] { new MenuItem { Header = "New" } } } } },
            ["PART_ItemsPresenter"]);
        Add("MenuItem", () => new MenuItem
        {
            Header = "x",
            InputGesture = new Avalonia.Input.KeyGesture(Avalonia.Input.Key.S, Avalonia.Input.KeyModifiers.Control),
            ItemsSource = new[] { new MenuItem { Header = "child" } },
        }, ["PART_LayoutRoot", "PART_HeaderPresenter", "PART_Popup", "PART_ItemsPresenter", "PART_ToggleIconPresenter", "PART_IconPresenter", "PART_InputGestureText", "PART_ChevronPath"], transparent: true);
        Add("ContextMenu", () => new ContextMenu { ItemsSource = new[] { new MenuItem { Header = "a" } } }, ["PART_ItemsPresenter", "LayoutRoot"]);
        Add("MenuFlyoutPresenter", () => new MenuFlyoutPresenter { ItemsSource = new[] { new MenuItem { Header = "a" } } }, ["PART_ItemsPresenter", "LayoutRoot"]);
        Add("FlyoutPresenter", () => new FlyoutPresenter { Content = "x" }, ["PART_ContentPresenter", "LayoutRoot"]);
        Add("ToolTip", () => new ToolTip { Content = "x" }, ["PART_LayoutRoot", "PART_ContentPresenter"]);
    }
}
