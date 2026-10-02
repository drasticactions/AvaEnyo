using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaEnyo.Animations;

namespace AvaEnyo.Primitives;

/// <summary>
/// The content presenter of the Enyo <c>ScrollViewer</c>. It ports Enyo's <c>ScrollStrategy</c>: content follows a
/// drag 1:1 and at half strength past an edge, a release flings with friction 0.97 per 10 ms, and an overscroll
/// springs back. Overscroll is drawn as a translation of the content, so <see cref="ScrollContentPresenter.Offset"/>
/// stays inside the extent.
/// </summary>
/// <remarks>
/// <para>The mouse wheel scrolls at once, without physics or overscroll. A press during a fling stops it and does
/// not click.</para>
/// <para>A drag is taken only on an axis the viewer can scroll, so a vertical list inside a horizontal SlidingPane
/// leaves horizontal drags to the pane.</para>
/// <para>It also drives the template look: the pseudo-classes <c>:scroll-up</c>, <c>:scroll-down</c>,
/// <c>:scroll-left</c> and <c>:scroll-right</c> (the content can scroll that way), <c>:fades</c> (Enyo fades on) and
/// <c>:mouse-active</c> (a mouse or pen scrolled or hovered in the last second, which shows the thin scroll bars) are set
/// on the owning <see cref="ScrollViewer"/>.</para>
/// </remarks>
public class EnyoScrollContentPresenter : ScrollContentPresenter
{
    private readonly ScrollPhysics _x = new();
    private readonly ScrollPhysics _y = new();
    private readonly DragTracker _drag;
    private FrameClock? _clock;
    private TaskCompletionSource? _settled;
    private bool _applying;
    private Vector _overscroll;
    private ITransform? _childTransform;
    private Control? _translatedChild;
    private DispatcherTimer? _mouseTimer;
    private bool _dragX;
    private bool _dragY;

    static EnyoScrollContentPresenter()
    {
        RequestBringIntoViewEvent.AddClassHandler<EnyoScrollContentPresenter>((p, e) => p.OnBringIntoViewRequested(e));
    }

    /// <summary>Initializes a new instance of the <see cref="EnyoScrollContentPresenter"/> class.</summary>
    public EnyoScrollContentPresenter()
    {
        _drag = new DragTracker(this)
        {
            CanStart = CanStartDrag,
        };
        _drag.Pressed += OnDragPressed;
        _drag.Started += OnDragStarted;
        _drag.Moved += OnDragMoved;
        _drag.Completed += OnDragCompleted;
        _drag.Canceled += OnDragCanceled;
        AddHandler(PointerMovedEvent, OnAnyPointerMoved, RoutingStrategies.Bubble, handledEventsToo: true);
    }

    /// <summary>True while a fling, spring or glide runs.</summary>
    public bool IsScrolling => _clock is not null;

    /// <summary>True while the content is dragged.</summary>
    public bool IsDragging => _drag.IsDragging;

    /// <summary>The overscroll drawn right now: how far the content is past its edges.</summary>
    public Vector Overscroll => _overscroll;

    private ScrollViewer? Owner => TemplatedParent as ScrollViewer;

    private bool UsesPhysics => (Owner ?? (Control)this) is { } c && Scroll.GetPhysics(c) == ScrollPhysicsMode.Enyo;

    /// <summary>
    /// Glides to <paramref name="offset"/> with Enyo's <c>scrollTo</c> motion. The task completes when the motion
    /// rests.
    /// </summary>
    public Task ScrollToAsync(Vector offset)
    {
        UpdateBounds();
        if (!EnyoAnimations.IsActive || !this.IsAttachedToVisualTree())
        {
            StopLoop();
            _x.SetPosition(Math.Clamp(offset.X, 0, _x.Max));
            _y.SetPosition(Math.Clamp(offset.Y, 0, _y.Max));
            Apply();
            return Task.CompletedTask;
        }

        SyncFromOffset();
        _x.ScrollTo(Math.Clamp(offset.X, 0, _x.Max));
        _y.ScrollTo(Math.Clamp(offset.Y, 0, _y.Max));
        return StartLoop();
    }

