using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AvaEnyo.Animations;

namespace AvaEnyo.Primitives;

/// <summary>
/// Turns the pointer events of a control into Enyo drags: a drag starts after <see cref="ThemeSettings.DragThreshold"/>
/// (4 px) of movement, on the axis the owner accepts. An accepted drag captures the pointer, so the control under it
/// (a button, an item) loses the press and does not click. Release reports a velocity from a least-squares
/// <see cref="VelocityTracker"/>.
/// </summary>
/// <remarks>
/// Positions are in top-level coordinates, so a control that moves under the pointer (a sliding view) gets steady
/// deltas. A press whose gesture recognition was turned off (a slider calls <c>PreventGestureRecognition</c>) is ignored,
/// and so is a mouse press in a text box, which selects text instead.
/// </remarks>
public sealed class DragTracker : IDisposable
{
    private readonly Control _owner;
    private readonly VelocityTracker _velocity = new();
    private IPointer? _pointer;
    private Point _start;
    private Point _last;
    private bool _disposed;

    /// <summary>Starts tracking the pointer events of <paramref name="owner"/>.</summary>
    public DragTracker(Control owner)
    {
        _owner = owner;
        owner.AddHandler(InputElement.PointerPressedEvent, OnPressed, RoutingStrategies.Bubble, handledEventsToo: true);
        owner.AddHandler(InputElement.PointerMovedEvent, OnMoved, RoutingStrategies.Bubble, handledEventsToo: true);
        owner.AddHandler(InputElement.PointerReleasedEvent, OnReleased, RoutingStrategies.Bubble, handledEventsToo: true);
        owner.AddHandler(InputElement.PointerCaptureLostEvent, OnCaptureLost, RoutingStrategies.Direct | RoutingStrategies.Bubble, handledEventsToo: true);
    }

    /// <summary>Decides whether a drag starts. Null accepts every drag.</summary>
    public Func<DragStartInfo, bool>? CanStart { get; set; }

    /// <summary>Decides whether a press is tracked at all. Null tracks every left press.</summary>
    public Func<PointerPressedEventArgs, bool>? CanTrack { get; set; }

    /// <summary>True when the mouse can drag too. Default true: Enyo dragged with the mouse on desktop.</summary>
    public bool AllowMouse { get; set; } = true;

    /// <summary>True between a press and its release or cancel.</summary>
    public bool IsTracking => _pointer is not null;

    /// <summary>True while an accepted drag runs.</summary>
    public bool IsDragging { get; private set; }

    /// <summary>The pointer type of the current press.</summary>
    public PointerType PointerType { get; private set; }

    /// <summary>A press started. Set <see cref="DragTrackerEventArgs.Capture"/> to take the pointer at once.</summary>
    public event EventHandler<DragTrackerEventArgs>? Pressed;

    /// <summary>A drag passed the threshold and was accepted.</summary>
    public event EventHandler<DragTrackerEventArgs>? Started;

    /// <summary>The pointer moved during an accepted drag.</summary>
    public event EventHandler<DragTrackerEventArgs>? Moved;

    /// <summary>An accepted drag ended with a release. <see cref="DragTrackerEventArgs.Velocity"/> is set.</summary>
    public event EventHandler<DragTrackerEventArgs>? Completed;

    /// <summary>An accepted drag ended because the pointer was taken away.</summary>
    public event EventHandler<DragTrackerEventArgs>? Canceled;

    /// <summary>A press ended without a drag.</summary>
    public event EventHandler<DragTrackerEventArgs>? Released;

