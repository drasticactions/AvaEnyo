using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Transformation;
using Avalonia.VisualTree;
using AvaEnyo.Controls.Primitives;
using Xunit;

namespace AvaEnyo.Controls.Tests;

public class OverlayTests
{
    private static Window Host(params Control[] popups)
    {
        var canvas = new Canvas();
        foreach (var p in popups)
        {
            canvas.Children.Add(p);
        }

        return TestHost.Show(canvas, 800, 600);
    }

    private static void Tap(Window w, Control target)
    {
        var p = target.TranslatePoint(new Point(target.Bounds.Width / 2, target.Bounds.Height / 2), w)!.Value;
        new TouchInput(w).Tap(p);
    }

    [AvaloniaFact]
    public void Menu_Item_Click_Closes_With_Handle_And_Value()
    {
        var open = new MenuItem { Header = "Open", Value = "open" };
        var menu = new Menu { Items = { open, new MenuItem { Header = "Save" } } };
        PopupClosedEventArgs? closed = null;
        menu.Closed += (_, e) => closed = e;
        var w = Host(menu);
        menu.OpenAt(new Point(100, 100));
        TestHost.Settle(w);
        Assert.True(open.IsAttachedToVisualTree());
        Assert.Contains(":first", open.Classes);
        var row = open.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_Row");
        Tap(w, row);
        Assert.False(menu.IsOpen);
        Assert.Equal(PopupCloseReason.Handle, closed!.Reason);
        Assert.Equal("open", closed.Value);
        w.Close();
    }

    [AvaloniaFact]
    public void Nested_Item_Expands_Instead_Of_Closing()
    {
        var child = new MenuItem { Header = "Child" };
        var parent = new MenuItem { Header = "Parent", Items = { child } };
        var menu = new Menu { Items = { parent } };
        var w = Host(menu);
        menu.OpenAtCenter();
        TestHost.Settle(w);
        Assert.Contains(":has-items", parent.Classes);
        Assert.False(child.IsAttachedToVisualTree() && child.IsEffectivelyVisible);
        parent.Activate();
        TestHost.Settle(w);
        Assert.True(menu.IsOpen);
        Assert.True(parent.IsOpen);
        Assert.True(child.IsEffectivelyVisible);
        Assert.Equal(1, child.Depth);
        child.Activate();
        Assert.False(menu.IsOpen);
        w.Close();
    }

    [AvaloniaFact]
    public void Opening_Closes_Sub_Drawers()
    {
        var parent = new MenuItem { Header = "Parent", IsOpen = true, Items = { new MenuItem { Header = "c" } } };
        var menu = new Menu { Items = { parent } };
        var w = Host(menu);
        menu.OpenAtCenter();
        Assert.False(parent.IsOpen);
        w.Close();
    }

    [AvaloniaFact]
    public void Strings_Become_Menu_Items()
    {
        var menu = new Menu { ItemsSource = new[] { "One", "Two" } };
        object? value = null;
        menu.Closed += (_, e) => value = e.Value;
        var w = Host(menu);
        menu.OpenAtCenter();
        TestHost.Settle(w);
        var items = menu.Presenter!.GetVisualDescendants().OfType<MenuItem>().ToList();
        Assert.Equal(2, items.Count);
        items[1].Activate();
        Assert.Equal("Two", value);
        w.Close();
    }

    [AvaloniaFact]
    public void MenuCheckItem_Toggles()
    {
        var check = new MenuCheckItem { Header = "Wrap" };
        var menu = new Menu { Items = { check } };
        var w = Host(menu);
        menu.OpenAtCenter();
        TestHost.Settle(w);
        check.Activate();
        Assert.True(check.IsChecked);
        Assert.Contains(":checked", check.Classes);
        w.Close();
    }

    [AvaloniaFact]
    public void PopupList_Selection_Raises_Selected_And_Closes()
    {
        var list = new PopupList { ItemsSource = Enumerable.Range(0, 200).Select(i => "Row " + i).ToList(), SelectedIndex = 1 };
        PopupSelectedEventArgs? selected = null;
        list.Selected += (_, e) => selected = e;
        var w = Host(list);
        list.OpenAtCenter();
        TestHost.Settle(w);
        var rows = list.Presenter!.GetVisualDescendants().OfType<PopupListItem>().ToList();
        Assert.True(rows.Count < 200, $"{rows.Count} rows realized");
        Assert.True(rows[1].IsSelected);
        Assert.Equal(Avalonia.Media.FontWeight.Bold, rows[1].FontWeight);
        rows[2].RaiseClick();
        Assert.False(list.IsOpen);
        Assert.Equal(2, selected!.Index);
        Assert.Equal("Row 2", selected.Item);
        Assert.Equal("Row 2", list.SelectedItem);
        w.Close();
    }

    [AvaloniaFact]
    public void PopupSelect_Selects_By_Item()
    {
        var select = new PopupSelect { Items = { "a", "b", "c" } };
        var w = Host(select);
        select.SelectedItem = "c";
        Assert.Equal(2, select.SelectedIndex);
        select.OpenAtCenter();
        TestHost.Settle(w);
        var rows = select.Presenter!.GetVisualDescendants().OfType<PopupListItem>().ToList();
        Assert.Equal(3, rows.Count);
        Assert.True(rows[2].IsSelected);
        w.Close();
    }

