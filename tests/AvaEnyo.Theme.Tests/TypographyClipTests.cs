using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Xunit;

namespace AvaEnyo.Theme.Tests;

public class TypographyClipTests
{
    /// <summary>
    /// A TextBlock does not clip to its bounds, so the descenders of Source Sans 3 draw below a tight line box, as in a
    /// browser.
    /// </summary>
    [AvaloniaFact]
    public void Descender_Ink_Is_Not_Clipped()
    {
        var text = new TextBlock
        {
            Text = "gjpqy",
            FontSize = 48,
            LineHeight = 30,
            Foreground = Brushes.Black,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
        };
        var panel = new Border { Background = Brushes.White, Child = text, Padding = new Thickness(0, 0, 0, 60) };
        var window = ThemeTestHelpers.Host(panel, "Light", EnyoDensity.Default, 300, 200);
        Assert.False(text.ClipToBounds);
        var bounds = text.Bounds;
        var frame = window.CaptureRenderedFrame()!;
        var dark = 0;
        using (var fb = frame.Lock())
        {
            var row = new byte[fb.RowBytes];
            for (var y = (int)System.Math.Ceiling(bounds.Bottom); y < (int)bounds.Bottom + 24; y++)
            {
                System.Runtime.InteropServices.Marshal.Copy(fb.Address + (y * fb.RowBytes), row, 0, fb.RowBytes);
                for (var x = 0; x < 250; x++)
                {
                    if (row[x * 4] < 200)
                    {
                        dark++;
                    }
                }
            }
        }

        Assert.True(dark > 0, $"bounds {bounds}: no ink below the line box");
        window.Close();
    }
}
