using System;
using System.Collections;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using AvaEnyo.Animations;
using AvaEnyo.Controls.Primitives;
using AvaEnyo.Primitives;

namespace AvaEnyo.Controls;

public partial class VirtualList
{
    /// <summary>The time neighbors take to slide out of the way during a reorder: 200 ms, cubic ease-out.</summary>
    public static double ReorderSlideMilliseconds = 200;

    /// <summary>The scale of a lifted row: 1.03.</summary>
    public static double ReorderLiftScale = 1.03;

    /// <summary>The distance from an edge within which a reorder drag scrolls the list: 40 px.</summary>
    public static double ReorderAutoScrollZone = 40;

    /// <summary>Defines the <see cref="CanReorder"/> property.</summary>
    public static readonly StyledProperty<bool> CanReorderProperty =
        AvaloniaProperty.Register<VirtualList, bool>(nameof(CanReorder));

    /// <summary>Defines the <see cref="Reordering"/> event.</summary>
    public static readonly RoutedEvent<ReorderingEventArgs> ReorderingEvent =
        RoutedEvent.Register<VirtualList, ReorderingEventArgs>(nameof(Reordering), RoutingStrategies.Bubble);

    private readonly Dictionary<Control, (TranslateTransform Translate, double Target)> _shifted = new();
    private DragTracker? _reorderClaim;
    private IPointer? _pressPointer;
    private Point _pressPoint;
    private IPointer? _reorderPointer;
    private Control? _lifted;
    private TranslateTransform? _liftTranslate;
    private int _reorderFrom = -1;
    private int _reorderTo = -1;
    private double _reorderHeight;
    private double _reorderStartOffset;

    /// <summary>
    /// True: holding a row for 200 ms lifts it, and dragging moves it to a new place (an AvaEnyo extension; Enyo lists
    /// had no reorder). Works with the list layout.
    /// </summary>
    public bool CanReorder
    {
        get => GetValue(CanReorderProperty);
        set => SetValue(CanReorderProperty, value);
    }

    /// <summary>True while a row is lifted for reordering.</summary>
    public bool IsReordering => _lifted is not null;

    /// <summary>
    /// Raised when a lifted row is dropped at a new index; cancelable. Unless canceled, the item moves in
    /// <see cref="ItemsControl.ItemsSource"/> when it is a changeable <see cref="IList"/>.
    /// </summary>
    public event EventHandler<ReorderingEventArgs>? Reordering
    {
        add => AddHandler(ReorderingEvent, value);
        remove => RemoveHandler(ReorderingEvent, value);
    }

    private void InitializeReorder()
    {
        _reorderClaim = new DragTracker(this) { CanTrack = _ => false };
        AddHandler(PointerPressedEvent, OnReorderPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
        AddHandler(PointerMovedEvent, OnReorderMoved, RoutingStrategies.Bubble, handledEventsToo: true);
        AddHandler(PointerReleasedEvent, OnReorderReleased, RoutingStrategies.Bubble, handledEventsToo: true);
        AddHandler(PointerCaptureLostEvent, OnReorderCaptureLost, RoutingStrategies.Direct | RoutingStrategies.Bubble, handledEventsToo: true);
        AddHandler(EnyoGestures.HoldEvent, OnReorderHold, RoutingStrategies.Bubble, handledEventsToo: true);
    }

    private void OnReorderPressed(object? sender, PointerPressedEventArgs e)
    {
        _pressPointer = e.Pointer;
        _pressPoint = e.GetPosition(this);
    }

    private void OnReorderHold(object? sender, HoldEventArgs e)
    {
        if (!CanReorder || Layout == VirtualListLayout.Grid || _lifted is not null || _pressPointer is not { } pointer || e.Source is not Visual source)
        {
            return;
        }

        Control? container = null;
        var index = -1;
        for (var v = source; v is not null && v != this; v = v.GetVisualParent())
        {
            if (v is Control c && IndexFromContainer(c) is var i && i >= 0)
            {
                container = c;
                index = i;
                break;
            }
        }

        if (container is null || _reorderClaim is null || !DragClaims.Claim(pointer, _reorderClaim))
        {
            return;
        }

        e.Handled = true;
        _reorderPointer = pointer;
        _lifted = container;
        _reorderFrom = _reorderTo = index;
        _reorderHeight = Math.Max(1, container.Bounds.Height);
        _reorderStartOffset = ScrollViewer?.Offset.Y ?? 0;
        _liftTranslate = new TranslateTransform();
        container.RenderTransform = new TransformGroup { Children = { new ScaleTransform(ReorderLiftScale, ReorderLiftScale), _liftTranslate } };
        container.RenderTransformOrigin = RelativePoint.Center;
        container.ZIndex = 1000;
        ((IPseudoClasses)container.Classes).Set(":reordering", true);
        PseudoClasses.Set(":reordering", true);
        pointer.Capture(this);
    }

    private void OnReorderMoved(object? sender, PointerEventArgs e)
    {
        if (_lifted is null || e.Pointer != _reorderPointer || _liftTranslate is null)
        {
            return;
        }

        e.Handled = true;
        var p = e.GetPosition(this);
        AutoScroll(p.Y);
        var scrolled = (ScrollViewer?.Offset.Y ?? 0) - _reorderStartOffset;
        var total = p.Y - _pressPoint.Y + scrolled;
        _liftTranslate.Y = total;
        var target = Math.Clamp(_reorderFrom + (int)Math.Round(total / _reorderHeight), 0, Math.Max(0, ItemCount - 1));
        if (target != _reorderTo)
        {
            _reorderTo = target;
            ShiftNeighbors();
        }
    }

    private void OnReorderReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_lifted is null || e.Pointer != _reorderPointer)
        {
            return;
        }

