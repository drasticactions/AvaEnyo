using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using AvaEnyo.Controls.Primitives;
using Xunit;

namespace AvaEnyo.Controls.Tests;

/// <summary>Fixes found while building the gallery pages.</summary>
public class GalleryFixTests
{
    [AvaloniaFact]
    public void Group_Text_Header_Is_The_Uppercase_Label()
    {
        var group = new Group { Header = "Ingredients", Items = { new Item { Content = "a" } } };
        var w = TestHost.Show(group);
        Assert.Equal("Ingredients", group.HeaderText);
        Assert.Contains(":text-header", group.Classes);
        var label = group.GetVisualDescendants().OfType<TextBlock>().First(t => t.Name == "PART_HeaderText");
        Assert.True(label.IsEffectivelyVisible);
        Assert.Equal("INGREDIENTS", label.Text);
        group.Header = new Button();
        Assert.Null(group.HeaderText);
        Assert.DoesNotContain(":text-header", group.Classes);
        w.Close();
    }

    [AvaloniaFact]
    public void RowGroup_Text_Boxes_Pull_In_Over_The_Row_Padding()
    {
        var box = new TextBox();
        var rows = new RowGroup { Items = { box } };
        var w = TestHost.Show(rows);
        Assert.Contains("in-row", box.Classes);
        Assert.Equal(new Avalonia.Thickness(-10, -11), box.Margin);
        w.Close();
    }

    [AvaloniaFact]
    public void Divider_Icon_Geometry_Is_Drawn_As_An_Icon()
    {
        var divider = new Divider { Caption = "x", Icon = Avalonia.Media.StreamGeometry.Parse("M0,0 L10,0 10,10 Z") };
        var w = TestHost.Show(divider);
        var icon = divider.GetVisualDescendants().OfType<IconPresenter>().Single(i => i.Name == "PART_Icon");
        Assert.IsType<Avalonia.Controls.Shapes.Path>(icon.Child);
        w.Close();
    }

    [AvaloniaFact]
    public void Carousel_Applies_Its_Template_Only_To_Data_Views()
    {
        var carousel = new Carousel
        {
            Width = 300,
            Height = 100,
            ContentTemplate = new FuncDataTemplate<int>((i, _) => new TextBlock { Text = "item " + i }),
        };
        var w = TestHost.Show(new StackPanel { Children = { carousel } });
        carousel.SetCenterView(5);
        TestHost.Settle(w);
        Assert.Contains(carousel.GetVisualDescendants().OfType<TextBlock>(), t => t.Text == "item 5");
        var view = new Border { Name = "control-view" };
        carousel.SetCenterView(view);
        TestHost.Settle(w);
        Assert.True(view.IsAttachedToVisualTree());
        w.Close();
    }
}
