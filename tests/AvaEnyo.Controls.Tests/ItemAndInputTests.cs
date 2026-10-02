using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace AvaEnyo.Controls.Tests;

public class ItemAndInputTests
{
    [AvaloniaFact]
    public void Tap_Clicks_And_Shows_Held_At_Once()
    {
        var item = new Item { Content = "Row", Height = 50 };
        var clicks = 0;
        item.Click += (_, _) => clicks++;
        var w = TestHost.Show(new StackPanel { Children = { item } });
        var touch = new TouchInput(w);
        touch.Down(new Point(50, 25));
        Assert.True(item.IsHeld);
        Assert.Contains(":held", item.Classes);
        touch.Up(new Point(50, 25));
        Assert.False(item.IsHeld);
        Assert.Equal(1, clicks);
        w.Close();
    }

    [AvaloniaFact]
    public void A_Drag_In_A_Scroller_Does_Not_Click()
    {
        var item = new Item { Content = "Row", Height = 50 };
        var clicks = 0;
        item.Click += (_, _) => clicks++;
        var list = new StackPanel { Children = { item } };
        for (var i = 0; i < 30; i++)
        {
            list.Children.Add(new Item { Content = "x", Height = 50 });
        }

        var w = TestHost.Show(new ScrollViewer { Content = list }, 300, 300);
        var touch = new TouchInput(w);
        touch.Drag(new Point(50, 25), new Vector(0, -100));
        touch.Up(new Point(50, -75));
        Assert.Equal(0, clicks);
        Assert.False(item.IsHeld);
        w.Close();
    }

    [AvaloniaFact]
    public void Item_Click_Delay_Can_Be_Restored()
    {
        ThemeSettings.DelayItemClick = true;
        ThemeSettings.ItemClickDelay = 1;
        try
        {
            var item = new Item { Content = "Row", Height = 50 };
            var clicks = 0;
            item.Click += (_, _) => clicks++;
            var w = TestHost.Show(new StackPanel { Children = { item } });
            new TouchInput(w).Tap(new Point(10, 10));
            Assert.Equal(0, clicks);
            TestHost.Pump(500, () => clicks > 0);

            Assert.Equal(1, clicks);
            w.Close();
        }
        finally
        {
            ThemeSettings.DelayItemClick = false;
            ThemeSettings.ItemClickDelay = 100;
        }
    }

    [AvaloniaFact]
    public void RadioGroup_Value_Defaults_To_Index()
    {
        var group = new RadioGroup { ItemsSource = new[] { "Day", "Week", "Month" } };
        var w = TestHost.Show(group);
        object? reported = null;
        group.ValueChanged += (_, e) => reported = e.NewValue;
        group.SelectedIndex = 2;
        Assert.Equal(2, group.Value);
        Assert.Equal(2, reported);
        var segments = group.GetVisualDescendants().OfType<RadioSegment>().ToList();
        Assert.Equal(3, segments.Count);
        Assert.True(segments[2].IsChecked);
        Assert.Contains(":first", segments[0].Classes);
        Assert.Contains(":last", segments[2].Classes);
        group.Value = 0;
        Assert.Equal(0, group.SelectedIndex);
        w.Close();
    }

    [AvaloniaFact]
    public void RadioGroup_Selection_Does_Not_Scroll_The_Page()
    {
        var panel = new StackPanel { Children = { new Border { Height = 2000 } } };
        var group = new RadioGroup { ItemsSource = new[] { "a", "b" } };
        panel.Children.Add(group);
        var viewer = new ScrollViewer { Content = panel };
        var w = TestHost.Show(viewer, 300, 300);
        group.SelectedIndex = 1;
        TestHost.Settle(w);
        Assert.Equal(0, viewer.Offset.Y);
        w.Close();
    }

    [AvaloniaFact]
    public void RadioSegment_Value_Is_Reported()
    {
        var group = new RadioGroup { Items = { new RadioSegment { Content = "A", Value = "a" }, new RadioSegment { Content = "B", Value = "b" } } };
        var w = TestHost.Show(group);
        group.SelectedIndex = 1;
        Assert.Equal("b", group.Value);
        w.Close();
    }

    [AvaloniaFact]
    public void Search_Commits_On_Enter_And_After_The_Delay()
    {
        var box = new TextBox { Classes = { "search" } };
        Search.SetInputDelay(box, 1);
        string? committed = null;
        var cancelled = 0;
        Search.AddCommittedHandler(box, (_, e) => committed = e.Text);
        Search.AddCancelledHandler(box, (_, _) => cancelled++);
        var w = TestHost.Show(box);
        box.Text = "enyo";
        TestHost.Pump(500, () => committed is not null);

        Assert.Equal("enyo", committed);
        committed = null;
        box.Focus();
        box.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Enter, Source = box });
        Assert.Equal("enyo", committed);
        box.Text = string.Empty;
        Assert.Equal(1, cancelled);
        w.Close();
    }

    [AvaloniaFact]
    public void ToolButton_Toggles_When_IsToggle()
    {
        var b = new ToolButton { Content = "Bold", IsToggle = true, Width = 80, Height = 40 };
        var w = TestHost.Show(new Canvas { Children = { b } });
        Avalonia.Headless.HeadlessWindowExtensions.MouseDown(w, new Point(40, 20), MouseButton.Left);
        Avalonia.Headless.HeadlessWindowExtensions.MouseUp(w, new Point(40, 20), MouseButton.Left);
        Assert.True(b.IsChecked);
        Assert.Contains(":checked", b.Classes);
        Assert.Contains(":captioned", b.Classes);
        w.Close();
    }

    [AvaloniaFact]
    public void Spinner_Is_Hidden_Until_Active()
    {
        var s = new Spinner();
        var w = TestHost.Show(new StackPanel { Children = { s } });
        Assert.False(s.IsVisible);
        s.IsActive = true;
        TestHost.Settle(w);
        Assert.True(s.IsVisible);
        w.Close();
    }

    [AvaloniaFact]
    public void Every_Divider_In_A_Group_Is_Dark_Over_Light()
    {
        var items = Enumerable.Range(0, 4).Select(i => new Item { Content = "Row " + i }).ToArray();
        var group = new Group();
        foreach (var item in items)
        {
            group.Items.Add(item);
        }

        var w = TestHost.Show(new StackPanel { Children = { group } }, 300, 400);
        var frame = TestHost.Render(w);
        for (var i = 1; i < items.Length; i++)
        {
            var top = items[i].TranslatePoint(new Point(100, 0), w)!.Value;
            var dark = TestHost.PixelAt(frame, top.WithY(top.Y - 0.5));
            var light = TestHost.PixelAt(frame, top.WithY(top.Y + 0.5));
            Assert.True(light.R > dark.R, $"divider above row {i}: {dark} then {light}");
        }

        w.Close();
    }
}