    private void OnPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_disposed || _pointer is not null)
        {
            return;
        }

        var point = e.GetCurrentPoint(_owner);
        if (!point.Properties.IsLeftButtonPressed)
        {
            return;
        }

        if (e.Pointer is Pointer { IsGestureRecognitionSkipped: true })
        {
            return;
        }

        if (e.Pointer.Type == PointerType.Mouse && (!AllowMouse || IsInTextBox(e.Source)))
        {
            return;
        }

        if (CanTrack is { } canTrack && !canTrack(e))
        {
            return;
        }

        if (DragClaims.OwnerOf(e.Pointer) is { } other && other != this)
        {
            return;
        }

        _pointer = e.Pointer;
        PointerType = e.Pointer.Type;
        _start = _last = Root(e);
        _velocity.Clear();
        _velocity.Add(e.Timestamp, _start.X, _start.Y);
        var args = new DragTrackerEventArgs(e, default, default, default, _start);
        Pressed?.Invoke(this, args);
        if (args.Capture && DragClaims.Claim(e.Pointer, this))
        {
            e.Pointer.Capture(_owner);
            e.Handled = true;
        }
    }

    private void OnMoved(object? sender, PointerEventArgs e)
    {
        if (_disposed || e.Pointer != _pointer)
        {
            return;
        }

        var p = Root(e);
        _velocity.Add(e.Timestamp, p.X, p.Y);
        var delta = p - _start;
        if (!IsDragging)
        {
            if (DragClaims.OwnerOf(e.Pointer) is { } other && other != this)
            {
                Reset();
                return;
            }

            if (Math.Sqrt((delta.X * delta.X) + (delta.Y * delta.Y)) < ThemeSettings.DragThreshold)
            {
                _last = p;
                return;
            }

            var ax = Math.Abs(delta.X);
            var ay = Math.Abs(delta.Y);
            var horizontal = ax > ay;
            var ratio = horizontal ? ay / Math.Max(ax, 1e-9) : ax / Math.Max(ay, 1e-9);
            var info = new DragStartInfo(delta, horizontal, ratio < ThemeSettings.LockRatio, e.Pointer.Type, e.Source);
            if ((CanStart is { } canStart && !canStart(info)) || !DragClaims.Claim(e.Pointer, this))
            {
                // Not this tracker's axis: let an ancestor have it.
                Reset();
                return;
            }

            IsDragging = true;
            e.Pointer.Capture(_owner);
            Started?.Invoke(this, new DragTrackerEventArgs(e, delta, p - _last, default, p));
            _last = p;
            e.Handled = true;
            return;
        }

        var step = p - _last;
        _last = p;
        Moved?.Invoke(this, new DragTrackerEventArgs(e, delta, step, default, p));
        e.Handled = true;
    }

    private void OnReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_disposed || e.Pointer != _pointer)
        {
            return;
        }

        var p = Root(e);
        _velocity.Add(e.Timestamp, p.X, p.Y);
        var dragging = IsDragging;
        var (vx, vy) = _velocity.GetVelocity();
        var args = new DragTrackerEventArgs(e, p - _start, p - _last, new Vector(vx, vy), p);
        var pointer = e.Pointer;
        Reset();
        if (dragging)
        {
            e.Handled = true;
            if (pointer.Captured == _owner)
            {
                pointer.Capture(null);
            }

            Completed?.Invoke(this, args);
        }
        else
        {
            Released?.Invoke(this, args);
        }
    }

    private void OnCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (_disposed || e.Pointer != _pointer || e.Source != _owner)
        {
            return;
        }

        if (e.Pointer.Captured is Visual v && (v == _owner || _owner.IsVisualAncestorOf(v)))
        {
            return;
        }

        var dragging = IsDragging;
        Reset();
        if (dragging)
        {
            Canceled?.Invoke(this, new DragTrackerEventArgs(null, _last - _start, default, default, _last));
        }
    }

    /// <summary>Forgets the current press without raising events.</summary>
    public void Reset()
    {
        if (_pointer is { } p)
        {
            DragClaims.Release(p, this);
        }

        _pointer = null;
        IsDragging = false;
    }

    /// <summary>Stops tracking the owner.</summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Reset();
        _owner.RemoveHandler(InputElement.PointerPressedEvent, OnPressed);
        _owner.RemoveHandler(InputElement.PointerMovedEvent, OnMoved);
        _owner.RemoveHandler(InputElement.PointerReleasedEvent, OnReleased);
        _owner.RemoveHandler(InputElement.PointerCaptureLostEvent, OnCaptureLost);
    }

    private Point Root(PointerEventArgs e) => e.GetPosition(TopLevel.GetTopLevel(_owner) as Visual ?? _owner);

    private static bool IsInTextBox(object? source) =>
        source is Visual v && v.FindAncestorOfType<TextBox>(includeSelf: true) is not null;
}
