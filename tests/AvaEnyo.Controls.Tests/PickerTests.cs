using System;
using System.Globalization;
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

public class PickerTests
{
    private static void Click(Window w, Control c)
    {
        var p = c.TranslatePoint(new Point(c.Bounds.Width / 2, c.Bounds.Height / 2), w)!.Value;
        w.MouseDown(p, MouseButton.Left);
        w.MouseUp(p, MouseButton.Left);
        TestHost.Settle(w);
    }

    [AvaloniaFact]
    public void Tap_Opens_The_List_And_Choosing_Closes_It()
    {
        var picker = new Picker { ItemsSource = new[] { "Google", "Yahoo", "Bing" }, SelectedIndex = 0 };
        var w = TestHost.Show(new StackPanel { Margin = new Thickness(50), Children = { picker } }, 800, 600);
        PickerValueChangedEventArgs? changed = null;
        picker.ValueChanged += (_, e) => changed = e;
        Click(w, picker.Pill!);
        Assert.True(picker.IsDropDownOpen);
        Assert.True(picker.Pill!.IsFocusOpen);
        Assert.True(OverlayHost.Existing(w)!.SharedScrim.IsOpen);
        var row = (PickerItem)picker.List.ContainerFromIndex(2)!;
        Assert.True(row.IsEffectivelyVisible);
        row.RaiseClick();
        TestHost.Settle(w);
        Assert.False(picker.IsDropDownOpen);
        Assert.False(picker.Pill.IsFocusOpen);
        Assert.Equal("Bing", picker.SelectedItem);
        Assert.Equal("Bing", picker.Value);
        Assert.Equal("Bing", changed!.NewValue);
        Assert.Equal("Google", changed.OldValue);
        w.Close();
    }

    [AvaloniaFact]
    public void Selected_Row_Is_Bold_On_White()
    {
        var picker = new Picker { ItemsSource = new[] { "a", "b" }, SelectedIndex = 1 };
        var w = TestHost.Show(new StackPanel { Children = { picker } });
        picker.OpenDropDown();
        TestHost.Settle(w);
        var row = (PickerItem)picker.List.ContainerFromIndex(1)!;
        Assert.Contains(":selected", row.Classes);
        Assert.Equal(Avalonia.Media.FontWeight.Bold, row.FontWeight);
        picker.CloseDropDown();
        w.Close();
    }

    [AvaloniaFact]
    public void Value_That_Is_Not_An_Item_Is_Ignored()
    {
        var picker = new Picker { ItemsSource = new[] { "a", "b" }, Value = "b" };
        var w = TestHost.Show(picker);
        Assert.Equal(1, picker.SelectedIndex);
        picker.Value = "zzz";
        Assert.Equal("b", picker.Value);
        w.Close();
    }

    [AvaloniaFact]
    public void IntegerPicker_Lists_Its_Range()
    {
        var picker = new IntegerPicker { Minimum = 3, Maximum = 7 };
        var w = TestHost.Show(picker);
        Assert.Equal(5, picker.ItemCount);
        Assert.Equal(3, picker.Value);
        picker.Value = 6;
        Assert.Equal(3, picker.SelectedIndex);
        picker.Maximum = 4;
        Assert.Equal(4, picker.Value);
        w.Close();
    }

    [AvaloniaTheory]
    [InlineData("en-US", "Mdy")]
    [InlineData("de-DE", "dMy")]
    [InlineData("ja-JP", "yMd")]
    public void Date_Field_Order_Follows_The_Culture(string culture, string expected)
    {
        var old = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo(culture);
        try
        {
            var picker = new EnyoDatePicker { Value = new DateTime(2011, 8, 31) };
            var w = TestHost.Show(picker);
            var order = string.Concat(picker.Pickers.Select(p => p.Name switch { "PART_Month" => "M", "PART_Day" => "d", _ => "y" }));
            Assert.Equal(expected, order);
            w.Close();
        }
        finally
        {
            CultureInfo.CurrentCulture = old;
        }
    }

