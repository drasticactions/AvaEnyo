namespace AvaEnyo.Controls.Tests;

public static partial class ThemedControls
{
    static partial void AddServicesPresenters()
    {
        Add("AppMenuPresenter", new Row("AppMenu", () => new AppMenuPresenter { Content = new AppMenuItem { Header = "New" } }, ["PART_Frame", "PART_ContentPresenter"], false));
        Add("AppMenuItem", new Row("AppMenu", () => new AppMenuItem { Header = "Sync", Icon = EnyoGlyph.Refresh }, ["PART_Header", "PART_ItemsPresenter"], true));
        Add("EditMenu", new Row("AppMenu", () => new EditMenu { IsExpanded = true, ShowShortcuts = true }, ["PART_Header", "PART_ItemsPresenter"], true));
        Add("HelpMenu", new Row("AppMenu", () => new HelpMenu(), ["PART_Header", "PART_ItemsPresenter"], true));
    }
}
