using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Xunit;

namespace AvaEnyo.Controls.Tests;

public class BackChainTests
{
    [AvaloniaFact]
    public void Popup_Closes_Before_The_Pane_Goes_Back()
    {
        var popup = new Popup { Content = new TextBlock { Text = "x" } };
        var pane = new Pane { Views = { new Border { Name = "a" }, new Panel { Name = "b", Children = { popup } } } };
        var w = TestHost.Show(pane);
        pane.SelectViewByName("b");
        popup.OpenAtCenter();
        Assert.True(EnyoBack.RequestBack(w, BackSource.Escape));
        Assert.False(popup.IsOpen);
        Assert.Equal("b", pane.SelectedView!.Name);
        Assert.True(EnyoBack.RequestBack(w, BackSource.Escape));
        Assert.Equal("a", pane.SelectedView!.Name);
        Assert.False(EnyoBack.RequestBack(w, BackSource.Escape));
        w.Close();
    }

    [AvaloniaFact]
    public void Inner_Pane_Goes_Back_Before_The_Outer_SlidingPane()
    {
        var inner = new Pane { Views = { new Border { Name = "x" }, new Border { Name = "y" } } };
        var c = new SlidingView { Content = inner };
        AvaEnyo.Controls.Primitives.Flex.SetValue(c, 1);
        var sliding = new SlidingPane { Views = { new SlidingView { Width = 320 }, c } };
        var w = TestHost.Show(sliding, 1024, 600);
        sliding.SelectViewByIndex(1);
        inner.SelectViewByName("y");
        Assert.True(EnyoBack.RequestBack(w));
        Assert.Equal("x", inner.SelectedView!.Name);
        Assert.Equal(1, sliding.SelectedIndex);
        Assert.True(EnyoBack.RequestBack(w));
        Assert.Equal(0, sliding.SelectedIndex);
        w.Close();
    }
}
