using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Xunit;

namespace AvaEnyo.Controls.Tests;

public class GroupLabelTests
{
    /// <summary>group-labeled.png carries the label in uppercase (Group.css text-transform).</summary>
    [AvaloniaFact]
    public void Group_Label_Is_Uppercase()
    {
        var group = new Group { Header = "Work", Items = { new Item { Content = "Inbox" } } };
        var w = TestHost.Show(group);
        var texts = group.GetVisualDescendants().OfType<TextBlock>().Select(t => t.Text).ToList();
        Assert.Contains("WORK", texts);
        Assert.Equal("Work", group.Header);
        w.Close();
    }

    [AvaloniaFact]
    public void Changed_Group_Label_Is_Uppercase()
    {
        var group = new Group { Header = "Work", Items = { new Item { Content = "Inbox" } } };
        var w = TestHost.Show(group);
        group.Header = "Personal";
        TestHost.Settle(w);
        var texts = group.GetVisualDescendants().OfType<TextBlock>().Select(t => t.Text).ToList();
        Assert.Contains("PERSONAL", texts);
        w.Close();
    }
}
