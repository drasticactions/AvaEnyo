using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Xunit;

namespace AvaEnyo.Controls.Tests;

public class MediaTests
{
    public static WriteableBitmap MakeBitmap(int width, int height)
    {
        var bitmap = new WriteableBitmap(new PixelSize(width, height), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Premul);
        using (var fb = bitmap.Lock())
        {
            var row = new byte[fb.RowBytes];
            for (var x = 0; x < width; x++)
            {
                row[(x * 4) + 0] = (byte)(x * 255 / width);
                row[(x * 4) + 1] = 128;
                row[(x * 4) + 2] = 64;
                row[(x * 4) + 3] = 255;
            }

            for (var y = 0; y < height; y++)
            {
                System.Runtime.InteropServices.Marshal.Copy(row, 0, fb.Address + (y * fb.RowBytes), fb.RowBytes);
            }
        }

        return bitmap;
    }

    private static (ZoomableImage Image, Window Window) MakeImage()
    {
        var image = new ZoomableImage { Source = MakeBitmap(200, 100) };
        var w = TestHost.Show(image, 400, 300);
        return (image, w);
    }

    [AvaloniaFact]
    public void Starts_Fitted_And_Centered()
    {
        var (image, w) = MakeImage();
        Assert.Equal(2, image.MinZoom, 3);
        Assert.Equal(4, image.MaxZoom, 3);
        Assert.Equal(2, image.Zoom, 3);
        Assert.False(image.IsZoomed);
        Assert.Equal(-50, image.PanOffset.Y, 1);
        w.Close();
    }

    [AvaloniaFact]
    public void Double_Tap_Toggles_Smart_Zoom()
    {
        var (image, w) = MakeImage();
        var touch = new TouchInput(w);
        touch.Tap(new Point(200, 150));
        touch.Tap(new Point(200, 150));
        Assert.Equal(4, image.Zoom, 3);
        Assert.True(image.IsZoomed);
        Assert.Equal(200, image.PanOffset.X, 1);
        touch.Tap(new Point(200, 150));
        touch.Tap(new Point(200, 150));
        Assert.Equal(2, image.Zoom, 3);
        w.Close();
    }

    [AvaloniaFact]
    public void Slow_Taps_Do_Not_Zoom()
    {
        var (image, w) = MakeImage();
        var touch = new TouchInput(w) { StepMs = 200 };
        touch.Tap(new Point(200, 150));
        touch.Tap(new Point(200, 150));
        Assert.Equal(2, image.Zoom, 3);
        w.Close();
    }

    [AvaloniaFact]
    public void Ctrl_Wheel_Zooms()
    {
        var (image, w) = MakeImage();
        w.MouseWheel(new Point(200, 150), new Vector(0, 1), RawInputModifiers.Control);
        Assert.Equal(2.2, image.Zoom, 3);
        w.MouseWheel(new Point(200, 150), new Vector(0, -5), RawInputModifiers.Control);
        Assert.Equal(2, image.Zoom, 3);
        w.Close();
    }

    [AvaloniaFact]
    public void Pan_Overscrolls_At_Half_Strength_And_Springs_Back()
    {
        var (image, w) = MakeImage();
        image.ZoomAround(4, new Point(200, 150));
        Assert.Equal(200, image.PanOffset.X, 1);
        var touch = new TouchInput(w);
        touch.Drag(new Point(100, 150), new Vector(300, 0));
        Assert.Equal(-50, image.PanOffset.X, 1);
        touch.Up(new Point(400, 150));
        TestHost.Settle(w);
        Assert.Equal(0, image.PanOffset.X, 1);
        w.Close();
    }

    [AvaloniaFact]
    public void Overscroll_Is_Limited_To_100px()
    {
        var (image, w) = MakeImage();
        image.ZoomAround(4, new Point(200, 150));
        var touch = new TouchInput(w);
        touch.Drag(new Point(10, 150), new Vector(390, 0), steps: 5);
        touch.MoveBy(new Point(400, 150), new Vector(300, 0), steps: 5);
        Assert.True(image.PanOffset.X >= -100.5, $"{image.PanOffset.X}");
        touch.Up(new Point(700, 150));
        w.Close();
    }