        e.Handled = true;
        EndReorder(commit: true);
    }

    private void OnReorderCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (_lifted is not null && e.Pointer == _reorderPointer && e.Pointer.Captured != this)
        {
            EndReorder(commit: false);
        }
    }

    private void ShiftNeighbors()
    {
        foreach (var container in GetRealizedContainers())
        {
            if (container == _lifted)
            {
                continue;
            }

            var i = IndexFromContainer(container);
            var shift = 0d;
            if (i > _reorderFrom && i <= _reorderTo)
            {
                shift = -_reorderHeight;
            }
            else if (i < _reorderFrom && i >= _reorderTo)
            {
                shift = _reorderHeight;
            }

            if (!_shifted.TryGetValue(container, out var state))
            {
                if (shift == 0)
                {
                    continue;
                }

                state = (new TranslateTransform(), 0);
                container.RenderTransform = state.Translate;
            }

            if (state.Target == shift)
            {
                continue;
            }

            var translate = state.Translate;
            _shifted[container] = (translate, shift);
            _ = EnyoAnimations.Animate(translate.Y, shift, TimeSpan.FromMilliseconds(ReorderSlideMilliseconds), EnyoEasing.CubicOut, v => translate.Y = v, this);
        }
    }

    private void AutoScroll(double y)
    {
        if (ScrollViewer is not { } sv)
        {
            return;
        }

        var height = sv.Bounds.Height;
        if (y < ReorderAutoScrollZone)
        {
            sv.Offset = new Vector(sv.Offset.X, Math.Max(0, sv.Offset.Y - 10));
        }
        else if (y > height - ReorderAutoScrollZone)
        {
            sv.Offset = new Vector(sv.Offset.X, Math.Min(Math.Max(0, sv.Extent.Height - sv.Viewport.Height), sv.Offset.Y + 10));
        }
    }

    private void EndReorder(bool commit)
    {
        var lifted = _lifted!;
        var pointer = _reorderPointer;
        var from = _reorderFrom;
        var to = _reorderTo;
        _lifted = null;
        _reorderPointer = null;
        lifted.ClearValue(RenderTransformProperty);
        lifted.ClearValue(RenderTransformOriginProperty);
        lifted.ClearValue(ZIndexProperty);
        ((IPseudoClasses)lifted.Classes).Set(":reordering", false);
        PseudoClasses.Set(":reordering", false);
        foreach (var container in _shifted.Keys)
        {
            container.ClearValue(RenderTransformProperty);
        }

        _shifted.Clear();
        if (pointer is not null && _reorderClaim is not null)
        {
            DragClaims.Release(pointer, _reorderClaim);
            if (pointer.Captured == this)
            {
                pointer.Capture(null);
            }
        }

        if (!commit || from == to)
        {
            return;
        }

        var args = new ReorderingEventArgs(ReorderingEvent, this, from, to);
        RaiseEvent(args);
        if (args.Cancel || ItemsSource is not IList list || list.IsReadOnly || list.IsFixedSize)
        {
            return;
        }

        var item = list[from];
        list.RemoveAt(from);
        list.Insert(to, item);
    }
}
