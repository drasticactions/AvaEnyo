using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AvaEnyo.Controls.Primitives;
using Xunit;

namespace AvaEnyo.Controls.Tests;

public class SlidingPaneTests
{
    private static (SlidingPane Pane, SlidingView A, SlidingView B, SlidingView C, Window Window) Make(double width = 1024, bool dismissible = false)
    {
        var a = new SlidingView { Width = 320, Name = "a", Content = new Border() };
        var b = new SlidingView { Width = 320, Name = "b", Content = new Border() };
        var c = new SlidingView { Name = "c", Dismissible = dismissible, Content = new Border() };
        Flex.SetValue(c, 1);
        var pane = new SlidingPane { Views = { a, b, c } };
        var window = TestHost.Show(pane, width, 600);
        return (pane, a, b, c, window);
    }

    [AvaloniaFact]
    public void Views_Start_At_Their_Natural_Places()
    {
        var (pane, a, b, c, w) = Make();
        Assert.False(pane.IsNarrow);
        Assert.Equal(0, a.SlidePosition);
        Assert.Equal(320, b.NaturalLeft);
        Assert.Equal(640, c.NaturalLeft);
        Assert.Equal(0, b.SlidePosition);
        Assert.Equal(0, c.SlidePosition);
        Assert.Equal(384, c.Bounds.Width, 0);
        w.Close();
    }

    [AvaloniaFact]
    public void Selecting_The_Last_View_Slides_It_Over_The_Others()
    {
        var (pane, a, b, c, w) = Make();
        pane.SelectViewByIndex(2);
        TestHost.Settle(w);
        Assert.Equal(-640, c.SlidePosition);
        Assert.Equal(-320, b.SlidePosition);
        Assert.Equal(0, a.SlidePosition);
        Assert.Equal(1024, c.Bounds.Width, 0);
        w.Close();
    }

    [AvaloniaFact]
    public void Views_After_The_Selected_Follow_It()
    {
        var (pane, a, b, c, w) = Make();
        pane.SelectViewByIndex(1);
        TestHost.Settle(w);
        Assert.Equal(-320, b.SlidePosition);
        Assert.Equal(-320, c.SlidePosition);
        Assert.Equal(704, c.Bounds.Width, 0);
        w.Close();
    }

    [AvaloniaFact]
    public void View_Zero_Never_Moves()
    {
        var (pane, a, _, _, w) = Make();
        pane.SelectViewByIndex(2);
        pane.SelectViewByIndex(0);
        TestHost.Settle(w);
        Assert.Equal(0, a.SlidePosition);
        Assert.Null(a.RenderTransform is Avalonia.Media.TranslateTransform t && t.X != 0 ? t : null);
        w.Close();
    }

    [AvaloniaFact]
    public void Narrow_Mode_At_Or_Below_900_Makes_Views_Full_Width()
    {
        var (pane, a, b, c, w) = Make(900);
        Assert.True(pane.IsNarrow);
        Assert.Equal(900, a.Bounds.Width, 0);
        Assert.Equal(900, b.NaturalLeft);
        pane.SelectViewByIndex(1);
        TestHost.Settle(w);
        Assert.Equal(-900, b.SlidePosition);
        w.Width = 901;
        TestHost.Settle(w);
        Assert.False(pane.IsNarrow);
        Assert.Equal(320, a.Bounds.Width, 0);
        w.Close();
    }

    [AvaloniaFact]
    public void Hiding_A_View_Slides_It_Off_The_Right_Edge()
    {
        var (pane, _, _, c, w) = Make();
        c.IsShowing = false;
        TestHost.Settle(w);
        Assert.Equal(1024 - 640, c.SlidePosition);
        Assert.False(c.IsVisible);
        c.IsShowing = true;
        TestHost.Settle(w);
        Assert.True(c.IsVisible);
        Assert.Equal(0, c.SlidePosition);
        w.Close();
    }

    [AvaloniaFact]
    public void Left_Drag_Selects_The_Dragged_View()
    {
        var (pane, _, b, _, w) = Make();
        var touch = new TouchInput(w);
        touch.Drag(new Point(800, 300), new Vector(-200, 0));
        Assert.True(pane.IsDragging);
        touch.Up(new Point(600, 300));
        TestHost.Settle(w);
        Assert.Equal(1, pane.SelectedIndex);
        Assert.Equal(-320, b.SlidePosition);
        w.Close();
    }

    [AvaloniaFact]
    public void One_Gesture_Pushes_Through_The_Stack()
    {
        var (pane, _, b, c, w) = Make();
        var touch = new TouchInput(w);
        touch.Drag(new Point(900, 300), new Vector(-800, 0), steps: 40);
        touch.Up(new Point(100, 300));
        TestHost.Settle(w);
        Assert.Equal(2, pane.SelectedIndex);
        Assert.Equal(-640, c.SlidePosition);
        Assert.Equal(-320, b.SlidePosition);
        w.Close();
    }

