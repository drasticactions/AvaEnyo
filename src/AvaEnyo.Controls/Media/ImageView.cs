using System;
using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace AvaEnyo.Controls;

/// <summary>
/// A full-screen photo viewer (<c>enyo.ImageView</c>): a <see cref="VirtualCarousel"/> whose three reused views are
/// <see cref="ZoomableImage"/>s. Swipe between <see cref="Images"/>; pinch or double tap to zoom a photo. An image may
/// be an <see cref="IImage"/>, or a string or <see cref="Uri"/> (an <c>avares://</c> asset or a file path).
/// </summary>
public class ImageView : VirtualCarousel
{
    /// <summary>Defines the <see cref="Images"/> property.</summary>
    public static readonly StyledProperty<IList?> ImagesProperty =
        AvaloniaProperty.Register<ImageView, IList?>(nameof(Images));

    /// <summary>Defines the <see cref="CenterSource"/> property.</summary>
    public static readonly DirectProperty<ImageView, IImage?> CenterSourceProperty =
        AvaloniaProperty.RegisterDirect<ImageView, IImage?>(nameof(CenterSource), o => o.CenterSource);

    private IImage? _centerSource;

    static ImageView()
    {
        ImagesProperty.Changed.AddClassHandler<ImageView>((v, e) => v.ItemsSource = e.GetNewValue<IList?>());
        SnapFinishedEvent.AddClassHandler<ImageView>((v, _) => v.UpdateCenterSource());
    }

    /// <summary>The images, in order.</summary>
    public IList? Images
    {
        get => GetValue(ImagesProperty);
        set => SetValue(ImagesProperty, value);
    }

    /// <summary>The image shown now.</summary>
    public IImage? CenterSource
    {
        get => _centerSource;
        private set => SetAndRaise(CenterSourceProperty, ref _centerSource, value);
    }

    /// <summary>The zoomable image shown now.</summary>
    public ZoomableImage? CurrentImage => CurrentView as ZoomableImage;

    /// <summary>Turns an image item into an <see cref="IImage"/>. Strings and URIs load as bitmaps.</summary>
    public static IImage? Resolve(object? item)
    {
        switch (item)
        {
            case IImage image:
                return image;
            case Uri uri:
                return Load(uri);
            case string s when !string.IsNullOrEmpty(s):
                return Load(new Uri(s, s.Contains("://", StringComparison.Ordinal) ? UriKind.Absolute : UriKind.RelativeOrAbsolute));
            default:
                return null;
        }
    }

    /// <inheritdoc/>
    protected override Control CreateView() => new ZoomableImage();

    /// <inheritdoc/>
    protected override void ApplyItem(Control view, object? item)
    {
        if (view is ZoomableImage image)
        {
            image.Source = Resolve(item);
            image.ResetZoom();
        }
    }

    /// <inheritdoc/>
    protected override bool SetupView(Control view, int index)
    {
        var shown = base.SetupView(view, index);
        Avalonia.Threading.Dispatcher.UIThread.Post(UpdateCenterSource);
        return shown;
    }

    private void UpdateCenterSource() => CenterSource = CurrentImage?.Source;

    private static IImage? Load(Uri uri)
    {
        try
        {
            if (uri.IsAbsoluteUri && uri.Scheme == "avares")
            {
                using var stream = AssetLoader.Open(uri);
                return new Bitmap(stream);
            }

            var path = uri.IsAbsoluteUri ? uri.LocalPath : uri.OriginalString;
            return new Bitmap(path);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
