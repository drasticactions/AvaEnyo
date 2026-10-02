using System.Linq;
using Avalonia.Controls;
using AvaEnyo.Controls;
using AvaEnyo.Samples;

namespace AvaEnyo.Gallery.Pages.Popups;

public partial class MenusPage : UserControl
{
    public MenusPage()
    {
        InitializeComponent();
        MenuButton.Click += (_, _) => SortMenu.OpenAroundControl(MenuButton);
        SortMenu.Closed += (_, e) =>
        {
            if (e.Reason == PopupCloseReason.Handle)
            {
                MenuResult.Text = $"Chosen: {e.Value} · flagged only: {FlaggedItem.IsChecked} · unread only: {UnreadItem.IsChecked}";
            }
        };

        FruitSelect.ItemsSource = SampleData.Fruits.Take(8).ToList();
        SelectButton.Click += (_, _) => FruitSelect.OpenAroundControl(SelectButton);
        FruitSelect.Selected += (_, e) => SelectResult.Text = $"Selected: {e.Item}";

        RowList.ItemsSource = Enumerable.Range(1, 500).Select(i => $"Row {i}").ToList();
        ListButton.Click += (_, _) => RowList.OpenAroundControl(ListButton);
        RowList.Selected += (_, e) => ListResult.Text = $"Selected: {e.Item} (index {e.Index})";
    }
}
