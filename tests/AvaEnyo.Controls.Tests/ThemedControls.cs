using System;
using System.Collections.Generic;
using Avalonia.Controls;
using AvaEnyo.Controls.Primitives;
using Xunit;

namespace AvaEnyo.Controls.Tests;

/// <summary>One row per AvaEnyo.Controls theme: a factory, the theme file, the required parts.</summary>
public static partial class ThemedControls
{
    public sealed record Row(string File, Func<Control> Create, string[] Parts, bool Transparent);

    public static readonly Dictionary<string, Row> Rows = new()
    {
        ["Item"] = new("Item", () => new Item { Content = "x", ShowDivider = true, DividerCaption = "A" }, ["PART_ContentPresenter", "PART_Divider", "PART_Border"], true),
        ["RowItem"] = new("Item", () => new RowItem { Content = "x" }, ["PART_ContentPresenter"], true),
        ["Divider"] = new("Divider", () => new Divider { Caption = "Section" }, ["PART_Caption", "PART_Rule"], true),
        ["AlphaDivider"] = new("Divider", () => new AlphaDivider { Caption = "A" }, ["PART_Caption", "PART_Rule"], true),
        ["Header"] = new("Header", () => new Header { Content = "Title", Classes = { "dark" } }, ["PART_ContentPresenter", "PART_Background", "PART_Sheen"], false),
        ["PageHeader"] = new("Header", () => new PageHeader { Content = "Title", ShowBackButton = true }, ["PART_ContentPresenter", "PART_BackButton"], false),
        ["Toolbar"] = new("Toolbar", () => new Toolbar { Items = { new ToolButton { Content = "A" }, new Spacer(), "B" } }, ["PART_ItemsPresenter", "PART_Background"], false),
        ["ToolbarLight"] = new("Toolbar", () => new Toolbar { Classes = { "light" }, Items = { new Button { Content = "A" } } }, ["PART_ItemsPresenter"], false),
        ["GrabButton"] = new("Toolbar", () => new GrabButton(), ["PART_Bars"], true),
        ["Group"] = new("Group", () => new Group { Header = "Label", Items = { new Item { Content = "a" }, new Item { Content = "b" } } }, ["PART_Frame", "PART_Band", "PART_HeaderPresenter", "PART_ItemsPresenter"], false),
        ["RowGroup"] = new("Group", () => new RowGroup { Items = { new TextBox(), new TextBox(), new TextBox() } }, ["PART_Frame", "PART_ItemsPresenter"], false),
        ["LabeledContainer"] = new("Group", () => new LabeledContainer { Header = "Name", Content = "x" }, ["PART_HeaderPresenter", "PART_ContentPresenter"], true),
        ["FloatingHeader"] = new("Group", () => new FloatingHeader { Content = "x" }, ["PART_ContentPresenter"], false),
        ["IconButton"] = new("Buttons", () => new IconButton { Content = "Go", Icon = EnyoGlyph.Search }, ["PART_Icon", "PART_ContentPresenter", "PART_Frame"], false),
        ["ToolButton"] = new("Buttons", () => new ToolButton { Icon = EnyoGlyph.Plus }, ["PART_Icon", "PART_ContentPresenter"], false),
        ["ToolButtonGroup"] = new("Buttons", () => new ToolButtonGroup { Items = { "A", "B", "C" } }, ["PART_ItemsPresenter"], true),
        ["ActivityButton"] = new("Buttons", () => new ActivityButton { Content = "Send", IsActive = true }, ["PART_Spinner", "PART_ContentPresenter"], false),
        ["RadioGroup"] = new("Segments", () => new RadioGroup { ItemsSource = new[] { "a", "b", "c" }, Classes = { "dark" } }, ["PART_ItemsPresenter"], true),
        ["RadioSegment"] = new("Segments", () => new RadioSegment { Content = "x", IsChecked = true }, ["PART_Border", "PART_ContentPresenter"], false),
        ["TabGroup"] = new("Segments", () => new TabGroup { ItemsSource = new[] { "a", "b" } }, ["PART_ItemsPresenter"], true),
        ["TabSegment"] = new("Segments", () => new TabSegment { Content = "x" }, ["PART_Border"], false),
        ["RadioToolButtonGroup"] = new("Segments", () => new RadioToolButtonGroup { ItemsSource = new[] { "a", "b" } }, ["PART_ItemsPresenter"], true),
        ["Spinner"] = new("Spinner", () => new Spinner { IsActive = true, Classes = { "large" } }, ["PART_Ring"], true),
        ["InputBox"] = new("InputBox", () => new InputBox { Content = new TextBox(), AlwaysLooksFocused = true }, ["PART_Frame", "PART_ContentPresenter"], false),
        ["Pane"] = new("Pane", () => new Pane { Views = { new TextBlock { Text = "a" }, new TextBlock { Text = "b" } } }, ["PART_Host", "PART_InputBlocker"], true),
        ["PaneView"] = new("Pane", () => new PaneView { Content = "x" }, ["PART_ContentPresenter"], true),
        ["SlidingPane"] = new("SlidingPane", () => new SlidingPane { Views = { new SlidingView { Width = 100 }, new SlidingView { EdgeDragging = true } } }, ["PART_Panel"], true),
        ["SlidingView"] = new("SlidingPane", () => new SlidingPane { Views = { new SlidingView { Width = 100, Content = "x" }, new SlidingView() } }, ["PART_Shadow", "PART_Content", "PART_EdgeDragger"], true),
    };

    static ThemedControls()
    {
        AddOverlays();
        AddPickers();
        AddLists();
        AddMedia();
        AddExtras();
        AddServicesPresenters();
    }

    static partial void AddOverlays();

    static partial void AddPickers();

    static partial void AddLists();

    static partial void AddMedia();

    static partial void AddExtras();

    static partial void AddServicesPresenters();

    public static TheoryData<string, string, EnyoDensity> Matrix
    {
        get
        {
            var data = new TheoryData<string, string, EnyoDensity>();
            foreach (var name in Rows.Keys)
            {
                foreach (var v in TestHost.Variants)
                {
                    foreach (var d in TestHost.Densities)
                    {
                        data.Add(name, v, d);
                    }
                }
            }

            return data;
        }
    }

    /// <summary>Extension point for rows added by other test files.</summary>
    public static void Add(string name, Row row) => Rows[name] = row;
}
