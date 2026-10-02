using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Input.GestureRecognizers;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using AvaEnyo.Animations;
using AvaEnyo.Controls.Primitives;
using AvaEnyo.Primitives;

namespace AvaEnyo.Controls;

/// <summary>
/// An image that zooms and pans (Enyo <c>SizeableImage</c>, <c>ScrollingImage</c> and <c>ViewImage</c>). It starts
/// fitted to the control. Pinch or Ctrl+wheel zooms; a double tap within 300 ms zooms between fit and
/// <see cref="MaxZoomRatio"/> times fit around the tapped point, animated over 350 ms with a cubic ease-out. A zoomed
/// image pans with Enyo's scroll physics and may overscroll up to 100 px per axis before it springs back.
/// </summary>
/// <remarks>
/// Inside a <see cref="Carousel"/>, a horizontal drag at the image's left or right edge (or on an image that is not
/// zoomed) is left to the carousel, so swiping between photos and panning a photo do not fight.
/// </remarks>
[PseudoClasses(":zoomed", ":dragging")]
public class ZoomableImage : Control
{
    /// <summary>The overscroll limit per axis: 100 px (Enyo <c>overscrollWidth</c>).</summary>
    public static double OverscrollLimit = 100;

    /// <summary>The double-tap window: 300 ms.</summary>
    public static double DoubleTapMilliseconds = 300;

    /// <summary>Defines the <see cref="Source"/> property.</summary>
    public static readonly StyledProperty<IImage?> SourceProperty =
        AvaloniaProperty.Register<ZoomableImage, IImage?>(nameof(Source));

