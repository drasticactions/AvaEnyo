using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace AvaEnyo.Theme.Tests;

public class InputBehaviorTests
{
    private static Window Host(Control control)
    {
        var window = ThemeTestHelpers.Host(new Canvas { Children = { control } }, "Light", EnyoDensity.Default, 600, 300);
        Canvas.SetLeft(control, 50);
        Canvas.SetTop(control, 50);
        window.UpdateLayout();
        return window;
    }

    private static Point Center(Visual v, Window w) => v.TranslatePoint(new Point(v.Bounds.Width / 2, v.Bounds.Height / 2), w)!.Value;

    private static void Pump()
    {
        Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public void CheckBox_Toggles_On_Press()
    {
        var box = new CheckBox { Content = "x" };
        var window = Host(box);
        var p = Center(box, window);
        window.MouseDown(p, MouseButton.Left);
        Assert.True(box.IsChecked);
        window.MouseUp(p, MouseButton.Left);
        Assert.True(box.IsChecked);
        window.Close();
    }

    [AvaloniaFact]
    public void CheckBox_Toggles_On_Release_When_The_Switch_Is_Off()
    {
        ThemeSettings.CheckBoxTogglesOnPress = false;
        try
        {
            var box = new CheckBox { Content = "x" };
            var window = Host(box);
            var p = Center(box, window);
            window.MouseDown(p, MouseButton.Left);
            Assert.False(box.IsChecked);
            window.MouseUp(p, MouseButton.Left);
            Assert.True(box.IsChecked);
            window.Close();
        }
        finally
        {
            ThemeSettings.CheckBoxTogglesOnPress = true;
        }
    }

    [AvaloniaFact]
    public void ToggleSwitch_Tap_Toggles()
    {
        var toggle = new ToggleSwitch();
        var window = Host(toggle);
        var p = Center(toggle, window);
        window.MouseDown(p, MouseButton.Left);
        window.MouseUp(p, MouseButton.Left);
        Assert.True(toggle.IsChecked);
        window.Close();
    }

    [AvaloniaFact]
    public void ToggleSwitch_Drag_Over_15px_Sets_The_Direction_And_Does_Not_Click()
    {
        var toggle = new ToggleSwitch();
        var window = Host(toggle);
        var p = Center(toggle, window);
        window.MouseDown(p, MouseButton.Left);
        for (var i = 1; i <= 10; i++)
        {
            window.MouseMove(p + new Vector(i * 3, 0));
            Pump();
        }

        Assert.True(toggle.IsChecked);
        window.MouseUp(p + new Vector(30, 0), MouseButton.Left);
        Assert.True(toggle.IsChecked);

        // A leftward drag switches it off again.
        window.MouseDown(p, MouseButton.Left);
        for (var i = 1; i <= 10; i++)
        {
            window.MouseMove(p - new Vector(i * 3, 0));
            Pump();
        }

        window.MouseUp(p - new Vector(30, 0), MouseButton.Left);
        Assert.False(toggle.IsChecked);
        window.Close();
    }

    [AvaloniaFact]
    public void ToggleSwitch_Short_Drag_Does_Not_Switch_Or_Click()
    {
        var toggle = new ToggleSwitch();
        var window = Host(toggle);
        var p = Center(toggle, window);
        window.MouseDown(p, MouseButton.Left);
        window.MouseMove(p + new Vector(6, 0));
        window.MouseMove(p + new Vector(10, 0));
        window.MouseUp(p + new Vector(10, 0), MouseButton.Left);
        Assert.False(toggle.IsChecked);
        window.Close();
    }

    [AvaloniaFact]
    public void ToggleSwitch_Labels_Are_Uppercase()
    {
        var toggle = new ToggleSwitch();
        var window = Host(toggle);
        var texts = toggle.GetVisualDescendants().OfType<TextBlock>().Select(t => t.Text).ToList();
        Assert.Contains("OFF", texts);
        Assert.Equal("Off", toggle.OffContent);
        window.Close();
    }

    [AvaloniaFact]
    public void Search_Clear_Button_Clears_The_Text()
    {
        var box = new TextBox { Classes = { "search" }, Text = "abc", Width = 200 };
        var window = Host(box);
        var clear = box.GetVisualDescendants().OfType<Button>().First(b => b.Name == "PART_ClearButton");
        Assert.NotNull(clear.Command);
        clear.Command!.Execute(clear.CommandParameter);
        Dispatcher.UIThread.RunJobs();
        Assert.True(string.IsNullOrEmpty(box.Text));
        window.UpdateLayout();
        Assert.DoesNotContain(box.GetVisualDescendants().OfType<Button>(), b => b.Name == "PART_ClearButton");
        window.Close();
    }

    [AvaloniaFact]
    public void Slider_Track_Tap_Moves_The_Value()
    {
        var slider = new Slider { Width = 300, Minimum = 0, Maximum = 100, Value = 0 };
        var window = Host(slider);
        var track = slider.GetVisualDescendants().OfType<Avalonia.Controls.Primitives.Track>().First();
        var thumb = track.Thumb!;
        var usable = track.Bounds.Width - thumb.Bounds.Width;
        var local = new Point((thumb.Bounds.Width / 2) + (usable * 0.75), track.Bounds.Height / 2);
        var p = track.TranslatePoint(local, window)!.Value;
        window.MouseDown(p, MouseButton.Left);
        window.MouseUp(p, MouseButton.Left);
        Assert.InRange(slider.Value, 74, 76);
        window.Close();
    }
}