    [AvaloniaFact]
    public void Date_Value_Round_Trips_And_Clamps_The_Day()
    {
        var old = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        try
        {
            var picker = new EnyoDatePicker { Value = new DateTime(2011, 1, 31), HideYear = false };
            var w = TestHost.Show(picker);
            var month = picker.Pickers.First(p => p.Name == "PART_Month");
            var day = (IntegerPicker)picker.Pickers.First(p => p.Name == "PART_Day");
            Assert.Equal("Jan", month.SelectedItem);
            Assert.Equal(31, day.Value);
            DateTime? reported = null;
            picker.ValueChanged += (_, e) => reported = (DateTime)e.NewValue!;
            month.SelectedIndex = 1;
            Assert.Equal(new DateTime(2011, 2, 28), picker.Value);
            Assert.Equal(new DateTime(2011, 2, 28), reported);
            Assert.Equal(28, day.Maximum);
            picker.HideYear = true;
            TestHost.Settle(w);
            Assert.Equal(2, picker.Pickers.Count(p => p.IsVisible && p.GetVisualParent() is not null));
            w.Close();
        }
        finally
        {
            CultureInfo.CurrentCulture = old;
        }
    }

    [AvaloniaFact]
    public void Time_Picker_12_And_24_Hour()
    {
        var picker = new EnyoTimePicker { Is24Hour = false, MinuteInterval = 15, Value = new TimeSpan(14, 30, 0) };
        var w = TestHost.Show(picker);
        Assert.Equal(12, picker.HourPicker.ItemCount);
        Assert.Equal(4, picker.MinutePicker.ItemCount);
        Assert.Equal("2", picker.HourPicker.SelectedItem);
        Assert.Equal("30", picker.MinutePicker.SelectedItem);
        Assert.Equal(1, picker.PeriodPicker.SelectedIndex);
        picker.PeriodPicker.SelectedIndex = 0;
        Assert.Equal(new TimeSpan(2, 30, 0), picker.Value);
        picker.HourPicker.SelectedIndex = 11;
        Assert.Equal(new TimeSpan(0, 30, 0), picker.Value);
        picker.Is24Hour = true;
        TestHost.Settle(w);
        Assert.Equal(24, picker.HourPicker.ItemCount);
        Assert.Equal("00", picker.HourPicker.SelectedItem);
        Assert.DoesNotContain(picker.PeriodPicker, picker.Pickers.Where(p => p.GetVisualParent() is not null));
        w.Close();
    }

    [AvaloniaFact]
    public void Group_Keeps_All_Pills_Focused_And_Switches_To_A_Sibling()
    {
        var first = new Picker { ItemsSource = new[] { "a", "b" }, SelectedIndex = 0 };
        var second = new Picker { ItemsSource = new[] { "x", "y" }, SelectedIndex = 0 };
        var group = new PickerGroup { Label = "Prefs", Items = { first, second } };
        var w = TestHost.Show(new StackPanel { Margin = new Thickness(40), Children = { group } }, 800, 600);
        first.OpenDropDown();
        TestHost.Settle(w);
        Assert.True(first.Pill!.IsFocusOpen);
        Assert.True(second.Pill!.IsFocusOpen);
        var host = OverlayHost.Existing(w)!;
        Assert.True(host.SharedScrim.IsOpen);
        Assert.False(first.ListPopup!.WantsScrim);
        var sp = second.Pill.TranslatePoint(new Point(second.Pill.Bounds.Width - 4, second.Pill.Bounds.Height / 2), w)!.Value;
        w.MouseDown(sp, MouseButton.Left);
        w.MouseUp(sp, MouseButton.Left);
        TestHost.Settle(w);
        Assert.False(first.IsDropDownOpen);
        Assert.True(second.IsDropDownOpen);
        Assert.True(first.Pill.IsFocusOpen);
        second.CloseDropDown();
        TestHost.Settle(w);
        Assert.False(first.Pill.IsFocusOpen);
        Assert.False(host.SharedScrim.IsOpen);
        w.Close();
    }