    /// <summary>Defines the <see cref="Zoom"/> property.</summary>
    public static readonly StyledProperty<double> ZoomProperty =
        AvaloniaProperty.Register<ZoomableImage, double>(nameof(Zoom), 1d, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Defines the <see cref="MaxZoomRatio"/> property.</summary>
    public static readonly StyledProperty<double> MaxZoomRatioProperty =
        AvaloniaProperty.Register<ZoomableImage, double>(nameof(MaxZoomRatio), 2d);

    /// <summary>Defines the <see cref="IsZoomEnabled"/> property.</summary>
    public static readonly StyledProperty<bool> IsZoomEnabledProperty =
        AvaloniaProperty.Register<ZoomableImage, bool>(nameof(IsZoomEnabled), true);

    /// <summary>Defines the <see cref="MinZoom"/> property.</summary>
    public static readonly DirectProperty<ZoomableImage, double> MinZoomProperty =
        AvaloniaProperty.RegisterDirect<ZoomableImage, double>(nameof(MinZoom), o => o.MinZoom);

    /// <summary>Defines the <see cref="MaxZoom"/> property.</summary>
    public static readonly DirectProperty<ZoomableImage, double> MaxZoomProperty =
        AvaloniaProperty.RegisterDirect<ZoomableImage, double>(nameof(MaxZoom), o => o.MaxZoom);

    private readonly ScrollPhysics _x = new();
    private readonly ScrollPhysics _y = new();
    private readonly PhysicsLoop _loop;
    private readonly DragTracker _drag;
    private double _minZoom = 1;
    private double _maxZoom = 2;
    private bool _settingZoom;
    private bool _fit = true;
    private ulong _lastTapTime;
    private Point _lastTapPoint;
    private bool _hasLastTap;
    private double _pinchStartZoom;
    private bool _pinching;
    private int _zoomGeneration;

    static ZoomableImage()
    {
        ClipToBoundsProperty.OverrideDefaultValue<ZoomableImage>(true);
        FocusableProperty.OverrideDefaultValue<ZoomableImage>(false);
        AffectsRender<ZoomableImage>(SourceProperty, ZoomProperty);
        SourceProperty.Changed.AddClassHandler<ZoomableImage>((z, _) => z.ResetZoom());
        MaxZoomRatioProperty.Changed.AddClassHandler<ZoomableImage>((z, _) => z.UpdateLimits());
        ZoomProperty.Changed.AddClassHandler<ZoomableImage>((z, e) => z.OnZoomChanged(e.GetOldValue<double>(), e.GetNewValue<double>()));
    }

    /// <summary>Initializes a new instance of the <see cref="ZoomableImage"/> class.</summary>
    public ZoomableImage()
    {
        _loop = new PhysicsLoop(this);
        _drag = new DragTracker(this) { CanStart = CanStartDrag };
        _drag.Started += (_, _) => OnDragStarted();
        _drag.Moved += (_, e) => OnDragMoved(e);
        _drag.Completed += (_, e) => OnDragFinished(e.Velocity);
        _drag.Canceled += (_, _) => OnDragFinished(default);
        _drag.Released += (_, e) => OnTap(e);
        GestureRecognizers.Add(new PinchGestureRecognizer());
        AddHandler(InputElement.PinchEvent, OnPinch);
        AddHandler(InputElement.PinchEndedEvent, (_, _) => _pinching = false);
    }

    /// <summary>The image.</summary>
    public IImage? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>The scale of the image: 1 is 100 %. Kept between <see cref="MinZoom"/> and <see cref="MaxZoom"/>.</summary>
    public double Zoom
    {
        get => GetValue(ZoomProperty);
        set => SetValue(ZoomProperty, value);
    }

    /// <summary>How far the image can zoom in, as a multiple of the fitted scale. Default 2.</summary>
    public double MaxZoomRatio
    {
        get => GetValue(MaxZoomRatioProperty);
        set => SetValue(MaxZoomRatioProperty, value);
    }

    /// <summary>True (default): pinch, wheel and double tap zoom.</summary>
    public bool IsZoomEnabled
    {
        get => GetValue(IsZoomEnabledProperty);
        set => SetValue(IsZoomEnabledProperty, value);
    }

    /// <summary>The fitted scale, the smallest zoom.</summary>
    public double MinZoom
    {
        get => _minZoom;
        private set => SetAndRaise(MinZoomProperty, ref _minZoom, value);
    }

    /// <summary>The largest zoom: <see cref="MinZoom"/> × <see cref="MaxZoomRatio"/>.</summary>
    public double MaxZoom
    {
        get => _maxZoom;
        private set => SetAndRaise(MaxZoomProperty, ref _maxZoom, value);
    }

    /// <summary>True when the image is zoomed in past the fit.</summary>
    public bool IsZoomed => Zoom > MinZoom + 1e-6;

    /// <summary>The pan position: the offset of the frame into the scaled image, overscroll included.</summary>
    public Vector PanOffset => new(_x.Position, _y.Position);

    /// <summary>The pan limits on each axis at rest.</summary>
    public Rect PanLimits => new(_x.Min, _y.Min, _x.Max - _x.Min, _y.Max - _y.Min);

    /// <summary>The area the image fits into and pans within. The whole control by default.</summary>
    protected virtual Rect ContentFrame => new(Bounds.Size);

    /// <summary>Returns the image to the fitted scale, centered.</summary>
    public void ResetZoom()
    {
        _zoomGeneration++;
        _loop.Stop();
        _fit = true;
        UpdateLimits();
        SetZoomSilently(MinZoom);
        UpdateLimits();
        _x.SetPosition((_x.Min + _x.Max) / 2);
        _y.SetPosition((_y.Min + _y.Max) / 2);
        InvalidateVisual();
    }

    /// <summary>
    /// Zooms between the fit and the largest zoom around <paramref name="point"/> (control coordinates), animated
    /// (Enyo <c>smartZoom</c>).
    /// </summary>
    public void SmartZoom(Point point)
    {
        if (!IsZoomEnabled || Source is null)
        {
            return;
        }

        var from = Zoom;
        var to = Math.Abs(Zoom - MinZoom) > 1e-3 ? MinZoom : MaxZoom;
        var generation = ++_zoomGeneration;
        _loop.Stop();
        _ = EnyoAnimations.Animate(from, to, TimeSpan.FromMilliseconds(EnyoDurations.SmartZoom), EnyoEasing.CubicOut, v =>
        {
            if (generation == _zoomGeneration)
            {
                ZoomAround(v, point);
            }
        }, this);
    }

    /// <summary>Sets the zoom keeping the image point under <paramref name="point"/> (control coordinates) in place.</summary>
    public void ZoomAround(double zoom, Point point)
    {
        if (Source is null)
        {
            return;
        }

        UpdateLimits();
        var old = Zoom;
        var z = Math.Clamp(zoom, MinZoom, MaxZoom);
        var frame = ContentFrame;
        var local = point - frame.TopLeft;
        var nx = ((_x.Position + local.X) * (z / old)) - local.X;
        var ny = ((_y.Position + local.Y) * (z / old)) - local.Y;
        SetZoomSilently(z);
        _fit = Math.Abs(z - MinZoom) < 1e-6;
        UpdateLimits();
        _x.SetPosition(Math.Clamp(nx, _x.Min, _x.Max));
        _y.SetPosition(Math.Clamp(ny, _y.Min, _y.Max));
        CenterIfFits();
        InvalidateVisual();
    }

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (Source is not { } source)
        {
            return;
        }

        var frame = ContentFrame;
        var size = source.Size;
        var dest = new Rect(frame.X - _x.Position, frame.Y - _y.Position, size.Width * Zoom, size.Height * Zoom);
        context.DrawImage(source, new Rect(size), dest);
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        var size = Source?.Size ?? default;
        return new Size(
            double.IsInfinity(availableSize.Width) ? size.Width : availableSize.Width,
            double.IsInfinity(availableSize.Height) ? size.Height : availableSize.Height);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = base.ArrangeOverride(finalSize);
        Avalonia.Threading.Dispatcher.UIThread.Post(OnSizeSettled, Avalonia.Threading.DispatcherPriority.Render);
        return size;
    }

