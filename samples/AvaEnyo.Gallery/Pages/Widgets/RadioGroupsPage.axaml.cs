using Avalonia.Controls;

namespace AvaEnyo.Gallery.Pages.Widgets;

public partial class RadioGroupsPage : UserControl
{
    public RadioGroupsPage()
    {
        InitializeComponent();
        Show();
        Modial.ValueChanged += (_, _) => Show();
    }

    private void Show() => ModialValue.Text = "Value: " + Modial.Value;
}
