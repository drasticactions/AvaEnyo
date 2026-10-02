using Avalonia.Controls;

namespace AvaEnyo.Gallery.Pages.StockControls;

public partial class AutoCompletePage : UserControl
{
    public AutoCompletePage()
    {
        InitializeComponent();
        Fruits.ItemsSource = AvaEnyo.Samples.SampleData.Fruits;
    }
}
