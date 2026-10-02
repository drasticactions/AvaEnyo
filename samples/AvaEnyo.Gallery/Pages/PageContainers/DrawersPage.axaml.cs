using Avalonia.Controls;

namespace AvaEnyo.Gallery.Pages.PageContainers;

public partial class DrawersPage : UserControl
{
    public DrawersPage()
    {
        InitializeComponent();
        ToggleButton.Click += (_, _) => PlainDrawer.ToggleOpen();
        AnimateSwitch.IsCheckedChanged += (_, _) => PlainDrawer.Animate = AnimateSwitch.IsChecked == true;
        PlainDrawer.IsOpenChanged += (_, _) => EventText.Text = $"IsOpenChanged: {PlainDrawer.IsOpen}";
        PlainDrawer.OpenAnimationCompleted += (_, _) => EventText.Text += " · animation completed";
    }
}
