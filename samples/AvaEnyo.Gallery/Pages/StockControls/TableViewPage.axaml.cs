using Avalonia.Controls;

namespace AvaEnyo.Gallery.Pages.StockControls;

public partial class TableViewPage : UserControl
{
    public TableViewPage()
    {
        InitializeComponent();
        Table.Columns.Add(Column("Name", c => c.Name));
        Table.Columns.Add(Column("Email", c => c.Email));
        Table.ItemsSource = AvaEnyo.Samples.SampleData.Contacts;
    }

    private static TableViewColumn Column(string header, System.Func<AvaEnyo.Samples.Contact, string> text) => new()
    {
        Header = header,
        CellTemplate = new Avalonia.Controls.Templates.FuncDataTemplate<AvaEnyo.Samples.Contact>((c, _) => new TextBlock { Text = c is null ? string.Empty : text(c) }),
    };
}
