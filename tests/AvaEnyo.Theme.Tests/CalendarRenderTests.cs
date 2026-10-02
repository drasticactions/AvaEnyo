using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Xunit;

namespace AvaEnyo.Theme.Tests;

public class CalendarRenderTests
{
    [AvaloniaFact]
    public void The_Outline_Draws_Over_The_Header_At_The_Top_Corners()
    {
        var calendar = new Calendar { DisplayDate = new DateTime(2026, 10, 1) };
        var w = ThemeTestHelpers.Host(new StackPanel { Children = { calendar } }, "Light", EnyoDensity.Default, 400, 400);
        var item = calendar.GetVisualDescendants().OfType<CalendarItem>().Single();
        var frame = w.CaptureRenderedFrame()!;
        var origin = item.TranslatePoint(default, w)!.Value;
        var r = item.CornerRadius.TopLeft;
        foreach (var right in new[] { false, true })
        {
            // Points on the stroke and just inside it, along the 45° line through the corner arc.
            var d = r - ((r - 0.5) / Math.Sqrt(2));
            var inset = d + 3;
            Point At(double o) => origin + new Point(right ? item.Bounds.Width - o : o, o);
            var stroke = ThemeTestHelpers.PixelAt(frame, At(d));
            var inside = ThemeTestHelpers.PixelAt(frame, At(inset));
            Assert.True(stroke.R + 30 < inside.R, $"{(right ? "right" : "left")} corner: stroke {stroke}, header {inside}");
        }

        w.Close();
    }
}
