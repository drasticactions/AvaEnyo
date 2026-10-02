using System;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvaEnyo.Animations;

namespace AvaEnyo.Controls.Primitives;

/// <summary>
/// Enyo's synthesized gestures (<c>enyo.gesture</c>) as routed events, raised on the pressed element and bubbling:
/// <c>Hold</c> after 200 ms without a drag, <c>HoldPulse</c> every 100 ms after that, <c>DragStarted</c> once the
/// pointer moves 4 px, and <c>Flick</c> when a release is fast. A drag start cancels the hold.
/// <c>BackRequested</c> is the back chain event (Escape, system back).
/// </summary>
public static class EnyoGestures
{
    private static readonly ConditionalWeakTable<IPointer, Tracker> s_trackers = new();

    /// <summary>Defines the <c>Hold</c> routed event.</summary>
    public static readonly RoutedEvent<HoldEventArgs> HoldEvent =
        RoutedEvent.Register<HoldEventArgs>("Hold", RoutingStrategies.Bubble, typeof(EnyoGestures));

    /// <summary>Defines the <c>HoldPulse</c> routed event.</summary>
    public static readonly RoutedEvent<HoldEventArgs> HoldPulseEvent =
        RoutedEvent.Register<HoldEventArgs>("HoldPulse", RoutingStrategies.Bubble, typeof(EnyoGestures));

    /// <summary>Defines the <c>DragStarted</c> routed event.</summary>
    public static readonly RoutedEvent<DragStartedEventArgs> DragStartedEvent =
        RoutedEvent.Register<DragStartedEventArgs>("DragStarted", RoutingStrategies.Bubble, typeof(EnyoGestures));

    /// <summary>Defines the <c>Flick</c> routed event.</summary>
    public static readonly RoutedEvent<FlickEventArgs> FlickEvent =
        RoutedEvent.Register<FlickEventArgs>("Flick", RoutingStrategies.Bubble, typeof(EnyoGestures));

    /// <summary>The <c>BackRequested</c> routed event (the same event as <see cref="EnyoBack.BackRequestedEvent"/>).</summary>
    public static readonly RoutedEvent<RoutedEventArgs> BackRequestedEvent = EnyoBack.BackRequestedEvent;

    static EnyoGestures()
    {
        InputElement.PointerPressedEvent.AddClassHandler<TopLevel>(OnPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
        InputElement.PointerMovedEvent.AddClassHandler<TopLevel>(OnMoved, RoutingStrategies.Tunnel, handledEventsToo: true);
        InputElement.PointerReleasedEvent.AddClassHandler<TopLevel>(OnReleased, RoutingStrategies.Tunnel, handledEventsToo: true);
        InputElement.PointerCaptureLostEvent.AddClassHandler<InputElement>(OnCaptureLost, RoutingStrategies.Direct | RoutingStrategies.Bubble, handledEventsToo: true);
    }

    /// <summary>Time a press must last to become a hold, in ms. Default 200.</summary>
    public static double HoldDelay
    {
        get => ThemeSettings.HoldDelay;
        set => ThemeSettings.HoldDelay = value;
    }

    /// <summary>Interval of the hold pulse, in ms. Default 100.</summary>
    public static double PulseInterval
    {
        get => ThemeSettings.PulseInterval;
        set => ThemeSettings.PulseInterval = value;
    }

    /// <summary>Distance a pointer moves before a drag starts, in px. Default 4.</summary>
    public static double DragThreshold
    {
        get => ThemeSettings.DragThreshold;
        set => ThemeSettings.DragThreshold = value;
    }

    /// <summary>Release speed above which a <c>Flick</c> is raised, in px/ms. Default 0.5.</summary>
    public static double FlickThreshold { get; set; } = 0.5;

    /// <summary>Makes sure the class handlers are registered.</summary>
    public static void EnsureRegistered()
    {
    }

    /// <summary>Adds a <c>Hold</c> handler.</summary>
    public static void AddHoldHandler(Interactive element, EventHandler<HoldEventArgs> handler) => element.AddHandler(HoldEvent, handler);

    /// <summary>Removes a <c>Hold</c> handler.</summary>
    public static void RemoveHoldHandler(Interactive element, EventHandler<HoldEventArgs> handler) => element.RemoveHandler(HoldEvent, handler);

    /// <summary>Adds a <c>HoldPulse</c> handler.</summary>
    public static void AddHoldPulseHandler(Interactive element, EventHandler<HoldEventArgs> handler) => element.AddHandler(HoldPulseEvent, handler);

    /// <summary>Removes a <c>HoldPulse</c> handler.</summary>
    public static void RemoveHoldPulseHandler(Interactive element, EventHandler<HoldEventArgs> handler) => element.RemoveHandler(HoldPulseEvent, handler);

    /// <summary>Adds a <c>DragStarted</c> handler.</summary>
    public static void AddDragStartedHandler(Interactive element, EventHandler<DragStartedEventArgs> handler) => element.AddHandler(DragStartedEvent, handler);

    /// <summary>Removes a <c>DragStarted</c> handler.</summary>
    public static void RemoveDragStartedHandler(Interactive element, EventHandler<DragStartedEventArgs> handler) => element.RemoveHandler(DragStartedEvent, handler);

    /// <summary>Adds a <c>Flick</c> handler.</summary>
    public static void AddFlickHandler(Interactive element, EventHandler<FlickEventArgs> handler) => element.AddHandler(FlickEvent, handler);

    /// <summary>Removes a <c>Flick</c> handler.</summary>
    public static void RemoveFlickHandler(Interactive element, EventHandler<FlickEventArgs> handler) => element.RemoveHandler(FlickEvent, handler);

    /// <summary>Adds a <c>BackRequested</c> handler.</summary>
    public static void AddBackRequestedHandler(Interactive element, EventHandler<RoutedEventArgs> handler) => element.AddHandler(BackRequestedEvent, handler);

    /// <summary>Removes a <c>BackRequested</c> handler.</summary>
    public static void RemoveBackRequestedHandler(Interactive element, EventHandler<RoutedEventArgs> handler) => element.RemoveHandler(BackRequestedEvent, handler);

    private static void OnPressed(TopLevel top, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(top).Properties.IsLeftButtonPressed || e.Source is not Interactive source)
        {
            return;
        }

        if (s_trackers.TryGetValue(e.Pointer, out var old))
        {
            old.Stop();
        }

        var tracker = new Tracker(top, source, e.GetPosition(top), e.Timestamp, e.Pointer.Type);
        s_trackers.AddOrUpdate(e.Pointer, tracker);
        tracker.Start();
    }

