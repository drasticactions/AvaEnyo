using Avalonia.Controls;

namespace AvaEnyo.Gallery.Pages.StockControls;

public partial class RefreshPage : UserControl
{
    public RefreshPage()
    {
        InitializeComponent();
        var rows = new System.Collections.ObjectModel.ObservableCollection<string>(AvaEnyo.Samples.SampleData.Fruits);
        Rows.ItemsSource = rows;
        Refresh.RefreshRequested += async (_, e) =>
        {
            var deferral = e.GetDeferral();
            await System.Threading.Tasks.Task.Delay(800);
            rows.Insert(0, "Fresh fruit " + rows.Count);
            deferral.Complete();
        };
    }
}