    [AvaloniaFact]
    public void ImageView_Swipes_Between_Images()
    {
        var a = MakeBitmap(200, 100);
        var b = MakeBitmap(100, 200);
        var view = new ImageView { Images = new object[] { a, b, MakeBitmap(50, 50) } };
        var w = TestHost.Show(view, 400, 300);
        TestHost.Settle(w);
        Assert.Same(a, view.CenterSource);
        var touch = new TouchInput(w);
        touch.Drag(new Point(300, 150), new Vector(-100, 0));
        touch.Up(new Point(200, 150));
        TestHost.Settle(w);
        Assert.Equal(1, view.SelectedIndex);
        Assert.Same(b, view.CenterSource);
        w.Close();
    }

    [AvaloniaFact]
    public void ImageView_Keeps_The_Center_Image_In_Place_After_A_Resize()
    {
        var view = new ImageView { Images = new object[] { MakeBitmap(100, 100), MakeBitmap(100, 100), MakeBitmap(100, 100) }, SelectedIndex = 1 };
        var host = new Border { Width = 200, Child = view };
        var w = TestHost.Show(new Canvas { Children = { host } }, 600, 300);
        host.Width = 500;
        host.Height = 300;
        TestHost.Settle(w);
        var center = view.CurrentImage!;
        Assert.Equal(0, center.TranslatePoint(default, view)!.Value.X, 1);
        Assert.Equal(500, center.Bounds.Width, 1);
        w.Close();
    }

    [AvaloniaFact]
    public void Zoomed_Image_Pans_Before_The_Carousel_Moves()
    {
        var a = MakeBitmap(200, 100);
        var view = new ImageView { Images = new object[] { a, MakeBitmap(100, 100) } };
        var w = TestHost.Show(view, 400, 300);
        TestHost.Settle(w);
        var image = view.CurrentImage!;
        image.ZoomAround(4, new Point(200, 150));
        var touch = new TouchInput(w);
        touch.Drag(new Point(300, 150), new Vector(-100, 0));
        touch.Up(new Point(200, 150));
        TestHost.Settle(w);
        Assert.Equal(0, view.SelectedIndex);
        Assert.True(image.PanOffset.X > 200, $"{image.PanOffset.X}");

        // At the right edge the next swipe goes to the carousel.
        touch.Drag(new Point(380, 150), new Vector(-370, 0), steps: 10);
        touch.Up(new Point(10, 150));
        TestHost.Settle(w);
        Assert.Equal(400, image.PanOffset.X, 1);
        touch.Drag(new Point(350, 150), new Vector(-150, 0));
        touch.Up(new Point(200, 150));
        TestHost.Settle(w);
        Assert.Equal(1, view.SelectedIndex);
        w.Close();
    }

    [AvaloniaFact]
    public void CroppableImage_Covers_The_Frame_And_Crops()
    {
        var crop = new CroppableImage { Source = MakeBitmap(800, 600) };
        Rect? reported = null;
        crop.CropRequested += (_, e) => reported = e.CropRect;
        var w = TestHost.Show(crop, 400, 300);
        Assert.Equal(new Rect(24, 24, 352, 252), crop.FrameRect);
        Assert.Equal(0.44, crop.MinZoom, 3);
        var bitmap = crop.Crop();
        Assert.NotNull(bitmap);
        Assert.Equal(800, bitmap!.PixelSize.Width);
        Assert.Equal(System.Math.Round(252 / 0.44), bitmap.PixelSize.Height);
        Assert.NotNull(reported);
        crop.Zoom = crop.MaxZoom;
        bitmap = crop.Crop();
        Assert.Equal(200, bitmap!.PixelSize.Width);
        w.Close();
    }
}
