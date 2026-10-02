using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Xunit;

namespace AvaEnyo.Services.Tests;

public class LifecycleTests
{
    [AvaloniaFact]
    public void Attach_Returns_One_Instance_Per_Window()
    {
        var w = Host.Show(new Canvas());
        Assert.Same(ApplicationEvents.Attach(w), ApplicationEvents.Attach(w));
        w.Close();
    }

    [AvaloniaFact]
    public void Activation_Follows_The_Window()
    {
        var w = Host.Show(new Canvas());
        var events = ApplicationEvents.Attach(w);
        var activated = 0;
        var deactivated = 0;
        events.Activated += (_, _) => activated++;
        events.Deactivated += (_, _) => deactivated++;
        w.Activate();
        Host.Settle(w);
        Assert.True(activated >= 1, $"activated {activated}");

        w.Close();
    }

    [AvaloniaFact]
    public void Unhandled_Back_Reaches_The_App()
    {
        var w = Host.Show(new Canvas());
        var events = ApplicationEvents.Attach(w);
        var backs = 0;
        events.BackRequested += (_, _) => backs++;
        EnyoBack.RequestBack(w);
        Assert.Equal(1, backs);
        w.Close();
    }

    [AvaloniaFact]
    public void Rotation_And_Minimize()
    {
        var w = Host.Show(new Canvas(), 800, 600);
        var events = ApplicationEvents.Attach(w);
        RotatedEventArgs? rotated = null;
        var hidden = 0;
        var shown = 0;
        events.Rotated += (_, e) => rotated = e;
        events.Hidden += (_, _) => hidden++;
        events.Shown += (_, _) => shown++;
        w.Width = 500;
        w.Height = 900;
        Host.Settle(w);
        Assert.Equal(DisplayOrientation.Portrait, rotated!.Orientation);
        w.WindowState = WindowState.Minimized;
        w.WindowState = WindowState.Normal;
        Assert.Equal(1, hidden);
        Assert.Equal(1, shown);
        w.Close();
    }

    [AvaloniaFact]
    public void Keys_And_Relaunch()
    {
        var w = Host.Show(new Canvas());
        var events = ApplicationEvents.Attach(w);
        Key? down = null;
        Key? up = null;
        object? relaunch = null;
        object? param = null;
        events.KeyDown += (_, e) => down = e.Key;
        events.KeyUp += (_, e) => up = e.Key;
        events.Relaunched += (_, e) => relaunch = e.Parameters;
        events.WindowParamsChanged += (_, e) => param = e.Parameters;
        w.KeyPressQwerty(PhysicalKey.A, RawInputModifiers.None);
        w.KeyReleaseQwerty(PhysicalKey.A, RawInputModifiers.None);
        events.Relaunch("args");
        Assert.Equal(Key.A, down);
        Assert.Equal(Key.A, up);
        Assert.Equal("args", relaunch);
        Assert.Equal("args", param);
        events.Detach();
        down = null;
        w.KeyPressQwerty(PhysicalKey.B, RawInputModifiers.None);
        Assert.Null(down);
        w.Close();
    }

    [Fact]
    public void Test_Double_Raises_Events()
    {
        var events = new TestApplicationEvents();
        KeyboardEventArgs? shown = null;
        events.KeyboardShown += (_, e) => shown = e;
        events.RaiseKeyboard(true, new Rect(0, 400, 800, 200));
        Assert.Equal(400, shown!.OccludedRect.Y);
        Assert.Equal(DisplayOrientation.Portrait, ApplicationEvents.OrientationOf(new Size(300, 600)));
    }
}
