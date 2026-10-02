using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Xunit;

namespace AvaEnyo.Theme.Tests;

public class MenuScrollViewerTests
{
    [AvaloniaFact]
    public void An_Overflowing_Menu_Scroller_Draws_No_Scroll_Bar()
    {
        var scroller = new ScrollViewer
        {
            Height = 200,
            Content = new Border { Height = 2000, Background = Brushes.White },
        };
        scroller.Theme = (Avalonia.Styling.ControlTheme)TestApplication.Current!.FindResource("EnyoMenuScrollViewer")!;
        var w = ThemeTestHelpers.Host(new StackPanel { Width = 200, Children = { scroller } }, "Light", EnyoDensity.Default, 300, 300);
        var frame = w.CaptureRenderedFrame()!;
        var origin = scroller.TranslatePoint(default, w)!.Value;
        var marked = Enumerable.Range(0, 60)
            .SelectMany(y => Enumerable.Range(0, 200).Select(x => new Point(x + 0.5, y + 0.5)))
            .Where(p => ThemeTestHelpers.PixelAt(frame, origin + p) != Colors.White)
            .ToList();
        Assert.True(marked.Count == 0, $"{marked.Count} pixels drawn over the content, first at {marked.FirstOrDefault()}");
        w.Close();
    }
}
