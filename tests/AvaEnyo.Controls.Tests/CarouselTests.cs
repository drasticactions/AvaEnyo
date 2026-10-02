using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Xunit;
using EnyoCarousel = AvaEnyo.Controls.Carousel;

namespace AvaEnyo.Controls.Tests;

public class CarouselTests
{
    private static (SnapScroller Scroller, Window Window, List<string> Events) MakeScroller()
    {
        var scroller = new SnapScroller();
        for (var i = 0; i < 4; i++)
        {
            scroller.Items.Add(new Border { Width = 300, Background = Avalonia.Media.Brushes.Gray });
        }

        var events = new List<string>();
        scroller.Snap += (_, e) => events.Add("snap" + e.Index);
        scroller.SnapFinished += (_, e) => events.Add("finished" + e.Index);
        var w = TestHost.Show(scroller, 300, 200);
        return (scroller, w, events);
    }

    [AvaloniaFact]
    public void SnapScroller_Swipe_Past_The_Threshold_Moves_To_The_Next_Item()
    {
        var (scroller, w, events) = MakeScroller();
        var touch = new TouchInput(w);
        touch.Drag(new Point(250, 100), new Vector(-100, 0));
        Assert.True(scroller.IsDragging);
        touch.Up(new Point(150, 100));
        TestHost.Settle(w);
        Assert.Equal(1, scroller.SelectedIndex);
        Assert.Equal(300, scroller.Position, 1);
        Assert.Equal(new[] { "snap1", "finished1" }, events);
        w.Close();
    }

    [AvaloniaFact]
    public void SnapScroller_Short_Swipe_Stays()
    {
        var (scroller, w, events) = MakeScroller();
        var touch = new TouchInput(w);
        touch.Drag(new Point(150, 100), new Vector(-20, 0));
        touch.Up(new Point(130, 100));
        TestHost.Settle(w);
        Assert.Equal(0, scroller.SelectedIndex);
        Assert.Equal(0, scroller.Position, 1);
        Assert.DoesNotContain("snap1", events);
        w.Close();
    }

    [AvaloniaFact]
    public void SnapScroller_SelectedIndex_Jumps()
    {
        var (scroller, w, events) = MakeScroller();
        scroller.SelectedIndex = 3;
        TestHost.Settle(w);
        Assert.Equal(900, scroller.Position, 1);
        Assert.Contains("finished3", events);
        scroller.Previous();
        Assert.Equal(2, scroller.SelectedIndex);
        w.Close();
    }

    private static (EnyoCarousel Carousel, Window Window, int[] Index) MakeCarousel(int start, int max = 9)
    {
        var index = new[] { start };
        var carousel = new EnyoCarousel();
        carousel.GetLeft += (_, e) =>
        {
            if (e.IsSnap)
            {
                index[0]--;
            }

            e.View = index[0] - 1 >= 0 ? (index[0] - 1).ToString() : null;
        };
        carousel.GetRight += (_, e) =>
        {
            if (e.IsSnap)
            {
                index[0]++;
            }

            e.View = index[0] + 1 <= max ? (index[0] + 1).ToString() : null;
        };
        var w = TestHost.Show(carousel, 300, 200);
        carousel.SetCenterView(start.ToString());
        TestHost.Settle(w);
        return (carousel, w, index);
    }

    [AvaloniaFact]
    public void Carousel_Asks_For_Neighbors_And_Rotates_After_A_Snap()
    {
        var (carousel, w, index) = MakeCarousel(5);
        Assert.Equal("5", carousel.CenterView);
        Assert.Equal(1, carousel.Index);
        Assert.Equal(300, carousel.Position, 1);
        var touch = new TouchInput(w);
        touch.Drag(new Point(250, 100), new Vector(-50, 0));
        touch.Up(new Point(200, 100));
        TestHost.Settle(w);
        Assert.Equal("6", carousel.CenterView);
        Assert.Equal(6, index[0]);
        Assert.Equal(1, carousel.Index);
        Assert.Equal(300, carousel.Position, 1);
        w.Close();
    }

    [AvaloniaFact]
    public void Carousel_Stops_At_An_End()
    {
        var (carousel, w, index) = MakeCarousel(8);
        carousel.Next();
        TestHost.Settle(w);
        Assert.Equal("9", carousel.CenterView);
        Assert.Equal(2, carousel.Index);
        carousel.Next();
        Assert.Equal("9", carousel.CenterView);
        carousel.Previous();
        TestHost.Settle(w);
        Assert.Equal("8", carousel.CenterView);
        Assert.Equal(1, carousel.Index);
        Assert.Equal(8, index[0]);
        w.Close();
    }

    [AvaloniaFact]
    public void Carousel_First_View_Has_No_Left()
    {
        var (carousel, w, _) = MakeCarousel(0);
        Assert.Equal("0", carousel.CenterView);
        Assert.Equal(0, carousel.Position, 1);
        carousel.Previous();
        Assert.Equal("0", carousel.CenterView);
        w.Close();
    }

    [AvaloniaFact]
    public void VirtualCarousel_Reuses_Three_Views_And_Stops_At_The_Ends()
    {
        var carousel = new VirtualCarousel { ItemsSource = new[] { "a", "b", "c", "d" } };
        var w = TestHost.Show(carousel, 300, 200);
        carousel.RenderViews(0);
        TestHost.Settle(w);
        var views = new HashSet<Control?>();
        Assert.Equal("a", ((ContentControl)carousel.CurrentView!).Content);
        for (var i = 1; i < 4; i++)
        {
            carousel.Next();
            TestHost.Settle(w);
            views.Add(carousel.CurrentView);
            Assert.Equal(i, carousel.SelectedIndex);
        }

        Assert.Equal("d", ((ContentControl)carousel.CurrentView!).Content);
        Assert.True(views.Count <= 3);
        carousel.Next();
        Assert.Equal(3, carousel.SelectedIndex);
        carousel.Previous();
        TestHost.Settle(w);
        Assert.Equal(2, carousel.SelectedIndex);
        Assert.Equal("c", ((ContentControl)carousel.CurrentView!).Content);
        w.Close();
    }

    [AvaloniaFact]
    public void VirtualCarousel_SetupView_False_Is_An_End()
    {
        var carousel = new VirtualCarousel();
        carousel.SetupViewRequested += (_, e) =>
        {
            e.Accepted = e.Index >= 0 && e.Index <= 2;
            ((ContentControl)e.View).Content = "v" + e.Index;
            e.Handled = true;
        };
        var w = TestHost.Show(carousel, 300, 200);
        carousel.RenderViews(2);
        TestHost.Settle(w);
        Assert.Equal("v2", ((ContentControl)carousel.CurrentView!).Content);
        carousel.Next();
        Assert.Equal(2, carousel.SelectedIndex);
        carousel.SelectedIndex = 0;
        TestHost.Settle(w);
        Assert.Equal("v0", ((ContentControl)carousel.CurrentView!).Content);
        w.Close();
    }
}