    [AvaloniaFact]
    public async Task ModalDialog_ShowAsync_Returns_The_Close_Value()
    {
        var ok = new Button { Content = "OK" };
        Popup.SetCloseValue(ok, "ok");
        var dialog = new ModalDialog { Caption = "Sure?", Content = new StackPanel { Children = { ok } } };
        var w = Host(dialog);
        var task = dialog.ShowAsync();
        TestHost.Settle(w);
        Assert.True(OverlayHost.Existing(w)!.SharedScrim.IsOpen);
        w.MouseDown(new Point(5, 5), MouseButton.Left);
        w.MouseUp(new Point(5, 5), MouseButton.Left);
        Assert.True(dialog.IsOpen);
        Assert.False(task.IsCompleted);
        var caption = dialog.Presenter!.GetVisualDescendants().OfType<TextBlock>().First(t => t.Name == "PART_Caption");
        Assert.Equal("Sure?", caption.Text);
        ok.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, ok));
        Assert.Equal("ok", await task);
        w.Close();
    }

    [AvaloniaFact]
    public void Toaster_Opens_Against_The_Bottom_And_Closes()
    {
        var toaster = new Toaster { Content = new Border { Height = 80 } };
        var w = Host(toaster);
        toaster.Open();
        TestHost.Settle(w);
        var p = toaster.Presenter!;
        var pos = p.TranslatePoint(default, w)!.Value;
        Assert.Equal(800, p.Bounds.Width, 0);
        Assert.Equal(600 - p.Bounds.Height, pos.Y, 0);
        toaster.Close();
        Assert.False(toaster.IsOpen);
        Assert.Null(toaster.Presenter);
        w.Close();
    }

    [AvaloniaFact]
    public void Toaster_Handle_Drag_Down_Dismisses()
    {
        var handle = new Border { Height = 40, Background = Avalonia.Media.Brushes.Gray };
        Sliding.SetIsHandle(handle, true);
        var toaster = new Toaster { Content = new StackPanel { Children = { handle, new Border { Height = 60 } } } };
        var w = Host(toaster);
        toaster.Open();
        TestHost.Settle(w);
        var start = handle.TranslatePoint(new Point(100, 20), w)!.Value;
        var touch = new TouchInput(w);
        touch.Drag(start, new Vector(0, 40));
        var shift = ((TransformOperations)toaster.Presenter!.RenderTransform!).Value.M32;
        Assert.True(shift > 20, $"shift {shift}");
        touch.Up(start + new Vector(0, 40));
        TestHost.Settle(w);
        Assert.False(toaster.IsOpen);
        w.Close();
    }

    [AvaloniaFact]
    public void Toaster_Handle_Drag_Back_Up_Stays_Open()
    {
        var handle = new Border { Height = 40, Background = Avalonia.Media.Brushes.Gray };
        Sliding.SetIsHandle(handle, true);
        var toaster = new Toaster { Content = new StackPanel { Children = { handle, new Border { Height = 60 } } } };
        var w = Host(toaster);
        toaster.Open();
        TestHost.Settle(w);
        var start = handle.TranslatePoint(new Point(100, 20), w)!.Value;
        var touch = new TouchInput(w);
        touch.Drag(start, new Vector(0, 40));
        touch.MoveBy(start + new Vector(0, 40), new Vector(0, -20), 4);
        touch.Up(start + new Vector(0, 20));
        TestHost.Settle(w);
        Assert.True(toaster.IsOpen);
        Assert.Null(toaster.Presenter!.RenderTransform);
        w.Close();
    }

    [AvaloniaFact]
    public async Task DialogPrompt_Accept_And_Cancel()
    {
        var prompt = new DialogPrompt { Title = "Delete?", Message = "Really?" };
        var accepted = 0;
        var cancelled = 0;
        prompt.Accepted += (_, _) => accepted++;
        prompt.Cancelled += (_, _) => cancelled++;
        var w = Host(prompt);
        var first = prompt.ShowAsync();
        TestHost.Settle(w);
        var accept = prompt.Presenter!.GetVisualDescendants().OfType<Button>().First(b => b.Name == "PART_AcceptButton");
        Assert.Equal("OK", accept.Content);
        accept.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, accept));
        Assert.True(await first);
        var second = prompt.ShowAsync();
        TestHost.Settle(w);
        var cancel = prompt.Presenter!.GetVisualDescendants().OfType<Button>().First(b => b.Name == "PART_CancelButton");
        cancel.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, cancel));
        Assert.False(await second);
        Assert.Equal(1, accepted);
        Assert.Equal(1, cancelled);
        w.Close();
    }

    [AvaloniaFact]
    public void ScrollFades_Follow_The_Target()
    {
        var list = new StackPanel();
        for (var i = 0; i < 40; i++)
        {
            list.Children.Add(new Border { Height = 30 });
        }

        var viewer = new ScrollViewer { Content = list };
        var fades = new ScrollFades { Target = viewer };
        var w = TestHost.Show(new Panel { Children = { viewer, fades } }, 300, 300);
        Assert.Contains(":scroll-down", fades.Classes);
        Assert.DoesNotContain(":scroll-up", fades.Classes);
        var top = fades.GetVisualDescendants().OfType<Avalonia.Controls.Shapes.Path>().First(p => p.Name == "PART_FadeTop");
        Assert.False(top.IsVisible);
        viewer.Offset = new Vector(0, 100);
        TestHost.Settle(w);
        Assert.Contains(":scroll-up", fades.Classes);
        Assert.True(top.IsVisible);
        w.Close();
    }
}
