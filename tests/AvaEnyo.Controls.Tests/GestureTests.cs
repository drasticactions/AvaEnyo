using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using AvaEnyo.Controls.Primitives;
using Xunit;

namespace AvaEnyo.Controls.Tests;

public class GestureTests
{
    private static void Pump(Func<bool> until, int ms = 1000)
    {
        TestHost.Pump(ms, until);
    }

    [AvaloniaFact]
    public void Defaults_Match_Enyo()
    {
        Assert.Equal(200, EnyoGestures.HoldDelay);
        Assert.Equal(100, EnyoGestures.PulseInterval);
        Assert.Equal(4, EnyoGestures.DragThreshold);
    }

    [AvaloniaFact]
    public void Hold_Then_Pulse()
    {
        var oldHold = EnyoGestures.HoldDelay;
        var oldPulse = EnyoGestures.PulseInterval;
        EnyoGestures.HoldDelay = 20;
        EnyoGestures.PulseInterval = 10;
        try
        {
            var target = new Border { Background = Avalonia.Media.Brushes.Red, Width = 100, Height = 100 };
            var holds = 0;
            var pulses = 0;
            EnyoGestures.AddHoldHandler(target, (_, _) => holds++);
            EnyoGestures.AddHoldPulseHandler(target, (_, e) => pulses++);
            var w = TestHost.Show(new Canvas { Children = { target } });
            var touch = new TouchInput(w);
            touch.Down(new Point(50, 50));
            Pump(() => pulses >= 2);
            Assert.Equal(1, holds);
            Assert.True(pulses >= 2);
            touch.Up(new Point(50, 50));
            var after = pulses;
            Pump(() => false, 60);
            Assert.Equal(after, pulses);
            w.Close();
        }
        finally
        {
            EnyoGestures.HoldDelay = oldHold;
            EnyoGestures.PulseInterval = oldPulse;
        }
    }

    [AvaloniaFact]
    public void Drag_Start_After_4px_Cancels_Hold()
    {
        var oldHold = EnyoGestures.HoldDelay;
        EnyoGestures.HoldDelay = 30;
        try
        {
            var target = new Border { Background = Avalonia.Media.Brushes.Red, Width = 100, Height = 100 };
            var holds = 0;
            DragStartedEventArgs? drag = null;
            EnyoGestures.AddHoldHandler(target, (_, _) => holds++);
            EnyoGestures.AddDragStartedHandler(target, (_, e) => drag = e);
            var w = TestHost.Show(new Canvas { Children = { target } });
            var touch = new TouchInput(w);
            touch.Down(new Point(50, 50));
            touch.Move(new Point(52, 50));
            Assert.Null(drag);
            touch.Move(new Point(55, 51));
            Assert.NotNull(drag);
            Assert.True(drag!.IsHorizontal);
            Assert.True(drag.IsLockable);
            Pump(() => false, 80);
            Assert.Equal(0, holds);
            touch.Up(new Point(55, 51));
            w.Close();
        }
        finally
        {
            EnyoGestures.HoldDelay = oldHold;
        }
    }

    [AvaloniaFact]
    public void Fast_Release_Flicks()
    {
        var target = new Border { Background = Avalonia.Media.Brushes.Red, Width = 300, Height = 100 };
        FlickEventArgs? flick = null;
        EnyoGestures.AddFlickHandler(target, (_, e) => flick = e);
        var w = TestHost.Show(new Canvas { Children = { target } });
        var touch = new TouchInput(w) { StepMs = 10 };
        touch.Down(new Point(10, 50));
        touch.MoveBy(new Point(10, 50), new Vector(200, 0), 5);
        touch.Up(new Point(210, 50));
        Assert.NotNull(flick);
        Assert.True(flick!.XVelocity > 1);
        w.Close();
    }
}
