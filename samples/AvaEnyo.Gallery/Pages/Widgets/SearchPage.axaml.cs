using System;
using System.Linq;
using Avalonia.Controls;
using AvaEnyo.Controls;
using AvaEnyo.Samples;

namespace AvaEnyo.Gallery.Pages.Widgets;

public partial class SearchPage : UserControl
{
    public SearchPage()
    {
        InitializeComponent();
        SearchResult.Text = "Type to search " + SampleData.Fruits.Count + " fruits.";
        Search.AddCommittedHandler(SearchBox, (_, e) =>
        {
            var hits = SampleData.Fruits.Where(f => f.Contains(e.Text, StringComparison.CurrentCultureIgnoreCase)).ToList();
            SearchResult.Text = hits.Count == 0 ? "No fruit matches." : string.Join(", ", hits);
            Log("Committed \"" + e.Text + "\"");
        });
        Search.AddCancelledHandler(SearchBox, (_, _) =>
        {
            SearchResult.Text = "Search cleared.";
            Log("Cancelled");
        });
    }

    private void Log(string line) => SearchLog.Text = line + Environment.NewLine + SearchLog.Text;
}
