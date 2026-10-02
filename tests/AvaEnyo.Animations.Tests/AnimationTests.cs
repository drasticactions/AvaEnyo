using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media.Transformation;
using Avalonia.Threading;
using Xunit;

namespace AvaEnyo.Animations.Tests;

public class AnimationTests
{
    private static Window Host(Control c)
    {
        var w = new Window { Content = new Canvas { Children = { c } }, Width = 400, Height = 300 };
        w.Show();
        return w;
    }

    [AvaloniaFact]
    public async Task TimeScale_Zero_Jumps_And_Leaves_Clean_End_States()
    {
        var b = new Border { Width = 50, Height = 50 };
        Host(b);
        Assert.True(await EnyoAnimations.FadeIn(b));
        Assert.False(b.IsSet(Visual.OpacityProperty));
        Assert.True(await EnyoAnimations.SlideTo(b, 0));
        Assert.Null(b.RenderTransform);
        Assert.True(await EnyoAnimations.SlideTo(b, 30));
        Assert.Equal(30, ((TransformOperations)b.RenderTransform!).Value.M31);
        Assert.True(await EnyoAnimations.FadeOut(b));
        Assert.Equal(0, b.Opacity);
    }

    [AvaloniaFact]
    public async Task Animate_Calls_Back_With_The_End_Value()
    {
        double last = -1;
        Assert.True(await EnyoAnimations.Animate(0, 10, TimeSpan.FromMilliseconds(350), EnyoEasing.CubicOut, v => last = v));
        Assert.Equal(10, last);
    }

    [AvaloniaFact]
    public void Supersede_Starts_From_The_On_Screen_Value()
    {
        var old = EnyoAnimations.TimeScale;
        EnyoAnimations.TimeScale = 1;
        try
        {
            var b = new Border { Width = 50, Height = 50 };
            var w = Host(b);
            var first = EnyoAnimations.FadeOut(b, 1000);
            // Advance part way.
            for (var i = 0; i < 10; i++)
            {
                Avalonia.Headless.AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                Dispatcher.UIThread.RunJobs();
            }

            var second = EnyoAnimations.FadeIn(b, 1000);
            Dispatcher.UIThread.RunJobs();
            Assert.True(first.IsCompleted);
            Assert.False(first.Result);
            Assert.False(second.IsCompleted);
            w.Close();
        }
        finally
        {
            EnyoAnimations.TimeScale = old;
        }
    }

    [AvaloniaFact]
    public async Task Pane_Transitions_Hide_The_Outgoing_View()
    {
        var a = new Border();
        var b = new Border { IsVisible = false };
        var panel = new Panel { Children = { a, b } };
        var w = new Window { Content = panel, Width = 200, Height = 100 };
        w.Show();
        await EnyoPaneTransitions.Fade.Start(a, b, true, default);
        Assert.False(a.IsVisible);
        Assert.True(b.IsVisible);
        Assert.False(b.IsSet(Visual.OpacityProperty));
        await EnyoPaneTransitions.FlyIn.Start(b, a, true, default);
        Assert.False(b.IsVisible);
        Assert.True(a.IsVisible);
        Assert.Null(a.RenderTransform);
        await EnyoPaneTransitions.None.Start(a, b, true, default);
        Assert.True(b.IsVisible);
    }

    [AvaloniaFact]
    public async Task Height_Animations_End_Auto_And_Collapsed()
    {
        var b = new Border { Child = new Border { Height = 80 } };
        var w = new Window { Content = new StackPanel { Children = { b } }, Width = 200, Height = 300 };
        w.Show();
        Assert.True(await EnyoAnimations.CollapseHeight(b));
        Assert.False(b.IsVisible);
        Assert.True(await EnyoAnimations.ExpandHeight(b));
        Assert.True(b.IsVisible);
        Assert.True(double.IsNaN(b.Height));
    }

    [AvaloniaFact]
    public void Spin_Runs_At_Real_Time_Scale()
    {
        var old = EnyoAnimations.TimeScale;
        EnyoAnimations.TimeScale = 1;
        try
        {
            var b = new Border { Width = 20, Height = 20 };
            var w = Host(b);
            var spin = EnyoAnimations.Spin(b);
            for (var i = 0; i < 10; i++)
            {
                Avalonia.Headless.AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                Dispatcher.UIThread.RunJobs();
            }

            Assert.IsType<TransformOperations>(b.RenderTransform);
            spin.Dispose();
            Assert.Null(b.RenderTransform);
            w.Close();
        }
        finally
        {
            EnyoAnimations.TimeScale = old;
        }
    }

    [AvaloniaFact]
    public void Spin_Disposes_Cleanly()
    {
        var b = new Border();
        Host(b);
        var spin = EnyoAnimations.Spin(b);
        Assert.NotNull(b.RenderTransform);
        spin.Dispose();
        Assert.Null(b.RenderTransform);
    }
}