    private static void OnMoved(TopLevel top, PointerEventArgs e)
    {
        if (s_trackers.TryGetValue(e.Pointer, out var tracker) && tracker.Top == top)
        {
            tracker.Move(e.GetPosition(top), e.Timestamp);
        }
    }

    private static void OnReleased(TopLevel top, PointerReleasedEventArgs e)
    {
        if (!s_trackers.TryGetValue(e.Pointer, out var tracker) || tracker.Top != top)
        {
            return;
        }

        s_trackers.Remove(e.Pointer);
        tracker.Release(e.GetPosition(top), e.Timestamp);
    }

    private static void OnCaptureLost(InputElement element, PointerCaptureLostEventArgs e)
    {
        // A capture taken by a dragging control ends the hold, as Enyo's dragstart stopped mousehold.
        if (s_trackers.TryGetValue(e.Pointer, out var tracker) && e.Pointer.Captured is not null && e.Pointer.Captured != tracker.Source)
        {
            tracker.CancelHold();
        }
    }

    private sealed class Tracker
    {
        private readonly VelocityTracker _velocity = new();
        private readonly Point _start;
        private readonly PointerType _type;
        private DispatcherTimer? _hold;
        private DispatcherTimer? _pulse;
        private bool _dragging;
        private bool _held;

        public Tracker(TopLevel top, Interactive source, Point start, ulong timestamp, PointerType type)
        {
            Top = top;
            Source = source;
            _start = start;
            _type = type;
            _velocity.Add(timestamp, start.X, start.Y);
        }

        public TopLevel Top { get; }

        public Interactive Source { get; }

        public void Start()
        {
            _hold = new DispatcherTimer(TimeSpan.FromMilliseconds(Math.Max(1, ThemeSettings.HoldDelay)), DispatcherPriority.Input, (_, _) => OnHold());
            _hold.Start();
        }

        public void Move(Point p, ulong time)
        {
            _velocity.Add(time, p.X, p.Y);
            if (_dragging)
            {
                return;
            }

            var d = p - _start;
            if (Math.Sqrt((d.X * d.X) + (d.Y * d.Y)) < ThemeSettings.DragThreshold)
            {
                return;
            }

            _dragging = true;
            CancelHold();
            var ax = Math.Abs(d.X);
            var ay = Math.Abs(d.Y);
            var horizontal = ax > ay;
            var ratio = horizontal ? ay / Math.Max(ax, 1e-9) : ax / Math.Max(ay, 1e-9);
            Source.RaiseEvent(new DragStartedEventArgs(DragStartedEvent, Source, d, horizontal, ratio < ThemeSettings.LockRatio, _type));
        }

        public void Release(Point p, ulong time)
        {
            _velocity.Add(time, p.X, p.Y);
            CancelHold();
            var (vx, vy) = _velocity.GetVelocity();
            if (Math.Sqrt((vx * vx) + (vy * vy)) >= FlickThreshold)
            {
                Source.RaiseEvent(new FlickEventArgs(FlickEvent, Source, vx, vy));
            }
        }

        public void CancelHold()
        {
            _hold?.Stop();
            _hold = null;
            _pulse?.Stop();
            _pulse = null;
        }

        public void Stop() => CancelHold();

        private void OnHold()
        {
            _hold?.Stop();
            _hold = null;
            if (_dragging || _held)
            {
                return;
            }

            _held = true;
            Source.RaiseEvent(new HoldEventArgs(HoldEvent, Source, TimeSpan.FromMilliseconds(ThemeSettings.HoldDelay), _start, _type));
            var elapsed = ThemeSettings.HoldDelay;
            _pulse = new DispatcherTimer(TimeSpan.FromMilliseconds(Math.Max(1, ThemeSettings.PulseInterval)), DispatcherPriority.Input, (_, _) =>
            {
                elapsed += ThemeSettings.PulseInterval;
                Source.RaiseEvent(new HoldEventArgs(HoldPulseEvent, Source, TimeSpan.FromMilliseconds(elapsed), _start, _type));
            });
            _pulse.Start();
        }
    }
}