    [AvaloniaFact]
    public void Drawer_End_States_And_Header_Toggle()
    {
        var content = new Border { Height = 80 };
        var drawer = new Drawer { Header = "Header", Content = content };
        var openChanges = 0;
        var completed = 0;
        drawer.IsOpenChanged += (_, _) => openChanges++;
        drawer.OpenAnimationCompleted += (_, _) => completed++;
        var w = TestHost.Show(new StackPanel { Children = { drawer } });
        var host = drawer.GetVisualDescendants().OfType<Control>().First(c => c.Name == "PART_ContentHost");
        Assert.True(host.IsVisible);
        drawer.IsOpen = false;
        TestHost.Settle(w);
        Assert.False(host.IsVisible);
        Assert.True(double.IsNaN(host.Height));
        Assert.Equal(1, openChanges);
        Assert.Equal(1, completed);
        var header = drawer.GetVisualDescendants().OfType<Control>().First(c => c.Name == "PART_Header");
        header.RaiseEvent(new TappedEventArgs(InputElement.TappedEvent, new PointerPressedEventArgs(header, new Pointer(1, PointerType.Mouse, true), header, default, 0, new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed), KeyModifiers.None)));
        TestHost.Settle(w);
        Assert.True(drawer.IsOpen);
        Assert.True(host.IsVisible);
        Assert.True(double.IsNaN(host.Height));
        Assert.Equal(80, host.Bounds.Height, 0);
        drawer.CanChangeOpen = false;
        drawer.IsOpen = false;
        Assert.True(drawer.IsOpen);
        w.Close();
    }

    [AvaloniaFact]
    public void DividerDrawer_Arrow_Turns_With_The_State()
    {
        var drawer = new DividerDrawer { Caption = "Section", Content = new Border { Height = 30 } };
        var w = TestHost.Show(new StackPanel { Children = { drawer } });
        Assert.Contains(":open", drawer.Classes);
        drawer.IsOpen = false;
        TestHost.Settle(w);
        Assert.DoesNotContain(":open", drawer.Classes);
        w.Close();
    }

    [AvaloniaFact]
    public void ProgressSlider_Bars()
    {
        var slider = new ProgressSlider { Width = 300, Value = 20, BarValue = 50, AltBarValue = 75 };
        var w = TestHost.Show(new StackPanel { Children = { slider } });
        var bar = slider.GetVisualDescendants().OfType<ProgressFill>().First(f => f.Name == "PART_Bar");
        var alt = slider.GetVisualDescendants().OfType<ProgressFill>().First(f => f.Name == "PART_AltBar");
        Assert.Equal(0.5, slider.BarRatio, 3);
        Assert.Equal(0.75, slider.AltBarRatio, 3);
        Assert.True(alt.Bounds.Width > bar.Bounds.Width);
        Assert.Equal(bar.Bounds.Width * 1.5, alt.Bounds.Width, 0);
        slider.LockBar = true;
        Assert.Equal(0.2, slider.BarRatio, 3);
        slider.Value = 60;
        Assert.Equal(0.6, slider.BarRatio, 3);
        w.Close();
    }

    [AvaloniaFact]
    public void ProgressButton_Cancel_Event()
    {
        var button = new ProgressButton { Width = 300, Value = 25, Content = "Copying" };
        var cancels = 0;
        button.Cancel += (_, _) => cancels++;
        var w = TestHost.Show(new StackPanel { Children = { button } });
        var bar = button.GetVisualDescendants().OfType<ProgressFill>().First();
        Assert.Equal(0.25, button.Ratio, 3);
        Assert.InRange(bar.Bounds.Width, 70, 80);
        var cancel = button.GetVisualDescendants().OfType<Button>().First(b => b.Name == "PART_CancelButton");
        cancel.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, cancel));
        Assert.Equal(1, cancels);
        button.Cancelable = false;
        TestHost.Settle(w);
        Assert.False(cancel.IsVisible);
        w.Close();
    }

    [AvaloniaFact]
    public void The_Pill_Caption_Is_Centered_Vertically()
    {
        var picker = new IntegerPicker { Minimum = 0, Maximum = 9 };
        var w = TestHost.Show(new StackPanel { Children = { picker } }, 300, 200);
        var pill = picker.GetVisualDescendants().OfType<PickerButton>().Single();
        var frame = TestHost.Render(w);
        var origin = pill.TranslatePoint(default, w)!.Value;
        var inked = Enumerable.Range(0, (int)pill.Bounds.Height)
            .Where(y => Enumerable.Range(10, (int)pill.Bounds.Width - 20)
                .Any(x => TestHost.PixelAt(frame, origin + new Point(x + 0.5, y + 0.5)).R < 120))
            .ToList();
        Assert.NotEmpty(inked);
        var middle = (inked.First() + inked.Last() + 1) / 2.0;
        Assert.InRange(middle, (pill.Bounds.Height / 2) - 2, (pill.Bounds.Height / 2) + 2);
        w.Close();
    }
}
