using System.Linq;

namespace AvaEnyo.Controls.Tests;

public static partial class ThemedControls
{
    static partial void AddLists()
    {
        Add("VirtualList", new Row("Lists", () => new VirtualList { Height = 200, ItemsSource = Enumerable.Range(0, 50).Select(i => "Row " + i).ToList() }, ["PART_ScrollViewer", "PART_ItemsPresenter"], true));
        Add("VirtualListGrid", new Row("Lists", () => new VirtualList { Height = 200, Layout = VirtualListLayout.Grid, ItemsPerRow = 3, ItemsSource = Enumerable.Range(0, 50).Select(i => "Cell " + i).ToList() }, ["PART_ScrollViewer", "PART_ItemsPresenter"], true));
        Add("SwipeableItem", new Row("Lists", () => new SwipeableItem { Content = "x", ConfirmShowing = true }, ["PART_Border", "PART_ConfirmPrompt", "PART_ContentPresenter"], true));
        Add("ConfirmPrompt", new Row("Lists", () => new ConfirmPrompt(), ["PART_ConfirmButton", "PART_CancelButton"], false));
    }
}