    /// <summary>Stops a fling where it is. An overscroll snaps back inside the edges, as Enyo's <c>stop()</c> did.</summary>
    public void StopScrolling()
    {
        StopLoop();
        _x.Stop();
        _y.Stop();
        if (_x.IsOverscrolled || _y.IsOverscrolled)
        {
            _x.SetPosition(Math.Clamp(_x.Position, _x.Min, Math.Max(_x.Min, _x.Max)));
            _y.SetPosition(Math.Clamp(_y.Position, _y.Min, Math.Max(_y.Min, _y.Max)));
            Apply();
        }
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == OffsetProperty && !_applying && !_drag.IsDragging)
        {
            // Someone else (wheel, keyboard, scroll bar, code) moved the content: follow it.
            StopLoop();
            ClearOverscroll();
            SyncFromOffset();
            UpdatePseudoClasses();
        }
        else if (change.Property == ExtentProperty || change.Property == ViewportProperty)
        {
            UpdateBounds();
            UpdatePseudoClasses();
        }
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdatePseudoClasses();
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        StopLoop();
        ClearOverscroll();
        _drag.Reset();
        base.OnDetachedFromVisualTree(e);
    }

    /// <inheritdoc/>
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        if (_x.IsOverscrolled || _y.IsOverscrolled || IsScrolling)
        {
            StopScrolling();
        }

        SignalMouseActivity();
        base.OnPointerWheelChanged(e);
    }

    private void OnAnyPointerMoved(object? sender, PointerEventArgs e)
    {
        if (e.Pointer.Type != PointerType.Touch)
        {
            SignalMouseActivity();
        }
    }

    private bool CanStartDrag(DragStartInfo info)
    {
        if (!UsesPhysics)
        {
            return false;
        }

        UpdateBounds();
        var canH = CanHorizontallyScroll && Extent.Width > Viewport.Width + 0.5;

        // Enyo's vertical axis bounced even when the content was short (autoVertical: false). A viewer whose vertical
        // scroll bar is disabled does not scroll vertically at all.
        var canV = CanVerticallyScroll;
        if (info.IsHorizontal ? !canH : !canV)
        {
            return false;
        }

        var lockAxis = info.IsLockable && Owner is { } o && Scroll.GetLock(o);
        _dragX = canH && (!lockAxis || info.IsHorizontal);
        _dragY = canV && (!lockAxis || info.IsVertical);
        return true;
    }

    private void OnDragPressed(object? sender, DragTrackerEventArgs e)
    {
        if (IsScrolling && !_drag.IsDragging && UsesPhysics)
        {
            // Tap to stop: the press ends the fling and is not a click.
            var moving = Math.Abs(_x.Velocity) > 0.5 || Math.Abs(_y.Velocity) > 0.5;
            StopScrolling();
            e.Capture = moving;
        }
    }

    private void OnDragStarted(object? sender, DragTrackerEventArgs e)
    {
        StopLoop();
        UpdateBounds();
        SyncFromOffset(keepOverscroll: true);
        _x.BeginDrag();
        _y.BeginDrag();
        OnDragMoved(sender, e);
    }

    private void OnDragMoved(object? sender, DragTrackerEventArgs e)
    {
        var flip = FlowDirection == FlowDirection.RightToLeft ? -1 : 1;
        if (_dragX)
        {
            _x.Drag(-e.Delta.X * flip);
        }

        if (_dragY)
        {
            _y.Drag(-e.Delta.Y);
        }

        Apply();
    }

    private void OnDragCompleted(object? sender, DragTrackerEventArgs e)
    {
        var flip = FlowDirection == FlowDirection.RightToLeft ? -1 : 1;
        _x.EndDrag(_dragX ? -e.Velocity.X * flip : 0);
        _y.EndDrag(_dragY ? -e.Velocity.Y : 0);
        _ = StartLoop();
    }

    private void OnDragCanceled(object? sender, DragTrackerEventArgs e)
    {
        _x.EndDrag(0);
        _y.EndDrag(0);
        _ = StartLoop();
    }

    private void OnBringIntoViewRequested(RequestBringIntoViewEventArgs e)
    {
        if (e.Handled || Owner is not { } owner || !Scroll.GetAnimateBringIntoView(owner) || !UsesPhysics || e.TargetObject is null || Child is null)
        {
            return;
        }

        if (e.TargetObject.TransformToVisual(Child) is not { } transform)
        {
            return;
        }

        var rect = e.TargetRect.TransformToAABB(transform);
        var x = Offset.X;
        var y = Offset.Y;
        if (rect.Left < x)
        {
            x = rect.Left;
        }
        else if (rect.Right > x + Viewport.Width)
        {
            x = Math.Min(rect.Left, rect.Right - Viewport.Width);
        }

        if (rect.Top < y)
        {
            y = rect.Top;
        }
        else if (rect.Bottom > y + Viewport.Height)
        {
            y = Math.Min(rect.Top, rect.Bottom - Viewport.Height);
        }

        e.Handled = true;
        _ = ScrollToAsync(new Vector(x, y));
    }

    private Task StartLoop()
    {
        _settled ??= new TaskCompletionSource();
        var task = _settled.Task;
        if (!EnyoAnimations.IsActive)
        {
            _x.Settle();
            _y.Settle();
            Apply();
            Complete();
            return task;
        }

        if (_clock is null)
        {
            _clock = new FrameClock(this, OnFrame);
        }

        return task;
    }

    private bool OnFrame(TimeSpan elapsed)
    {
        if (_drag.IsDragging)
        {
            return true;
        }

        UpdateBounds();
        var scaled = TimeSpan.FromMilliseconds(elapsed.TotalMilliseconds / Math.Max(1e-6, EnyoAnimations.TimeScale));

        // Enyo counted at least 16 ms per frame (60 fps); a longer frame is simulated in full, so a slow frame does not
        // slow the motion down.
        var x = _x.Step(scaled);
        var y = _y.Step(scaled);
        Apply();
        if (x || y)
        {
            return true;
        }

        _clock = null;
        Complete();
        return false;
    }

    private void StopLoop()
    {
        _clock?.Dispose();
        _clock = null;
        Complete();
    }

    private void Complete()
    {
        var s = _settled;
        _settled = null;
        s?.TrySetResult();
    }

    private void UpdateBounds()
    {
        _x.Min = 0;
        _x.Max = Math.Max(0, Extent.Width - Viewport.Width);
        _y.Min = 0;
        _y.Max = Math.Max(0, Extent.Height - Viewport.Height);
    }

    private void SyncFromOffset(bool keepOverscroll = false)
    {
        var ox = keepOverscroll ? _overscroll.X : 0;
        var oy = keepOverscroll ? _overscroll.Y : 0;
        _x.SetPosition(Offset.X + ox);
        _y.SetPosition(Offset.Y + oy);
    }

    /// <summary>Writes the simulated positions: the in-bounds part as the offset, the rest as overscroll.</summary>
    private void Apply()
    {
        var cx = Math.Clamp(_x.Position, _x.Min, Math.Max(_x.Min, _x.Max));
        var cy = Math.Clamp(_y.Position, _y.Min, Math.Max(_y.Min, _y.Max));
        _applying = true;
        try
        {
            var target = new Vector(cx, cy);
            if (Offset != target)
            {
                SetCurrentValue(OffsetProperty, target);
            }
        }
        finally
        {
            _applying = false;
        }

        SetOverscroll(new Vector(_x.Position - cx, _y.Position - cy));
        UpdatePseudoClasses();
    }

    private void SetOverscroll(Vector overscroll)
    {
        if (Math.Abs(overscroll.X) < 0.01 && Math.Abs(overscroll.Y) < 0.01)
        {
            ClearOverscroll();
            return;
        }

        _overscroll = overscroll;
        if (Child is not { } child)
        {
            return;
        }

        if (_translatedChild != child)
        {
            ClearOverscroll();
            _translatedChild = child;
            _childTransform = child.RenderTransform;
        }

        child.RenderTransform = new TranslateTransform(-overscroll.X, -overscroll.Y);
    }

    private void ClearOverscroll()
    {
        _overscroll = default;
        if (_translatedChild is { } child)
        {
            if (_childTransform is null)
            {
                child.ClearValue(RenderTransformProperty);
            }
            else
            {
                child.RenderTransform = _childTransform;
            }
        }

        _translatedChild = null;
        _childTransform = null;
    }

    private void SignalMouseActivity()
    {
        if (Owner is not { } owner)
        {
            return;
        }

        SetPseudo(owner, ":mouse-active", true);
        _mouseTimer ??= new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Background, (_, _) =>
        {
            _mouseTimer?.Stop();
            if (Owner is { } o)
            {
                SetPseudo(o, ":mouse-active", false);
            }
        });
        _mouseTimer.Stop();
        _mouseTimer.Start();
    }

    private void UpdatePseudoClasses()
    {
        if (Owner is not { } owner)
        {
            return;
        }

        const double e = 0.5;
        var maxX = Math.Max(0, Extent.Width - Viewport.Width);
        var maxY = Math.Max(0, Extent.Height - Viewport.Height);
        SetPseudo(owner, ":scroll-up", Offset.Y > e);
        SetPseudo(owner, ":scroll-down", Offset.Y < maxY - e);
        SetPseudo(owner, ":scroll-left", Offset.X > e);
        SetPseudo(owner, ":scroll-right", Offset.X < maxX - e);
        SetPseudo(owner, ":fades", Scroll.GetFades(owner));
    }

    private static void SetPseudo(StyledElement element, string name, bool value) =>
        ((IPseudoClasses)element.Classes).Set(name, value);
}
