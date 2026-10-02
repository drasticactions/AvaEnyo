using Avalonia.Collections;
using Avalonia.Controls;

namespace AvaEnyo.Theme.Tests;

public static partial class ThemedControls
{
    static partial void AddMisc()
    {
        Add("PipsPager", () => new PipsPager { NumberOfPages = 3 }, ["PART_RootPanel", "PART_PreviousButton", "PART_NextButton", "PART_PipsPagerList"], transparent: true);
        Add("RefreshContainer", () => new RefreshContainer { Content = new Border() }, ["PART_ContentPresenter", "PART_RefreshVisualizerPresenter"], transparent: true);
        Add("RefreshVisualizer", () => new RefreshVisualizer(), ["PART_Root"], transparent: true);
        Add("ManagedFileChooser", () => new Avalonia.Dialogs.ManagedFileChooser(), ["PART_QuickLinks", "PART_Files", "NavBar", "Finalize"], scopedKeys: ["Icons"]);
        Add("TableView", () => new TableView
        {
            Columns = new AvaloniaList<TableViewColumn> { new TableViewColumn { Header = "Name" } },
            ItemsSource = new[] { "a", "b" },
        },
            ["PART_ScrollViewer", "PART_ItemsPresenter", "PART_ContentPresenter"]);
        Add("TableViewRow", () => new TableViewRow { Content = "x" }, ["PART_CellsPresenter"], transparent: true);
        Add("TableViewCell", () => new TableViewCell { Content = "x" }, ["PART_ContentPresenter"], transparent: true);
        Add("TableViewColumnHeader", () => new TableViewColumnHeader { Content = "x" }, ["PART_ContentPresenter", "PART_Resizer"], transparent: true);
    }
}
