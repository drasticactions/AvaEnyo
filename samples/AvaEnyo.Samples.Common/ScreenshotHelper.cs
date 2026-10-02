using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;

namespace AvaEnyo.Samples;

/// <summary>Renders a window's view to a PNG for the <c>--screenshot</c> switch.</summary>
public static class ScreenshotHelper
{
    /// <summary>
    /// Lays the window's content out at exactly the <c>--size</c> of <see cref="LaunchOptions"/>, renders it and closes
    /// the window. The content is moved into a fixed-size frame on a canvas first, so the shot has the requested size
    /// even when a tiling compositor gives the window a different one.
    /// </summary>
    public static async Task CaptureAndCloseAsync(Window window, string path, int delayMs)
    {
        var target = Frame(window) ?? (Control)window;
        await Task.Delay(delayMs);
        var scaling = window.RenderScaling;
        var size = new PixelSize((int)Math.Ceiling(target.Bounds.Width * scaling), (int)Math.Ceiling(target.Bounds.Height * scaling));
        using var bitmap = new RenderTargetBitmap(size, new Vector(96 * scaling, 96 * scaling));
        bitmap.Render(target);
        bitmap.Save(path, new PngBitmapEncoderOptions());
        window.Close();
    }

    /// <summary>
    /// In screenshot mode, moves the window's content into a frame of exactly the requested size and returns the frame;
    /// returns the existing frame when called again, and null when no screenshot was asked for. Call it right after
    /// setting the content, before opening popups, because moving the content closes them.
    /// </summary>
    public static Control? Frame(Window window)
    {
        var o = LaunchOptions.Current;
        if (o.Screenshot is null)
        {
            return null;
        }

        if (window.Content is Canvas { Tag: "screenshot-frame", Children: [Control existing] })
        {
            return existing;
        }

        if (window.Content is Control view)
        {
            window.Content = null;
            // The frame has its own layer manager, so popups open in an overlay layer of the frame's size and are
            // in the shot.
            var frame = new Border
            {
                Width = o.Width,
                Height = o.Height,
                Background = window.Background,
                ClipToBounds = true,
                Child = new Avalonia.Controls.Primitives.VisualLayerManager { EnableOverlayLayer = true, Child = view },
            };
            window.Content = new Canvas { Tag = "screenshot-frame", Children = { frame } };
            return frame;
        }

        return null;
    }
}
