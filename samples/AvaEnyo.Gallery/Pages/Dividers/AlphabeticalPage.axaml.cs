using System.Linq;
using Avalonia.Controls;
using AvaEnyo.Controls;
using AvaEnyo.Samples;

namespace AvaEnyo.Gallery.Pages.Dividers;

public partial class AlphabeticalPage : UserControl
{
    public AlphabeticalPage()
    {
        InitializeComponent();
        foreach (var group in SampleData.Contacts.OrderBy(c => c.Name).GroupBy(c => c.Initial))
        {
            List.Children.Add(new AlphaDivider { Caption = group.Key });
            foreach (var contact in group)
            {
                List.Children.Add(new Item { Content = contact.Name });
            }
        }
    }
}
