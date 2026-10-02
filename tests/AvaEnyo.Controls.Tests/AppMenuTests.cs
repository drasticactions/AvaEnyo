using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.VisualTree;
using Xunit;

namespace AvaEnyo.Controls.Tests;

public class AppMenuTests
{
    [AvaloniaFact]
    public void Opens_At_The_Top_Left()
    {
        var menu = new AppMenu { Items = { new AppMenuItem { Header = "Preferences" } } };
        var w = TestHost.Show(new Canvas { Children = { menu } }, 800, 600);
        menu.Open();
        TestHost.Settle(w);
        Assert.True(menu.IsOpen);
        var pos = menu.Presenter!.TranslatePoint(default, w)!.Value;
        Assert.Equal(0, pos.X);
        Assert.Equal(0, pos.Y);
        Assert.IsType<AppMenuPresenter>(menu.Presenter);
        w.Close();
    }

    [AvaloniaFact]
    public void Item_Click_Closes_With_Handle_And_Value()
    {
        var item = new AppMenuItem { Header = "Preferences", Value = "prefs" };
        var clicks = 0;
        item.Click += (_, _) => clicks++;
        var menu = new AppMenu { Items = { item, new AppMenuItem { Header = "About" } } };
        PopupClosedEventArgs? closed = null;
        menu.Closed += (_, e) => closed = e;
        var w = TestHost.Show(new Canvas { Children = { menu } }, 800, 600);
        menu.Open();
        TestHost.Settle(w);
        var center = item.TranslatePoint(new Point(20, item.Bounds.Height / 2), w)!.Value;
        w.MouseDown(center, MouseButton.Left);
        Assert.Contains(":held", item.Classes);
        w.MouseUp(center, MouseButton.Left);
        TestHost.Settle(w);
        Assert.Equal(1, clicks);
        Assert.False(menu.IsOpen);
        Assert.Equal(PopupCloseReason.Handle, closed!.Reason);
        Assert.Equal("prefs", closed.Value);
        w.Close();
    }

    [AvaloniaFact]
    public void Item_With_Children_Expands_Instead_Of_Closing()
    {
        var edit = new EditMenu();
        var menu = new AppMenu { Items = { edit } };
        var w = TestHost.Show(new Canvas { Children = { menu } }, 800, 600);
        menu.Open();
        TestHost.Settle(w);
        edit.RaiseClick();
        Assert.True(edit.IsExpanded);
        Assert.True(menu.IsOpen);
        w.Close();
    }

    [AvaloniaFact]
    public void Last_Item_Has_No_Divider_Order()
    {
        var a = new AppMenuItem { Header = "A" };
        var b = new AppMenuItem { Header = "B" };
        var menu = new AppMenu { Items = { a, b } };
        var w = TestHost.Show(new Canvas { Children = { menu } }, 800, 600);
        menu.Open();
        TestHost.Settle(w);
        Assert.Contains(":first", a.Classes);
        Assert.Contains(":last", b.Classes);
        w.Close();
    }

    [AvaloniaFact]
    public void EditMenu_Enables_By_TextBox_State()
    {
        var box = new TextBox { Text = "hello world", Width = 200 };
        var edit = new EditMenu();
        var menu = new AppMenu { Items = { edit } };
        var other = new Button { Content = "Other" };
        var w = TestHost.Show(new StackPanel { Children = { other, box, menu } }, 800, 600);

        other.Focus();
        menu.Open();
        TestHost.Settle(w);
        Assert.False(edit.SelectAllItem.IsEnabled);
        Assert.False(edit.PasteItem.IsEnabled);
        menu.Close();
        TestHost.Settle(w);

        box.Focus();
        box.SelectionStart = 0;
        box.SelectionEnd = 0;
        menu.Open();
        TestHost.Settle(w);
        Assert.Same(box, edit.Target);
        Assert.True(edit.SelectAllItem.IsEnabled);
        Assert.True(edit.PasteItem.IsEnabled);
        Assert.False(edit.CutItem.IsEnabled);
        Assert.False(edit.CopyItem.IsEnabled);

        edit.SelectAllItem.RaiseClick();
        Assert.Equal(0, box.SelectionStart);
        Assert.Equal(box.Text!.Length, box.SelectionEnd);
        Assert.False(menu.IsOpen);

        menu.Open();
        TestHost.Settle(w);
        Assert.True(edit.CutItem.IsEnabled);
        Assert.True(edit.CopyItem.IsEnabled);
        w.Close();
    }

    [AvaloniaFact]
    public void Modal_Popup_Blocks_The_Menu()
    {
        var modal = new Popup { IsModal = true, Content = new TextBlock { Text = "Modal" } };
        var menu = new AppMenu { Items = { new AppMenuItem { Header = "A" } } };
        var w = TestHost.Show(new Canvas { Children = { modal, menu } }, 800, 600);
        modal.OpenAtCenter();
        TestHost.Settle(w);
        Assert.True(menu.IsBlockedByModal);
        menu.Open();
        Assert.False(menu.IsOpen);
        w.Close();
    }

    [AvaloniaFact]
    public void Shortcuts_Show_On_Request()
    {
        var edit = new EditMenu { ShowShortcuts = true, IsExpanded = true };
        var w = TestHost.Show(edit, 400, 400);
        Assert.True(edit.CutItem.ShowShortcut);
        var shortcut = edit.CutItem.GetVisualDescendants().OfType<TextBlock>().First(t => t.Name == "PART_Shortcut");
        Assert.Equal("Ctrl+X", shortcut.Text);
        Assert.True(shortcut.IsVisible);
        w.Close();
    }
}
