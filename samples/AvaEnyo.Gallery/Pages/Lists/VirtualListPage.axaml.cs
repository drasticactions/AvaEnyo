using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using AvaEnyo.Controls;
using AvaEnyo.Samples;

namespace AvaEnyo.Gallery.Pages.Lists;

public partial class VirtualListPage : UserControl
{
    public VirtualListPage()
    {
        InitializeComponent();
        var rows = MakeRows(500);
        List.ItemsSource = rows;
        List.DividerKeySelector = o => o is string s && s.Length > 0 ? s[..1] : null;
        Count.Text = $"{rows.Count} rows, realized on demand";
        Kind.ValueChanged += (_, _) => List.DividerKind = Kind.SelectedIndex == 1 ? DividerKind.Alpha : DividerKind.Labeled;
    }

    /// <summary>Names built from the sample contacts, sorted, so the first letter changes every few rows.</summary>
    internal static List<string> MakeRows(int count)
    {
        var first = SampleData.Contacts.Select(c => c.Name.Split(' ')[0]).ToArray();
        var last = SampleData.Contacts.Select(c => c.Name.Split(' ')[^1]).ToArray();
        var list = new List<string>(count);
        for (var i = 0; i < count; i++)
        {
            list.Add(first[(i * 7) % first.Length] + " " + last[((i * 11) + (i / first.Length)) % last.Length] + " " + (i + 1));
        }

        list.Sort(System.StringComparer.CurrentCulture);
        return list;
    }
}
