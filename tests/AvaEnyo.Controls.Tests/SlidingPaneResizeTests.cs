using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using AvaEnyo.Controls.Primitives;
using Xunit;

namespace AvaEnyo.Controls.Tests;

public class SlidingPaneResizeTests
{
    [AvaloniaFact]
    public void Going_Narrow_Revalidates_Positions()
    {
        var index = new SlidingView { Width = 320 };
        var sample = new SlidingView { PeekWidth = 320 };
        Flex.SetValue(sample, 1);
        var source = new SlidingView { Width = 400, Dismissible = true, IsShowing = false };
        var pane = new SlidingPane { Views = { index, sample, source } };
        pane.SelectView(sample, immediate: true);
        var w = TestHost.Show(pane, 1280, 700);
        Assert.Equal(0, sample.SlidePosition);
        w.Width = 480;
        TestHost.Settle(w);
        Assert.True(pane.IsNarrow);
        Assert.Equal(480, sample.NaturalLeft);
        Assert.Equal(-480, sample.SlidePosition);
        w.Width = 1280;
        TestHost.Settle(w);
        Assert.Equal(0, sample.SlidePosition);
        w.Close();
    }
}
