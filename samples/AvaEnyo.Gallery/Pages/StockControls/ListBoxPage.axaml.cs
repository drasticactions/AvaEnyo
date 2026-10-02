using Avalonia.Controls;

namespace AvaEnyo.Gallery.Pages.StockControls;

public partial class ListBoxPage : UserControl
{
    public ListBoxPage()
    {
        InitializeComponent();
        Fruits.ItemsSource = AvaEnyo.Samples.SampleData.Fruits;
        Multi.ItemsSource = AvaEnyo.Samples.SampleData.Fruits;
    }
}