    [AvaloniaFact]
    public void Overslide_Moves_At_A_Quarter()
    {
        var (pane, _, b, _, w) = Make();
        var touch = new TouchInput(w);
        touch.Drag(new Point(400, 300), new Vector(100, 0), steps: 20);
        Assert.InRange(b.SlidePosition, 20, 26);
        touch.Up(new Point(500, 300));
        TestHost.Settle(w);
        Assert.Equal(0, b.SlidePosition);
        Assert.Equal(0, pane.SelectedIndex);
        w.Close();
    }

    [AvaloniaFact]
    public void Dismissible_View_Hides_Past_The_Dismiss_Distance()
    {
        var (pane, _, _, c, w) = Make(dismissible: true);
        var touch = new TouchInput(w);
        touch.Drag(new Point(700, 300), new Vector(150, 0), steps: 15);
        Assert.InRange(c.SlidePosition, 140, 150);
        touch.Up(new Point(850, 300));
        TestHost.Settle(w);
        Assert.False(c.IsShowing);
        w.Close();
    }

    [AvaloniaFact]
    public void Dismissing_The_Last_View_Leaves_The_Middle_View_Filling_To_The_Right_Edge()
    {
        // The gallery's SlidingPane page: 220 | 240 peek 60 | flex, dismissible.
        var a = new SlidingView { Width = 220, Content = new Border() };
        var b = new SlidingView { Width = 240, PeekWidth = 60, Content = new Border() };
        var c = new SlidingView { Dismissible = true, Content = new Border() };
        Flex.SetValue(c, 1);
        var pane = new SlidingPane { Views = { a, b, c } };
        var w = TestHost.Show(pane, 1024, 600);
        var touch = new TouchInput(w);
        touch.Drag(new Point(700, 300), new Vector(150, 0), steps: 15);
        touch.Up(new Point(850, 300));
        TestHost.Settle(w);
        Assert.False(c.IsShowing);
        var left = b.TranslatePoint(default, pane)!.Value.X;
        Assert.True(left + b.Bounds.Width >= 1023, $"middle view spans {left}..{left + b.Bounds.Width}, slide {b.SlidePosition}, fit {b.FitWidth}");

        // Showing the last view again gives the middle view its own width back.
        c.IsShowing = true;
        pane.SelectView(c);
        TestHost.Settle(w);
        Assert.Equal(240, b.Bounds.Width, 0);
        Assert.Equal(240, b.Width);
        Assert.Equal(1024, c.TranslatePoint(new Point(c.Bounds.Width, 0), pane)!.Value.X, 0);
        w.Close();
    }

    private static (SlidingPane Pane, SlidingView Middle, SlidingView Right, Window Window) MakeGalleryLayout(bool fitWhileSliding)
    {
        // The gallery's SlidingPane page: 220 | 240 peek 60 | flex, dismissible.
        var a = new SlidingView { Width = 220, Content = new Border() };
        var b = new SlidingView { Width = 240, PeekWidth = 60, Content = new Border() };
        var c = new SlidingView { Dismissible = true, Content = new Border() };
        Flex.SetValue(c, 1);
        var pane = new SlidingPane { FitWhileSliding = fitWhileSliding, Views = { a, b, c } };
        return (pane, b, c, TestHost.Show(pane, 1024, 600));
    }

    private static double RightEdge(SlidingView v, SlidingPane pane) => v.TranslatePoint(new Point(v.Bounds.Width, 0), pane)!.Value.X;

    [AvaloniaFact]
    public void Dragging_The_Last_View_Left_Keeps_It_Reaching_The_Right_Edge()
    {
        var (pane, _, c, w) = MakeGalleryLayout(fitWhileSliding: true);
        var touch = new TouchInput(w);
        touch.Drag(new Point(700, 300), new Vector(-300, 0), steps: 30);
        TestHost.Settle(w);
        Assert.True(c.SlidePosition < -200, $"slide {c.SlidePosition}");
        Assert.Equal(1024, RightEdge(c, pane), 0);
        touch.Up(new Point(400, 300));
        w.Close();
    }

