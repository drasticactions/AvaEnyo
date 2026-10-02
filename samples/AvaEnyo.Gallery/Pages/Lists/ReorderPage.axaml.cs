using System.Collections.ObjectModel;
using Avalonia.Controls;
using AvaEnyo.Samples;

namespace AvaEnyo.Gallery.Pages.Lists;

public partial class ReorderPage : UserControl
{
    public ReorderPage()
    {
        InitializeComponent();
        List.ItemsSource = new ObservableCollection<string>(SampleData.Fruits);
        List.Reordering += (_, e) => Log.Text = $"Moved row {e.From + 1} to {e.To + 1}";
    }
}
