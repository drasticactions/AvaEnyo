using Avalonia.Controls;

namespace AvaEnyo.Gallery.Pages.PageContainers;

public partial class GroupsPage : UserControl
{
    public GroupsPage()
    {
        InitializeComponent();
        HideButton.Click += (_, _) => Rows.HideRow(1);
        ShowButton.Click += (_, _) => Rows.ShowRow(1);
    }
}
