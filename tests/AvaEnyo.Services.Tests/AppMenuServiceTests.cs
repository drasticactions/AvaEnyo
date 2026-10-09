using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using AvaEnyo.Controls;
using Xunit;

namespace AvaEnyo.Services.Tests;

public class AppMenuServiceTests
{
    private static (Window Window, IAppMenuHost Host, AppMenu Menu) Make()
    {
        var w = Host.Show(new Canvas());
        var menu = new AppMenu { Items = { new AppMenuItem { Header = "Preferences", Value = "prefs" } } };
        var host = AppMenuService.Attach(w, menu);
        Host.Settle(w);
        return (w, host, menu);
    }

    [AvaloniaFact]
    public void Ctrl_Backquote_Toggles_The_Menu()
    {
        var (w, host, _) = Make();
        w.KeyPressQwerty(PhysicalKey.Backquote, RawInputModifiers.Control);
        Assert.True(host.IsOpen);
        w.KeyPressQwerty(PhysicalKey.Backquote, RawInputModifiers.Control);
        Assert.False(host.IsOpen);
        w.Close();
    }

    [AvaloniaFact]
    public void F10_Opens_The_Menu()
    {
        var (w, host, _) = Make();
        w.KeyPressQwerty(PhysicalKey.F10, RawInputModifiers.None);
        Assert.True(host.IsOpen);
        w.Close();
    }

    [AvaloniaFact]
    public void Manual_Mode_Ignores_Keys()
    {
        var (w, host, _) = Make();
        AppMenuService.SetAutomatic(w, false);
        w.KeyPressQwerty(PhysicalKey.F10, RawInputModifiers.None);
        Assert.False(host.IsOpen);
        host.Open();
        Assert.True(host.IsOpen);
        w.Close();
    }

    [AvaloniaFact]
    public void Open_Region_Tap_Opens_The_Menu()
    {
        var title = new Border { Width = 200, Height = 50, Background = Avalonia.Media.Brushes.Gray };
        AppMenuService.SetIsOpenRegion(title, true);
        var w = Host.Show(new Canvas { Children = { title } });
        var host = AppMenuService.Attach(w, new AppMenu { Items = { new AppMenuItem { Header = "A" } } });
        w.MouseDown(new Avalonia.Point(20, 20), MouseButton.Left);
        w.MouseUp(new Avalonia.Point(20, 20), MouseButton.Left);
        Assert.True(host.IsOpen);
        w.Close();
    }

    [AvaloniaFact]
    public void Modal_Popup_Blocks_Opening()
    {
        var (w, host, _) = Make();
        var modal = new Popup { IsModal = true, Content = new TextBlock { Text = "Modal" } };
        ((Canvas)w.Content!).Children.Add(modal);
        modal.OpenAtCenter();
        Host.Settle(w);
        host.Open();
        Assert.False(host.IsOpen);
        w.Close();
    }

    [AvaloniaFact]
    public void Opening_Can_Add_Items_And_Closed_Reports_The_Value()
    {
        var (w, host, menu) = Make();
        host.Opening += (_, e) => e.Menu!.Items.Add(new AppMenuItem { Header = "Dynamic" });
        AppMenuClosedEventArgs? closed = null;
        host.Closed += (_, e) => closed = e;
        host.Open();
        Host.Settle(w);
        Assert.Equal(2, menu.Items.Count);
        ((AppMenuItem)menu.Items[0]).RaiseClick();
        Assert.Equal(PopupCloseReason.Handle, closed!.Reason);
        Assert.Equal("prefs", closed.Value);
        w.Close();
    }

    [AvaloniaFact]
    public void Attached_Property_Connects_The_Menu()
    {
        var w = Host.Show(new Canvas());
        var menu = new AppMenu { Items = { new AppMenuItem { Header = "A" } } };
        AppMenuService.SetMenu(w, menu);
        Assert.NotNull(AppMenuService.GetHost(w));
        w.KeyPressQwerty(PhysicalKey.F10, RawInputModifiers.None);
        Assert.True(menu.IsOpen);
        w.Close();
    }

    [AvaloniaFact]
    public void Attaching_Marks_The_Window_As_Having_An_App_Menu()
    {
        var (w, _, _) = Make();
        Assert.True(EnyoWindow.GetHasAppMenu(w));
        AppMenuService.Detach(w);
        Assert.False(EnyoWindow.GetHasAppMenu(w));
        w.Close();
    }

    [AvaloniaFact]
    public void App_Menu_Button_Toggles_The_Menu()
    {
        var button = new Button();
        EnyoWindow.SetOpensAppMenu(button, true);
        var w = Host.Show(new Canvas { Children = { button } });
        var host = AppMenuService.Attach(w, new AppMenu { Items = { new AppMenuItem { Header = "A" } } });
        Host.Settle(w);
        button.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
        Assert.True(host.IsOpen);
        button.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
        Assert.False(host.IsOpen);
        w.Close();
    }

    [Fact]
    public void Test_Double_Tracks_State()
    {
        var host = new TestAppMenuHost();
        host.Toggle();
        Assert.True(host.IsOpen);
        object? value = null;
        host.Closed += (_, e) => value = e.Value;
        host.CloseWith(PopupCloseReason.Handle, "x");
        Assert.Equal("x", value);
        Assert.Equal(1, host.OpenCount);
    }
}