    [AvaloniaFact]
    public void Dragging_The_Last_View_Right_Widens_The_View_It_Uncovers_Up_To_Its_Edge()
    {
        var (pane, b, c, w) = MakeGalleryLayout(fitWhileSliding: true);
        var touch = new TouchInput(w);
        touch.Drag(new Point(700, 300), new Vector(150, 0), steps: 15);
        TestHost.Settle(w);
        Assert.True(c.SlidePosition > 0);
        Assert.True(b.Bounds.Width > 240);
        Assert.Equal(c.TranslatePoint(default, pane)!.Value.X, RightEdge(b, pane), 0);

        // A short drag springs back, and the uncovered view gets its own width again.
        touch.Drag(new Point(850, 300), new Vector(-100, 0), steps: 10);
        touch.Up(new Point(750, 300));
        TestHost.Settle(w);
        Assert.True(c.IsShowing);
        Assert.Equal(240, b.Bounds.Width, 0);
        w.Close();
    }

    [AvaloniaFact]
    public void Without_FitWhileSliding_Views_Keep_Their_Width_Until_The_Slide_Completes()
    {
        var (pane, b, c, w) = MakeGalleryLayout(fitWhileSliding: false);
        var touch = new TouchInput(w);
        touch.Drag(new Point(700, 300), new Vector(-300, 0), steps: 30);
        TestHost.Settle(w);
        Assert.True(c.SlidePosition < -200, $"slide {c.SlidePosition}");
        Assert.Equal(1024 + c.SlidePosition, RightEdge(c, pane), 0);
        touch.Up(new Point(400, 300));
        TestHost.Settle(w);
        Assert.Equal(1024, RightEdge(c, pane), 0);
        w.Close();
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Fixed_Width_View_Fills_When_The_Next_View_Hides_And_Settles_Back(bool fitWhileSliding)
    {
        // The gallery's SlidingPane Many page: 300 | 320 fixed | flex.
        var a = new SlidingView { Width = 300, Content = new Border() };
        var b = new SlidingView { Width = 320, FixedWidth = true, Content = new Border() };
        var c = new SlidingView { Content = new Border() };
        Flex.SetValue(c, 1);
        var pane = new SlidingPane { FitWhileSliding = fitWhileSliding, Views = { a, b, c } };
        var w = TestHost.Show(pane, 1024, 600);
        pane.SelectView(c);
        TestHost.Settle(w);

        c.IsShowing = false;
        TestHost.Settle(w);
        if (fitWhileSliding)
        {
            Assert.Equal(1024, RightEdge(b, pane), 0);
        }
        else
        {
            Assert.Equal(320, b.Bounds.Width, 0);
        }

        c.IsShowing = true;
        pane.SelectView(c);
        TestHost.Settle(w);
        Assert.Equal(320, b.Bounds.Width, 0);
        Assert.Equal(1024, RightEdge(c, pane), 0);
        w.Close();
    }

    [AvaloniaFact]
    public void Showing_A_Hidden_View_Leaves_The_Views_Before_It_In_Place()
    {
        var old = Animations.EnyoAnimations.TimeScale;
        Animations.EnyoAnimations.TimeScale = 1;
        try
        {
            // Mail: folders 320 | messages 320 fixed | reader flex, dismissible, peek 320.
            var a = new SlidingView { Width = 320, Content = new Border() };
            var b = new SlidingView { Width = 320, FixedWidth = true, Content = new Border() };
            var c = new SlidingView { Dismissible = true, PeekWidth = 320, Content = new Border() };
            Flex.SetValue(c, 1);
            var pane = new SlidingPane { Views = { a, b, c } };
            var w = TestHost.Show(pane, 1024, 600);
            pane.SelectView(b, immediate: true);
            c.IsShowing = false;
            TestHost.Pump(1500, () => !c.IsVisible && !pane.IsSlideAnimating);
            Assert.Equal(-320, b.SlidePosition);

            c.IsShowing = true;
            pane.SelectView(c);
            double lowest = b.SlidePosition, highest = b.SlidePosition;
            var gaps = 0;
            TestHost.Pump(1500, () =>
            {
                lowest = System.Math.Min(lowest, b.SlidePosition);
                highest = System.Math.Max(highest, b.SlidePosition);
                w.UpdateLayout();
                if (RightEdge(b, pane) < System.Math.Min(1024, c.TranslatePoint(default, pane)!.Value.X) - 1)
                {
                    gaps++;
                }

                return !pane.IsSlideAnimating && c.SlidePosition == c.CalcSlideMin();
            });
            Assert.Equal(-320, lowest);
            Assert.Equal(-320, highest);
            Assert.Equal(0, gaps);
            Assert.Equal(-320, c.SlidePosition);
            w.Close();
        }
        finally
        {
            Animations.EnyoAnimations.TimeScale = old;
        }
    }

    [AvaloniaFact]
    public void Going_Back_Keeps_The_Views_Together_While_They_Slide()
    {
        var old = Animations.EnyoAnimations.TimeScale;
        Animations.EnyoAnimations.TimeScale = 1;
        try
        {
            var a = new SlidingView { Width = 320, Content = new Border() };
            var b = new SlidingView { Width = 320, Content = new Border() };
            var c = new SlidingView { PeekWidth = 320, Content = new Border() };
            Flex.SetValue(c, 1);
            var pane = new SlidingPane { Views = { a, b, c } };
            var w = TestHost.Show(pane, 1024, 600);
            pane.SelectView(c, immediate: true);
            Assert.Equal(-320, b.SlidePosition);

            pane.SelectView(a);
            var midFrames = 0;
            var apart = 0;
            TestHost.Pump(1500, () =>
            {
                if (c.SlidePosition is > -320 and < 0)
                {
                    midFrames++;
                    if (b.SlidePosition != c.SlidePosition)
                    {
                        apart++;
                    }
                }

                return !pane.IsSlideAnimating && c.SlidePosition == 0;
            });
            Assert.True(midFrames > 0);
            Assert.Equal(0, apart);
            Assert.Equal(0, b.SlidePosition);
            w.Close();
        }
        finally
        {
            Animations.EnyoAnimations.TimeScale = old;
        }
    }

    [AvaloniaFact]
    public void Short_Dismissible_Drag_Springs_Back()
    {
        var (pane, _, _, c, w) = Make(dismissible: true);
        var touch = new TouchInput(w);
        touch.Drag(new Point(700, 300), new Vector(60, 0), steps: 6);
        touch.Up(new Point(760, 300));
        TestHost.Settle(w);
        Assert.True(c.IsShowing);
        Assert.Equal(0, c.SlidePosition);
        w.Close();
    }

    [AvaloniaFact]
    public void Back_Selects_The_Previous_View()
    {
        var (pane, _, _, _, w) = Make();
        pane.SelectViewByIndex(2);
        Assert.True(EnyoBack.RequestBack(w));
        Assert.Equal(1, pane.SelectedIndex);
        Assert.True(EnyoBack.RequestBack(w));
        Assert.Equal(0, pane.SelectedIndex);
        Assert.False(EnyoBack.RequestBack(w));
        w.Close();
    }

    [AvaloniaFact]
    public void Tapping_A_Handle_Toggles_The_View()
    {
        var grab = new GrabButton();
        var b = new SlidingView { Width = 320, Content = new StackPanel { Children = { grab } } };
        var pane = new SlidingPane { Views = { new SlidingView { Width = 320 }, b, new SlidingView() } };
        var w = TestHost.Show(pane, 1024, 600);
        grab.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, grab));
        Assert.Equal(1, pane.SelectedIndex);
        grab.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, grab));
        Assert.Equal(0, pane.SelectedIndex);
        w.Close();
    }

    [AvaloniaFact]
    public void Vertical_Drags_Are_Left_To_A_Scroller()
    {
        var list = new StackPanel();
        for (var i = 0; i < 50; i++)
        {
            list.Children.Add(new TextBlock { Text = "Row " + i, Height = 40 });
        }

        var scroller = new ScrollViewer { Content = list };
        var c = new SlidingView { Content = scroller };
        Flex.SetValue(c, 1);
        var pane = new SlidingPane { Views = { new SlidingView { Width = 320 }, new SlidingView { Width = 320 }, c } };
        var w = TestHost.Show(pane, 1024, 600);
        var touch = new TouchInput(w);
        touch.Drag(new Point(800, 400), new Vector(0, -200));
        touch.Up(new Point(800, 200));
        TestHost.Settle(w);
        Assert.Equal(0, pane.SelectedIndex);
        Assert.True(scroller.Offset.Y > 150, $"offset {scroller.Offset.Y}");
        touch.Drag(new Point(800, 300), new Vector(-300, 0));
        touch.Up(new Point(500, 300));
        TestHost.Settle(w);
        Assert.Equal(1, pane.SelectedIndex);
        w.Close();
    }

    [AvaloniaFact]
    public void A_List_Filled_Off_Screen_Realizes_Its_Rows_When_Its_View_Slides_In()
    {
        var list = new VirtualList();
        var b = new SlidingView { Width = 320, Content = list };
        var pane = new SlidingPane { Views = { new SlidingView { Width = 320 }, b, new SlidingView() } };
        var w = TestHost.Show(new Border { ClipToBounds = true, Child = pane }, 400, 600);
        Assert.True(pane.IsNarrow);
        list.ItemsSource = Enumerable.Range(0, 100).Select(i => "Row " + i).ToList();
        TestHost.Settle(w);
        pane.SelectView(b);
        TestHost.Pump(1000, () => !pane.IsSlideAnimating);
        TestHost.Pump(100);
        Assert.True(list.GetRealizedContainers().Count() > 5, $"realized {list.GetRealizedContainers().Count()}");
        w.Close();
    }
}
