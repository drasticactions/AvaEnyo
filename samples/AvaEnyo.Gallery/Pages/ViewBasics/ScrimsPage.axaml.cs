using Avalonia.Controls;

namespace AvaEnyo.Gallery.Pages.ViewBasics;

public partial class ScrimsPage : UserControl
{
    private int _count;

    public ScrimsPage()
    {
        InitializeComponent();
        DimToggle.IsCheckedChanged += (_, _) =>
        {
            var on = DimToggle.IsChecked == true;
            RegionScrim.IsOpen = on;
            RegionSpinner.IsActive = on;
        };
        BlockToggle.IsCheckedChanged += (_, _) => BlockScrim.IsOpen = BlockToggle.IsChecked == true;
        CountButton.Click += (_, _) => CountButton.Content = $"Pressed {++_count} times";
    }
}
