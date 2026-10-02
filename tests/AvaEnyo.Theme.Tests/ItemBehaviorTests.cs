using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.VisualTree;
using Xunit;

namespace AvaEnyo.Theme.Tests;

public class ItemBehaviorTests
{
    private static T Part<T>(Control owner, string name) where T : Control =>
        owner.GetVisualDescendants().OfType<T>().First(c => c.Name == name);

    [AvaloniaFact]
    public void Expander_Ends_Collapsed_Then_At_Auto_Height()
    {
        var expander = new Expander { Header = "Drawer", Content = new Border { Height = 80 } };
        var window = ThemeTestHelpers.Host(new StackPanel { Children = { expander } }, "Light", EnyoDensity.Default);
        var content = Part<Border>(expander, "ExpanderContent");
        Assert.False(content.IsVisible);

        expander.IsExpanded = true;
        window.UpdateLayout();
        Assert.True(content.IsVisible);
        Assert.True(double.IsNaN(content.Height));
        Assert.Equal(80, content.Bounds.Height);

        expander.IsExpanded = false;
        window.UpdateLayout();
        Assert.False(content.IsVisible);
        Assert.True(double.IsNaN(content.Height));
        window.Close();
    }

    [AvaloniaFact]
    public void Expander_Header_Toggles()
    {
        var expander = new Expander { Header = "Drawer", Content = "c" };
        var window = ThemeTestHelpers.Host(new StackPanel { Children = { expander } }, "Light", EnyoDensity.Default);
        var header = Part<Avalonia.Controls.Primitives.ToggleButton>(expander, "ExpanderHeader");
        header.IsChecked = true;
        Assert.True(expander.IsExpanded);
        window.Close();
    }

    [AvaloniaFact]
    public void Pressed_Item_Shows_The_Held_Gradient_At_Once()
    {
        var item = new ListBoxItem { Content = "Row" };
        var list = new ListBox { Items = { item }, Width = 300, Height = 200 };
        var window = ThemeTestHelpers.Host(list, "Light", EnyoDensity.Default);
        var held = Part<Border>(item, "PART_HeldBorder");
        Assert.False(held.IsVisible);

        var center = item.TranslatePoint(new Point(20, item.Bounds.Height / 2), window)!.Value;
        window.MouseDown(center, MouseButton.Left);
        window.UpdateLayout();
        Assert.True(held.IsVisible);
        window.MouseUp(center, MouseButton.Left);
        window.UpdateLayout();

        // The release selected the row: the selected gradient stays.
        Assert.True(item.IsSelected);
        Assert.True(held.IsVisible);
        window.Close();
    }

    [AvaloniaFact]
    public void ComboBox_Opens_And_Closes()
    {
        var combo = new ComboBox { ItemsSource = new[] { "Small", "Medium", "Large" }, SelectedIndex = 1 };
        var window = ThemeTestHelpers.Host(new StackPanel { Children = { combo } }, "Light", EnyoDensity.Default);
        combo.IsDropDownOpen = true;
        window.UpdateLayout();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        var popup = Part<Avalonia.Controls.Primitives.Popup>(combo, "PART_Popup");
        Assert.True(popup.IsOpen);
        Assert.Equal(Avalonia.Controls.PlacementMode.Center, popup.Placement);
        combo.IsDropDownOpen = false;
        window.UpdateLayout();
        Assert.False(popup.IsOpen);
        Assert.Equal(0, popup.VerticalOffset);
        window.Close();
    }

    [AvaloniaFact]
    public void ListSelector_Label_Shows_Uppercase()
    {
        var combo = new ComboBox { ItemsSource = new[] { "a" }, SelectedIndex = 0 };
        ListSelectorAssist.SetLabel(combo, "Size");
        var window = ThemeTestHelpers.Host(new StackPanel { Children = { combo } }, "Light", EnyoDensity.Default);
        var label = Part<TextBlock>(combo, "PART_Label");
        Assert.True(label.IsVisible);
        Assert.Equal("SIZE", label.Text);
        window.Close();
    }

    [AvaloniaFact]
    public void Group_Label_Band_Hides_Without_A_Header()
    {
        var group = new GroupBox { Content = "c" };
        var window = ThemeTestHelpers.Host(group, "Light", EnyoDensity.Default);
        Assert.False(Part<Border>(group, "Header").IsVisible);
        group.Header = "Account";
        window.UpdateLayout();
        Assert.True(Part<Border>(group, "Header").IsVisible);
        window.Close();
    }
}