    /// <summary>Computes the fitted scale. The default fits the whole image inside the frame.</summary>
    protected virtual double ComputeMinZoom(Size image, Rect frame) =>
        image.Width <= 0 || image.Height <= 0 || frame.Width <= 0 || frame.Height <= 0
            ? 1
            : Math.Min(frame.Width / image.Width, frame.Height / image.Height);

    /// <inheritdoc/>
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        if (IsZoomEnabled && Source is not null && e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.Delta.Y != 0)
        {
            _zoomGeneration++;
            ZoomAround(Zoom * Math.Pow(1.1, e.Delta.Y), e.GetPosition(this));
            e.Handled = true;
        }
    }

    private void OnSizeSettled()
    {
        var wasFit = _fit;
        UpdateLimits();
        if (wasFit || Zoom < MinZoom || Zoom > MaxZoom)
        {
            SetZoomSilently(wasFit ? MinZoom : Math.Clamp(Zoom, MinZoom, MaxZoom));
            UpdateLimits();
        }

        if (wasFit && !_drag.IsDragging && !_loop.IsRunning)
        {
            _x.SetPosition((_x.Min + _x.Max) / 2);
            _y.SetPosition((_y.Min + _y.Max) / 2);
        }
        else if (!_drag.IsDragging && !_loop.IsRunning)
        {
            _x.SetPosition(Math.Clamp(_x.Position, _x.Min, _x.Max));
            _y.SetPosition(Math.Clamp(_y.Position, _y.Min, _y.Max));
            CenterIfFits();
        }

        InvalidateVisual();
    }

    private void OnZoomChanged(double oldZoom, double newZoom)
    {
        PseudoClasses.Set(":zoomed", IsZoomed);
        if (_settingZoom || oldZoom <= 0)
        {
            return;
        }

        // An outside change (a bound slider) zooms around the middle of the frame.
        var frame = ContentFrame;
        _settingZoom = true;
        try
        {
            SetCurrentValue(ZoomProperty, oldZoom);
        }
        finally
        {
            _settingZoom = false;
        }

        ZoomAround(newZoom, frame.Center);
    }

    private void SetZoomSilently(double zoom)
    {
        _settingZoom = true;
        try
        {
            SetCurrentValue(ZoomProperty, zoom);
        }
        finally
        {
            _settingZoom = false;
        }

        PseudoClasses.Set(":zoomed", IsZoomed);
    }

    private void UpdateLimits()
    {
        var frame = ContentFrame;
        var image = Source?.Size ?? default;
        MinZoom = ComputeMinZoom(image, frame);
        MaxZoom = MinZoom * Math.Max(1, MaxZoomRatio);
        var cw = image.Width * Zoom;
        var ch = image.Height * Zoom;
        SetAxis(_x, cw, frame.Width);
        SetAxis(_y, ch, frame.Height);
    }

    private static void SetAxis(ScrollPhysics axis, double content, double frame)
    {
        if (content <= frame)
        {
            axis.Min = axis.Max = -(frame - content) / 2;
        }
        else
        {
            axis.Min = 0;
            axis.Max = content - frame;
        }
    }

    private void CenterIfFits()
    {
        if (_x.Min == _x.Max)
        {
            _x.SetPosition(_x.Min);
        }

        if (_y.Min == _y.Max)
        {
            _y.SetPosition(_y.Min);
        }
    }

    private bool CanPanX => _x.Max > _x.Min + 0.5;

    private bool CanPanY => _y.Max > _y.Min + 0.5;

    private bool CanStartDrag(DragStartInfo info)
    {
        if (_pinching || Source is null)
        {
            return false;
        }

        UpdateLimits();
        if (info.IsHorizontal)
        {
            if (!CanPanX)
            {
                return false;
            }

            if (this.FindAncestorOfType<Carousel>() is not null)
            {
                // At the edge the swipe belongs to the carousel.
                return info.Delta.X > 0 ? _x.Position > _x.Min + 0.5 : _x.Position < _x.Max - 0.5;
            }

            return true;
        }

        return CanPanY;
    }

    private void OnDragStarted()
    {
        _zoomGeneration++;
        _loop.Stop();
        PseudoClasses.Set(":dragging", true);
        UpdateLimits();
        _x.BeginDrag();
        _y.BeginDrag();
    }

    private void OnDragMoved(DragTrackerEventArgs e)
    {
        if (CanPanX)
        {
            _x.Drag(-e.Delta.X);
        }

        if (CanPanY)
        {
            _y.Drag(-e.Delta.Y);
        }

        ClampOverscroll();
        InvalidateVisual();
    }

    private void OnDragFinished(Vector velocity)
    {
        PseudoClasses.Set(":dragging", false);
        _x.EndDrag(CanPanX ? -velocity.X : 0);
        _y.EndDrag(CanPanY ? -velocity.Y : 0);
        StartLoop();
    }

    private void StartLoop()
    {
        _loop.Start(
            elapsed =>
            {
                var x = _x.Step(elapsed);
                var y = _y.Step(elapsed);
                ClampOverscroll();
                InvalidateVisual();
                return x || y;
            },
            () =>
            {
                _x.Settle();
                _y.Settle();
                InvalidateVisual();
            },
            InvalidateVisual);
    }

    private void ClampOverscroll()
    {
        if (_x.Position < _x.Min - OverscrollLimit || _x.Position > _x.Max + OverscrollLimit)
        {
            _x.SetPosition(Math.Clamp(_x.Position, _x.Min - OverscrollLimit, _x.Max + OverscrollLimit));
        }

        if (_y.Position < _y.Min - OverscrollLimit || _y.Position > _y.Max + OverscrollLimit)
        {
            _y.SetPosition(Math.Clamp(_y.Position, _y.Min - OverscrollLimit, _y.Max + OverscrollLimit));
        }
    }

    private void OnTap(DragTrackerEventArgs e)
    {
        if (e.Pointer is not { } pointer)
        {
            return;
        }

        var point = pointer.GetPosition(this);
        var time = pointer.Timestamp;
        if (_hasLastTap && time - _lastTapTime <= DoubleTapMilliseconds && Math.Abs(point.X - _lastTapPoint.X) < 30 && Math.Abs(point.Y - _lastTapPoint.Y) < 30)
        {
            _hasLastTap = false;
            SmartZoom(point);
            return;
        }

        _hasLastTap = true;
        _lastTapTime = time;
        _lastTapPoint = point;
    }

    private void OnPinch(object? sender, PinchEventArgs e)
    {
        if (!IsZoomEnabled || Source is null)
        {
            return;
        }

        if (!_pinching)
        {
            _pinching = true;
            _pinchStartZoom = Zoom;
            _drag.Reset();
        }

        _zoomGeneration++;
        ZoomAround(_pinchStartZoom * e.Scale, e.ScaleOrigin);
        e.Handled = true;
    }
}
