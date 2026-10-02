using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using AvaEnyo.Primitives;
using Xunit;

namespace AvaEnyo.Controls.Tests;

public class ScrollTests
{
    private static (ScrollViewer Viewer, EnyoScrollContentPresenter Presenter, Window Window) Make()
    {
        var list = new StackPanel();
        for (var i = 0; i < 100; i++)
        {
            list.Children.Add(new Border { Height = 40, Background = Avalonia.Media.Brushes.Transparent });
        }

        var viewer = new ScrollViewer { Content = list };
        var w = TestHost.Show(viewer, 300, 400);
        var presenter = viewer.GetVisualDescendants().OfType<EnyoScrollContentPresenter>().Single();
        return (viewer, presenter, w);
    }

    [AvaloniaFact]
    public void Drag_Moves_The_Content_One_To_One()
    {
        var (viewer, presenter, w) = Make();
        var touch = new TouchInput(w);
        touch.Drag(new Point(100, 300), new Vector(0, -150));
        Assert.True(presenter.IsDragging);
        Assert.Equal(150 - 0, viewer.Offset.Y, 0);
        touch.Up(new Point(100, 150));
        w.Close();
    }

    [AvaloniaFact]
    public void Drag_Past_The_Top_Is_Damped_And_Springs_Back()
    {
        var (viewer, presenter, w) = Make();
        var touch = new TouchInput(w);
        touch.Drag(new Point(100, 100), new Vector(0, 100));
        Assert.Equal(0, viewer.Offset.Y);
        Assert.InRange(presenter.Overscroll.Y, -50, -45);
        touch.Up(new Point(100, 200));
        TestHost.Settle(w);
        Assert.Equal(0, presenter.Overscroll.Y);
        Assert.Equal(0, viewer.Offset.Y);
        w.Close();
    }

    [AvaloniaFact]
    public void Release_Flings_Further()
    {
        var (viewer, _, w) = Make();
        var touch = new TouchInput(w) { StepMs = 10 };
        touch.Drag(new Point(100, 350), new Vector(0, -200), steps: 10);
        touch.Up(new Point(100, 150));
        TestHost.Settle(w);
        Assert.True(viewer.Offset.Y > 300, $"offset {viewer.Offset.Y}");
        w.Close();
    }

    [AvaloniaFact]
    public void Horizontal_Drag_Is_Not_Taken_By_A_Vertical_Scroller()
    {
        var (viewer, presenter, w) = Make();
        var touch = new TouchInput(w);
        touch.Drag(new Point(50, 200), new Vector(120, 0));
        Assert.False(presenter.IsDragging);
        Assert.Equal(0, viewer.Offset.Y);
        touch.Up(new Point(170, 200));
        w.Close();
    }

    [AvaloniaFact]
    public void Wheel_Scrolls_At_Once()
    {
        var (viewer, _, w) = Make();
        Avalonia.Headless.HeadlessWindowExtensions.MouseWheel(w, new Point(100, 100), new Vector(0, -1));
        Assert.True(viewer.Offset.Y > 0);
        w.Close();
    }

    [AvaloniaFact]
    public void ScrollToAsync_Lands_On_The_Target()
    {
        var (viewer, _, w) = Make();
        var task = viewer.ScrollToAsync(new Vector(0, 1000));
        TestHost.Settle(w);
        Assert.True(task.IsCompleted);
        Assert.Equal(1000, viewer.Offset.Y, 0);
        w.Close();
    }

    [AvaloniaFact]
    public void Fades_Follow_The_Scroll_Position()
    {
        var (viewer, _, w) = Make();
        Assert.Contains(":scroll-down", viewer.Classes);
        Assert.DoesNotContain(":scroll-up", viewer.Classes);
        Assert.Contains(":fades", viewer.Classes);
        viewer.Offset = new Vector(0, 200);
        TestHost.Settle(w);
        Assert.Contains(":scroll-up", viewer.Classes);
        w.Close();
    }

    [AvaloniaFact]
    public void Native_Physics_Turns_Dragging_Off()
    {
        var (viewer, presenter, w) = Make();
        Scroll.SetPhysics(viewer, ScrollPhysicsMode.Native);
        var touch = new TouchInput(w);
        touch.Drag(new Point(100, 300), new Vector(0, -150));
        Assert.False(presenter.IsDragging);
        touch.Up(new Point(100, 150));
        w.Close();
    }
}
