using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AvaEnyo.Controls.Primitives;
using Xunit;

namespace AvaEnyo.Controls.Tests;

public class PopupTests
{
    private static (Popup Popup, Window Window, Button Anchor) Make(object? content = null)
    {
        var anchor = new Button { Content = "Anchor", Margin = new Thickness(100, 100, 0, 0) };
        var popup = new Popup { Content = content ?? new TextBlock { Text = "Hello", Width = 100, Height = 50 } };
        var root = new Canvas { Children = { anchor, popup } };
        var w = TestHost.Show(root, 800, 600);
        return (popup, w, anchor);
    }

    [AvaloniaFact]
    public void Placeholder_Has_No_Size_And_Presenter_Goes_To_The_Overlay()
    {
        var (popup, w, _) = Make();
        Assert.Equal(default, popup.Bounds.Size);
        popup.OpenAtCenter();
        TestHost.Settle(w);
        Assert.True(popup.IsOpen);
        Assert.NotNull(popup.Presenter);
        Assert.True(popup.Presenter!.IsAttachedToVisualTree());
        Assert.NotNull(popup.Presenter.FindAncestorOfType<Avalonia.Controls.Primitives.OverlayLayer>());
        w.Close();
    }

    [AvaloniaFact]
    public void OpenAt_Is_Clamped_Inside_The_Window()
    {
        var (popup, w, _) = Make();
        popup.OpenAt(new Point(780, 590));
        TestHost.Settle(w);
        var p = popup.Presenter!;
        var pos = p.TranslatePoint(default, w)!.Value;
        Assert.True(pos.X + p.Bounds.Width <= 800.5, $"x {pos.X} w {p.Bounds.Width}");
        Assert.True(pos.Y + p.Bounds.Height <= 600.5);
        Assert.True(p.Bounds.Width >= 300);
        w.Close();
    }

    [AvaloniaFact]
    public void OpenAroundControl_Places_Below_The_Anchor()
    {
        var (popup, w, anchor) = Make();
        popup.OpenAroundControl(anchor, PopupAlignment.Below);
        TestHost.Settle(w);
        var a = anchor.TranslatePoint(new Point(0, anchor.Bounds.Height), w)!.Value;
        var pos = popup.Presenter!.TranslatePoint(default, w)!.Value;
        Assert.Equal(a.Y, pos.Y, 1);
        Assert.Equal(a.X, pos.X, 1);
        w.Close();
    }

    [AvaloniaFact]
    public void Outside_Click_Closes_With_Reason_Click()
    {
        var (popup, w, _) = Make();
        PopupClosedEventArgs? closed = null;
        popup.Closed += (_, e) => closed = e;
        popup.OpenAt(new Point(400, 300));
        TestHost.Settle(w);
        w.MouseDown(new Point(10, 10), MouseButton.Left);
        w.MouseUp(new Point(10, 10), MouseButton.Left);
        TestHost.Settle(w);
        Assert.False(popup.IsOpen);
        Assert.Equal(PopupCloseReason.Click, closed!.Reason);
        w.Close();
    }

    [AvaloniaFact]
    public void Modal_Popup_Ignores_Outside_Clicks_And_Shows_The_Scrim()
    {
        var (popup, w, anchor) = Make();
        var clicked = false;
        anchor.Click += (_, _) => clicked = true;
        popup.IsModal = true;
        popup.OpenAtCenter();
        TestHost.Settle(w);
        var host = OverlayHost.Existing(w)!;
        Assert.True(host.SharedScrim.IsOpen);
        Assert.Equal(popup.Presenter!.ZIndex - 1, host.SharedScrim.ZIndex);
        var a = anchor.TranslatePoint(new Point(5, 5), w)!.Value;
        w.MouseDown(a, MouseButton.Left);
        w.MouseUp(a, MouseButton.Left);
        Assert.True(popup.IsOpen);
        Assert.False(clicked);
        w.Close();
    }

    [AvaloniaFact]
    public void Escape_Closes_The_Topmost_Popup_First()
    {
        var (first, w, _) = Make();
        var second = new Popup { Content = new TextBlock { Text = "Two" } };
        ((Canvas)w.Content!).Children.Add(second);
        first.OpenAtCenter();
        second.OpenAt(new Point(10, 10));
        TestHost.Settle(w);
        var host = OverlayHost.Existing(w)!;
        Assert.Equal(new[] { first, second }, host.Stack.ToArray());
        Assert.True(second.Presenter!.ZIndex > first.Presenter!.ZIndex);
        PopupCloseReason? reason = null;
        second.Closed += (_, e) => reason = e.Reason;
        w.KeyPressQwerty(PhysicalKey.Escape, RawInputModifiers.None);
        Assert.False(second.IsOpen);
        Assert.True(first.IsOpen);
        Assert.Equal(PopupCloseReason.Escape, reason);
        Assert.True(EnyoBack.RequestBack(w, BackSource.System));
        Assert.False(first.IsOpen);
        w.Close();
    }

    [AvaloniaFact]
    public void CloseValue_Button_Closes_With_Handle()
    {
        var ok = new Button { Content = "OK" };
        Popup.SetCloseValue(ok, "ok");
        var (popup, w, _) = Make(new StackPanel { Children = { ok } });
        PopupClosedEventArgs? closed = null;
        popup.Closed += (_, e) => closed = e;
        popup.OpenAtCenter();
        TestHost.Settle(w);
        ok.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, ok));
        Assert.Equal(PopupCloseReason.Handle, closed!.Reason);
        Assert.Equal("ok", closed.Value);
        w.Close();
    }

    [AvaloniaFact]
    public void Focus_Moves_In_And_Returns()
    {
        var box = new TextBox();
        var (popup, w, anchor) = Make(new StackPanel { Children = { box } });
        anchor.Focus();
        popup.OpenAtCenter();
        TestHost.Settle(w);
        Assert.True(box.IsFocused);
        popup.Close();
        TestHost.Settle(w);
        Assert.True(anchor.IsFocused);
        w.Close();
    }

    [AvaloniaFact]
    public void Lazy_Content_Builds_On_First_Open()
    {
        var built = 0;
        var popup = new Popup { ContentTemplate = new Avalonia.Controls.Templates.FuncDataTemplate<object?>((_, _) => { built++; return new TextBlock { Text = "x" }; }) };
        var first = default(bool?);
        popup.BeforeOpen += (_, e) => first = e.IsFirstOpen;
        var w = TestHost.Show(new Canvas { Children = { popup } });
        Assert.Equal(0, built);
        popup.IsOpen = true;
        TestHost.Settle(w);
        Assert.Equal(1, built);
        Assert.True(first);
        popup.IsOpen = false;
        popup.IsOpen = true;
        Assert.Equal(1, built);
        Assert.False(first);
        w.Close();
    }

    [AvaloniaFact]
    public void BeforeOpen_Can_Cancel()
    {
        var (popup, w, _) = Make();
        popup.BeforeOpen += (_, e) => e.Cancel = true;
        popup.OpenAtCenter();
        Assert.False(popup.IsOpen);
        Assert.Null(popup.Presenter);
        w.Close();
    }

    [AvaloniaFact]
    public void Removing_The_Placeholder_Closes_As_Hidden()
    {
        var (popup, w, _) = Make();
        PopupCloseReason? reason = null;
        popup.Closed += (_, e) => reason = e.Reason;
        popup.OpenAtCenter();
        ((Canvas)w.Content!).Children.Remove(popup);
        Assert.Equal(PopupCloseReason.Hidden, reason);
        w.Close();
    }
}

