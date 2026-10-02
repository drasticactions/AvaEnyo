using System;
using Xunit;

namespace AvaEnyo.Animations.Tests;

public class ScrollPhysicsTests
{
    private static ScrollPhysics Make(double max = 1000) => new() { Min = 0, Max = max };

    [Fact]
    public void Friction_Keeps_097_Of_The_Velocity_Per_Step()
    {
        var p = Make();
        p.SetPosition(100);
        p.PreviousPosition = 90; // 10 px per step
        p.Step(TimeSpan.FromMilliseconds(10));
        Assert.Equal(10 * 0.97, p.Velocity, 9);
        p.Step(TimeSpan.FromMilliseconds(10));
        Assert.Equal(10 * 0.97 * 0.97, p.Velocity, 9);
    }

    [Fact]
    public void Motion_Stops_Below_The_Friction_Epsilon()
    {
        var p = Make();
        p.SetPosition(100);
        p.PreviousPosition = 99.995;
        Assert.False(p.Step(TimeSpan.FromMilliseconds(10)));
        Assert.Equal(0, p.Velocity);
    }

    [Fact]
    public void Drag_Past_The_Edge_Is_Damped_By_Half()
    {
        var p = Make();
        p.SetPosition(0);
        p.BeginDrag();
        p.Drag(-40);
        Assert.Equal(-20, p.Position, 9);
        p.Drag(30);
        Assert.Equal(30, p.Position, 9);
    }

    [Fact]
    public void Overscroll_Spring_Returns_Within_Half_A_Pixel()
    {
        var p = Make();
        p.SetPosition(-60);
        var steps = 0;
        while (p.Step(TimeSpan.FromMilliseconds(10)) && steps < 10_000)
        {
            steps++;
        }

        Assert.Equal(0, p.Position, 9);
        Assert.False(p.IsOverscrolled);
        Assert.InRange(steps, 1, 2000);
    }

    [Fact]
    public void Spring_Keeps_093_Of_The_Overscroll_Per_Step()
    {
        var p = Make();
        p.SetPosition(-100);
        p.Step(TimeSpan.FromMilliseconds(10));
        // Constrain: -100 → -93, with no velocity (snap friction on zero velocity).
        Assert.Equal(-93, p.Position, 9);
    }

    [Theory]
    [InlineData(250.0)]
    [InlineData(-50.0)]
    [InlineData(999.0)]
    public void ScrollTo_Lands_On_The_Target(double target)
    {
        var p = Make();
        p.SetPosition(400);
        var t = Math.Clamp(target, 0, 1000);
        p.ScrollTo(t);
        var end = p.Settle();
        Assert.InRange(end, t - 1, t + 1);
    }

    [Fact]
    public void Same_End_State_At_Any_Frame_Rate()
    {
        double Run(double frameMs)
        {
            var p = Make(5000);
            p.SetPosition(100);
            p.BeginDrag();
            p.EndDrag(2.0); // 2 px/ms
            var total = 0.0;
            while (total < 3000)
            {
                p.Step(TimeSpan.FromMilliseconds(frameMs));
                total += frameMs;
            }

            return p.Position;
        }

        var a = Run(8);
        var b = Run(16);
        var c = Run(33);
        Assert.Equal(a, b, 6);
        Assert.Equal(a, c, 6);
        Assert.True(a > 100);
    }

    [Fact]
    public void Release_Velocity_Maps_Through_The_Flick_Scalar()
    {
        var p = Make();
        p.SetPosition(100);
        p.BeginDrag();
        p.EndDrag(1.5); // 1.5 px/ms = 1500 px/s → 15 px per step
        Assert.Equal(15, p.Velocity, 9);
    }

    [Fact]
    public void Fling_Into_The_End_Springs_Back_To_Max()
    {
        var p = Make(200);
        p.SetPosition(150);
        p.BeginDrag();
        p.EndDrag(3);
        Assert.Equal(200, p.Settle(), 9);
    }

    [Fact]
    public void Snap_Physics_Lands_Exactly_On_Target()
    {
        var p = SnapPhysics.ForCarousel();
        p.Min = 0;
        p.Max = 3000;
        p.SetPosition(1000);
        p.SnapTo(2000);
        var steps = 0;
        while (p.Step(TimeSpan.FromMilliseconds(16)) && steps++ < 1000)
        {
        }

        Assert.Equal(2000, p.Position);
    }

    [Fact]
    public void Velocity_Tracker_Fits_A_Line()
    {
        var v = new VelocityTracker();
        for (var t = 0; t <= 200; t += 10)
        {
            v.Add(t, t * 2.0, -t * 0.5);
        }

        var (x, y) = v.GetVelocity();
        Assert.Equal(2.0, x, 6);
        Assert.Equal(-0.5, y, 6);
    }
}
