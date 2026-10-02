using Avalonia.Controls;
using AvaEnyo.Controls.Primitives;

namespace AvaEnyo.Controls.Tests;

public static partial class ThemedControls
{
    static partial void AddOverlays()
    {
        Add("ScrollFades", new Row("ScrollFades", () => new ScrollFades { Width = 100, Height = 50 }, ["PART_FadeTop", "PART_FadeBottom", "PART_FadeLeft", "PART_FadeRight"], true));
        Add("MenuPresenter", new Row("Menu", () => new MenuPresenter { Content = new StackPanel { Children = { new MenuItem { Header = "a" } } } }, ["PART_Frame", "PART_ScrollViewer", "PART_ContentPresenter"], false));
        Add("PopupListPresenter", new Row("Menu", () => new PopupListPresenter { Content = "x" }, ["PART_Frame", "PART_ContentPresenter"], false));
        Add("MenuItem", new Row("Menu", () => new MenuItem { Header = "Parent", Icon = EnyoGlyph.Plus, IsOpen = true, Items = { new MenuItem { Header = "Child" } } }, ["PART_Row", "PART_HeaderPresenter", "PART_ItemsPresenter", "PART_Arrow", "PART_Check", "PART_Icon"], true));
        Add("MenuCheckItem", new Row("Menu", () => new MenuCheckItem { Header = "Check", IsChecked = true }, ["PART_Row", "PART_Check"], true));
        Add("Menu", new Row("Menu", () => new Menu { Items = { new MenuItem { Header = "a" } } }, [], true));
        Add("PopupSelect", new Row("Menu", () => new PopupSelect { Items = { "a" } }, [], true));
        Add("PopupList", new Row("Menu", () => new PopupList { Items = { "a" } }, [], true));
        Add("ModalDialog", new Row("ModalDialog", () => new ModalDialog { Caption = "x" }, [], true));
        Add("ModalDialogPresenter", new Row("ModalDialog", () => new ModalDialogPresenter { Caption = "Title", Content = "Body" }, ["PART_Frame", "PART_Caption", "PART_ContentPresenter"], false));
        Add("Toaster", new Row("Toaster", () => new Toaster(), [], true));
        Add("Dialog", new Row("Toaster", () => new Dialog(), [], true));
        Add("DialogPrompt", new Row("Toaster", () => new DialogPrompt { Title = "t" }, [], true));
        Add("ToasterPresenter", new Row("Toaster", () => new ToasterPresenter { Content = "x" }, ["PART_ContentPresenter"], true));
        Add("DialogPresenter", new Row("Toaster", () => new DialogPresenter { Content = "x" }, ["PART_Frame", "PART_ContentPresenter"], false));
    }
}
