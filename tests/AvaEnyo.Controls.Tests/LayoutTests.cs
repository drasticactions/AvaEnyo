using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using AvaEnyo.Controls.Primitives;
using Xunit;

namespace AvaEnyo.Controls.Tests;

public class LayoutTests
{
    [AvaloniaFact]
    public void Flex_Children_Share_The_Leftover_Space_From_Zero()
    {
        var f1 = new Border { Child = new Border { Width = 500 } };
        var f2 = new Border();
        Flex.SetValue(f1, 1);
        Flex.SetValue(f2, 2);
        var box = new HFlexBox { Width = 450, Height = 50, Children = { new Border { Width = 50 }, f1, new Border { Width = 100 }, f2 } };
        var w = TestHost.Show(new Canvas { Children = { box } });
        Assert.Equal(100, f1.Bounds.Width, 3);
        Assert.Equal(200, f2.Bounds.Width, 3);
        Assert.Equal(50, f1.Bounds.X, 3);
        Assert.Equal(250, f2.Bounds.X, 3);
        w.Close();
    }

    [AvaloniaFact]
    public void Pack_And_Align_Place_Unflexed_Children()
    {
        var child = new Border { Width = 100, Height = 20 };
        var box = new HFlexBox { Width = 400, Height = 100, Pack = FlexPack.Center, Align = FlexAlign.End, Children = { child } };
        var w = TestHost.Show(new Canvas { Children = { box } });
        Assert.Equal(150, child.Bounds.X, 3);
        Assert.Equal(80, child.Bounds.Y, 3);
        box.Pack = FlexPack.End;
        box.Align = FlexAlign.Stretch;
        child.Height = double.NaN;
        TestHost.Settle(w);
        Assert.Equal(300, child.Bounds.X, 3);
        Assert.Equal(100, child.Bounds.Height, 3);
        w.Close();
    }

    [AvaloniaFact]
    public void Justify_Spreads_Children()
    {
        var a = new Border { Width = 50 };
        var b = new Border { Width = 50 };
        var c = new Border { Width = 50 };
        var box = new HFlexBox { Width = 350, Height = 20, Pack = FlexPack.Justify, Children = { a, b, c } };
        var w = TestHost.Show(new Canvas { Children = { box } });
        Assert.Equal(0, a.Bounds.X, 3);
        Assert.Equal(150, b.Bounds.X, 3);
        Assert.Equal(300, c.Bounds.X, 3);
        w.Close();
    }

    [AvaloniaFact]
    public void VFlexBox_Fills_Vertically()
    {
        var header = new Border { Height = 50 };
        var body = new Border();
        Flex.SetValue(body, 1);
        var box = new VFlexBox { Width = 100, Height = 300, Children = { header, body, new Border { Height = 54 } } };
        var w = TestHost.Show(new Canvas { Children = { box } });
        Assert.Equal(196, body.Bounds.Height, 3);
        Assert.Equal(50, body.Bounds.Y, 3);
        w.Close();
    }

    [AvaloniaFact]
    public void OrderedPanel_Marks_Visible_Children()
    {
        var a = new Border();
        var b = new Border();
        var c = new Border();
        var panel = new OrderedPanel { Children = { a, b, c } };
        var w = TestHost.Show(panel);
        Assert.Equal(OrderPosition.First, Order.GetPosition(a));
        Assert.Equal(OrderPosition.Middle, Order.GetPosition(b));
        Assert.Equal(OrderPosition.Last, Order.GetPosition(c));
        Assert.Contains(":first", a.Classes);
        b.IsVisible = false;
        TestHost.Settle(w);
        Assert.Equal(OrderPosition.Last, Order.GetPosition(c));
        Assert.Equal(OrderPosition.None, Order.GetPosition(b));
        a.IsVisible = false;
        TestHost.Settle(w);
        Assert.Equal(OrderPosition.Single, Order.GetPosition(c));
        Assert.Contains(":single", c.Classes);
        w.Close();
    }

    [AvaloniaFact]
    public void Toolbar_Pins_The_Grab_Button_Left_And_Centers_The_Rest()
    {
        var grab = new GrabButton();
        var tool = new ToolButton { Content = "Go" };
        var bar = new Toolbar { Width = 600, Items = { grab, tool } };
        var w = TestHost.Show(new StackPanel { Children = { bar } });
        var g = grab.TranslatePoint(default, bar)!.Value;
        var t = tool.TranslatePoint(default, bar)!.Value;
        Assert.InRange(g.X, 0, 6);
        Assert.InRange(t.X + (tool.Bounds.Width / 2), 290, 310);
        w.Close();
    }

    [AvaloniaFact]
    public void Toolbar_Spacer_Pushes_Trailing_Buttons_To_The_Right_Edge()
    {
        var grab = new GrabButton();
        var reply = new ToolButton { Content = "Reply" };
        var forward = new ToolButton { Content = "Forward" };
        var bar = new Toolbar { Items = { grab, reply, new Spacer(), forward } };
        var w = TestHost.Show(new StackPanel { Width = 600, Children = { bar } });
        var f = forward.TranslatePoint(default, bar)!.Value;
        var r = reply.TranslatePoint(default, bar)!.Value;
        Assert.InRange(f.X + forward.Bounds.Width, 560, 600);
        Assert.True(r.X >= grab.Bounds.Right, $"reply at {r.X}, grab ends at {grab.Bounds.Right}");
        w.Close();
    }

    [AvaloniaFact]
    public void Toolbar_In_A_Peeking_Sliding_View_Fits_The_View()
    {
        var forward = new Button { Content = "Forward" };
        var bar = new Toolbar { Items = { new Button { Content = "Cancel" }, new Spacer(), forward } };
        var sample = new SlidingView { PeekWidth = 320, Content = new ScrollViewer { Content = new StackPanel { Margin = new Thickness(16), Children = { bar } } } };
        Flex.SetValue(sample, 1);
        var pane = new SlidingPane { Views = { new SlidingView { Width = 320, Content = new Border() }, sample } };
        var w = TestHost.Show(pane, 1024, 768);
        pane.SelectView(sample, true);
        TestHost.Settle(w);
        var f = forward.TranslatePoint(default, sample)!.Value;
        Assert.InRange(f.X + forward.Bounds.Width, sample.Bounds.Width - 40, sample.Bounds.Width);
        Assert.InRange(sample.Bounds.Width, 690, 720);
        w.Close();
    }

    [AvaloniaFact]
    public void Divider_Keeps_Its_Rule_Beside_A_Long_Caption()
    {
        var d = new Divider { Caption = new string('W', 200), Width = 300 };
        var w = TestHost.Show(new StackPanel { Children = { d } });
        var rule = Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(d).OfType<Border>().First(b => b.Name == "PART_Rule");
        Assert.True(rule.Bounds.Width >= 3, $"rule {rule.Bounds.Width}");
        w.Close();
    }
}
