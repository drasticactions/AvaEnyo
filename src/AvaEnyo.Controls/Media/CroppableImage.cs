using System;
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace AvaEnyo.Controls;

/// <summary>
/// A <see cref="ZoomableImage"/> under a fixed crop frame (<c>enyo.CroppableImage</c>). The image always covers the
/// frame; the area outside it is dimmed and the frame has a thin light border. Bind <see cref="ZoomableImage.Zoom"/>
/// to a slider (between <see cref="ZoomableImage.MinZoom"/> and <see cref="ZoomableImage.MaxZoom"/>), and call
/// <see cref="Crop"/> for the framed part as a bitmap.
/// </summary>
public class CroppableImage : ZoomableImage
{
    /// <summary>Defines the <see cref="CropMargin"/> property.</summary>
    public static readonly StyledProperty<Thickness> CropMarginProperty =
        AvaloniaProperty.Register<CroppableImage, Thickness>(nameof(CropMargin), new Thickness(24));

    /// <summary>Defines the <see cref="ScrimBrush"/> property.</summary>
    public static readonly StyledProperty<IBrush?> ScrimBrushProperty =
        AvaloniaProperty.Register<CroppableImage, IBrush?>(nameof(ScrimBrush));

    /// <summary>Defines the <see cref="FrameBrush"/> property.</summary>
    public static readonly StyledProperty<IBrush?> FrameBrushProperty =
        AvaloniaProperty.Register<CroppableImage, IBrush?>(nameof(FrameBrush));

    /// <summary>Defines the <see cref="CropRequested"/> event.</summary>
    public static readonly RoutedEvent<CropEventArgs> CropRequestedEvent =
        RoutedEvent.Register<CroppableImage, CropEventArgs>(nameof(CropRequested), RoutingStrategies.Bubble);

    static CroppableImage()
    {
        AffectsRender<CroppableImage>(CropMarginProperty, ScrimBrushProperty, FrameBrushProperty);
        AffectsArrange<CroppableImage>(CropMarginProperty);
        MaxZoomRatioProperty.OverrideDefaultValue<CroppableImage>(4d);
    }

    /// <summary>The space between the control edge and the crop frame. Default 24.</summary>
    public Thickness CropMargin
    {
        get => GetValue(CropMarginProperty);
        set => SetValue(CropMarginProperty, value);
    }

    /// <summary>The dimming outside the frame.</summary>
    public IBrush? ScrimBrush
    {
        get => GetValue(ScrimBrushProperty);
        set => SetValue(ScrimBrushProperty, value);
    }

    /// <summary>The frame border.</summary>
    public IBrush? FrameBrush
    {
        get => GetValue(FrameBrushProperty);
        set => SetValue(FrameBrushProperty, value);
    }

    /// <summary>Raised by <see cref="Crop"/> with the crop rectangle.</summary>
    public event EventHandler<CropEventArgs>? CropRequested
    {
        add => AddHandler(CropRequestedEvent, value);
        remove => RemoveHandler(CropRequestedEvent, value);
    }

    /// <summary>The crop frame in control coordinates.</summary>
    public Rect FrameRect => ContentFrame;

    /// <summary>The part of the source image inside the frame, in source units.</summary>
    public Rect CropRect
    {
        get
        {
            if (Source is not { } source || Zoom <= 0)
            {
                return default;
            }

            var frame = ContentFrame;
            var offset = PanOffset;
            var rect = new Rect(offset.X / Zoom, offset.Y / Zoom, frame.Width / Zoom, frame.Height / Zoom);
            return rect.Intersect(new Rect(source.Size));
        }
    }

    /// <inheritdoc/>
    protected override Rect ContentFrame => new Rect(Bounds.Size).Deflate(CropMargin);

    /// <summary>Returns the framed part of the image as a bitmap at source resolution, and raises <see cref="CropRequested"/>.</summary>
    public Bitmap? Crop()
    {
        var rect = CropRect;
        RaiseEvent(new CropEventArgs(CropRequestedEvent, this, rect, Zoom));
        if (Source is not { } source || rect.Width < 1 || rect.Height < 1)
        {
            return null;
        }

        var size = new PixelSize((int)Math.Round(rect.Width), (int)Math.Round(rect.Height));
        var bitmap = new RenderTargetBitmap(size);
        using (var context = bitmap.CreateDrawingContext())
        {
            context.DrawImage(source, rect, new Rect(0, 0, size.Width, size.Height));
        }

        return bitmap;
    }

    /// <inheritdoc/>
    protected override double ComputeMinZoom(Size image, Rect frame) =>
        image.Width <= 0 || image.Height <= 0 || frame.Width <= 0 || frame.Height <= 0
            ? 1
            : Math.Max(frame.Width / image.Width, frame.Height / image.Height);

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        base.Render(context);
        var bounds = new Rect(Bounds.Size);
        var frame = ContentFrame;
        if (ScrimBrush is { } scrim)
        {
            context.FillRectangle(scrim, new Rect(bounds.X, bounds.Y, bounds.Width, Math.Max(0, frame.Top - bounds.Top)));
            context.FillRectangle(scrim, new Rect(bounds.X, frame.Bottom, bounds.Width, Math.Max(0, bounds.Bottom - frame.Bottom)));
            context.FillRectangle(scrim, new Rect(bounds.X, frame.Top, Math.Max(0, frame.Left - bounds.Left), frame.Height));
            context.FillRectangle(scrim, new Rect(frame.Right, frame.Top, Math.Max(0, bounds.Right - frame.Right), frame.Height));
        }

        if (FrameBrush is { } brush)
        {
            context.DrawRectangle(null, new Pen(brush, 1), frame.Deflate(0.5));
        }
    }
}
