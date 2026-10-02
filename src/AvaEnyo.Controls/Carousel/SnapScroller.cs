using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Layout;
using AvaEnyo.Animations;
using AvaEnyo.Controls.Primitives;
using AvaEnyo.Primitives;

namespace AvaEnyo.Controls;

/// <summary>
/// A strip of items that snaps to one item at a time (<c>enyo.SnapScroller</c>). Dragging moves the strip 1:1; on
/// release, a travel above <see cref="DragSnapThreshold"/> of the item's size moves to the next item in the swipe
/// direction, a shorter one returns. The snap glides with friction 0.85 per 10 ms.
/// </summary>
/// <remarks>
/// <see cref="RevealAmount"/> leaves that much of the neighbors in view. Setting <see cref="SelectedIndex"/> glides to
/// that item. <see cref="Snap"/> fires when a new item is chosen, <see cref="SnapFinished"/> when the glide ends.
/// </remarks>
[TemplatePart("PART_Strip", typeof(SnapStrip), IsRequired = true)]
[TemplatePart("PART_ItemsPresenter", typeof(ItemsPresenter), IsRequired = true)]
[PseudoClasses(":dragging", ":snapping")]
public class SnapScroller : ItemsControl
{
    /// <summary>Defines the <see cref="Orientation"/> property.</summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<SnapScroller, Orientation>(nameof(Orientation));

    /// <summary>Defines the <see cref="SelectedIndex"/> property.</summary>
    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<SnapScroller, int>(nameof(SelectedIndex), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Defines the <see cref="DragSnapThreshold"/> property.</summary>
    public static readonly StyledProperty<double> DragSnapThresholdProperty =
        AvaloniaProperty.Register<SnapScroller, double>(nameof(DragSnapThreshold), 0.1);

    /// <summary>Defines the <see cref="RevealAmount"/> property.</summary>
    public static readonly StyledProperty<double> RevealAmountProperty =
        AvaloniaProperty.Register<SnapScroller, double>(nameof(RevealAmount));

    /// <summary>Defines the <see cref="Snap"/> event.</summary>
    public static readonly RoutedEvent<SnapEventArgs> SnapEvent =
        RoutedEvent.Register<SnapScroller, SnapEventArgs>(nameof(Snap), RoutingStrategies.Bubble);

    /// <summary>Defines the <see cref="SnapFinished"/> event.</summary>
    public static readonly RoutedEvent<SnapEventArgs> SnapFinishedEvent =
        RoutedEvent.Register<SnapScroller, SnapEventArgs>(nameof(SnapFinished), RoutingStrategies.Bubble);

    private readonly SnapPhysics _physics = new();
    private readonly PhysicsLoop _loop;
    private readonly DragTracker _drag;
    private SnapStrip? _strip;
    private ItemsPresenter? _presenter;
    private bool _settingIndex;
    private double _startPosition;

    static SnapScroller()
    {
        SelectedIndexProperty.Changed.AddClassHandler<SnapScroller>((s, e) => s.OnSelectedIndexChanged(e.GetNewValue<int>()));
        ItemsPanelProperty.OverrideDefaultValue<SnapScroller>(new FuncTemplate<Panel?>(() => new StackPanel { Orientation = Orientation.Horizontal }));
        OrientationProperty.Changed.AddClassHandler<SnapScroller>((s, _) => s.InvalidateMeasure());
    }

    /// <summary>Initializes a new instance of the <see cref="SnapScroller"/> class.</summary>
    public SnapScroller()
    {
        _loop = new PhysicsLoop(this);
        _drag = new DragTracker(this) { CanStart = CanStartDrag };
        _drag.Started += (_, _) => OnDragStarted();
        _drag.Moved += (_, e) => OnDragMoved(e);
        _drag.Completed += (_, e) => OnDragFinished();
        _drag.Canceled += (_, e) => OnDragFinished();
    }

    /// <summary>The axis the strip scrolls on. A vertical scroller should use a vertical items panel.</summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>The item snapped to. Setting it glides there.</summary>
    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    /// <summary>The fraction of an item's size a drag must travel to move to the next item. Default 0.1.</summary>
    public double DragSnapThreshold
    {
        get => GetValue(DragSnapThresholdProperty);
        set => SetValue(DragSnapThresholdProperty, value);
    }

    /// <summary>How much of the neighboring items stays in view at the start edge.</summary>
    public double RevealAmount
    {
        get => GetValue(RevealAmountProperty);
        set => SetValue(RevealAmountProperty, value);
    }

    /// <summary>The current scroll position of the strip.</summary>
    public double Position => _physics.Position;

    /// <summary>True while a drag moves the strip.</summary>
    public bool IsDragging => _drag.IsDragging;

    /// <summary>True while a snap glides.</summary>
    public bool IsSnapping => _loop.IsRunning;

    /// <summary>Raised when a new item is chosen.</summary>
    public event EventHandler<SnapEventArgs>? Snap
    {
        add => AddHandler(SnapEvent, value);
        remove => RemoveHandler(SnapEvent, value);
    }

    /// <summary>Raised when the snap glide ends.</summary>
    public event EventHandler<SnapEventArgs>? SnapFinished
    {
        add => AddHandler(SnapFinishedEvent, value);
        remove => RemoveHandler(SnapFinishedEvent, value);
    }

    /// <summary>Glides to the previous item.</summary>
    public void Previous() => SnapTo(SelectedIndex - 1);

    /// <summary>Glides to the next item.</summary>
    public void Next() => SnapTo(SelectedIndex + 1);

    /// <summary>Glides to the item at <paramref name="index"/>, clamped to the items.</summary>
    public void SnapTo(int index)
    {
        if (ItemCount == 0)
        {
            return;
        }

        index = Math.Clamp(index, 0, ItemCount - 1);
        var old = SelectedIndex;
        SetIndexSilently(index);
        if (index != old)
        {
            RaiseEvent(new SnapEventArgs(SnapEvent, this, index));
        }

        UpdateBounds();
        var target = PositionOf(index);
        _physics.SnapTo(target);
        PseudoClasses.Set(":snapping", true);
        _loop.Start(
            elapsed =>
            {
                UpdateBounds();
                var moving = _physics.Step(elapsed);
                Apply();
                return moving;
            },
            () =>
            {
                _physics.SetPosition(target);
                Apply();
            },
            () =>
            {
                PseudoClasses.Set(":snapping", false);
                RaiseEvent(new SnapEventArgs(SnapFinishedEvent, this, SelectedIndex));
            });
    }

    /// <summary>The scroll position at which item <paramref name="index"/> is snapped.</summary>
    public double PositionOf(int index)
    {
        if (_presenter is null || ContainerFromIndex(index) is not { } c || c.TranslatePoint(default, _presenter) is not { } p)
        {
            return 0;
        }

        return (Orientation == Orientation.Horizontal ? p.X : p.Y) - RevealAmount;
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _strip = e.NameScope.Find<SnapStrip>("PART_Strip");
        _presenter = e.NameScope.Find<ItemsPresenter>("PART_ItemsPresenter");
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = base.ArrangeOverride(finalSize);
        if (!_drag.IsDragging && !_loop.IsRunning && ItemCount > 0)
        {
            var p = PositionOf(Math.Clamp(SelectedIndex, 0, ItemCount - 1));
            if (p != _physics.Position)
            {
                _physics.SetPosition(p);
                Apply();
            }
        }

        return size;
    }

    private void OnSelectedIndexChanged(int index)
    {
        if (_settingIndex)
        {
            return;
        }

        if (_strip is null)
        {
            return;
        }

        SnapTo(index);
    }

    private void SetIndexSilently(int index)
    {
        _settingIndex = true;
        try
        {
            SetCurrentValue(SelectedIndexProperty, index);
        }
        finally
        {
            _settingIndex = false;
        }
    }

    private bool CanStartDrag(DragStartInfo info) =>
        ItemCount > 0 && (Orientation == Orientation.Horizontal ? info.IsHorizontal : info.IsVertical);

    private void OnDragStarted()
    {
        _loop.Stop();
        PseudoClasses.Set(":snapping", false);
        PseudoClasses.Set(":dragging", true);
        UpdateBounds();
        _startPosition = _physics.Position;
        _physics.BeginDrag();
    }

    private void OnDragMoved(DragTrackerEventArgs e)
    {
        _physics.Drag(-(Orientation == Orientation.Horizontal ? e.Delta.X : e.Delta.Y));
        Apply();
    }

    private void OnDragFinished()
    {
        PseudoClasses.Set(":dragging", false);
        _physics.EndDrag(0);
        var travel = _physics.Position - _startPosition;
        var index = Math.Clamp(SelectedIndex, 0, Math.Max(0, ItemCount - 1));
        var size = ItemSize(index);
        if (Math.Abs(travel) > DragSnapThreshold * size && Math.Abs(travel) > 0)
        {
            index += Math.Sign(travel);
        }

        SnapTo(index);
    }

    private double ItemSize(int index)
    {
        if (ContainerFromIndex(index) is { } c)
        {
            return Orientation == Orientation.Horizontal ? c.Bounds.Width : c.Bounds.Height;
        }

        return Orientation == Orientation.Horizontal ? Bounds.Width : Bounds.Height;
    }

    private void UpdateBounds()
    {
        _physics.Min = ItemCount > 0 ? PositionOf(0) : 0;
        _physics.Max = ItemCount > 0 ? Math.Max(_physics.Min, PositionOf(ItemCount - 1)) : 0;
    }

    private void Apply()
    {
        if (_strip is not null)
        {
            _strip.Orientation = Orientation;
            _strip.Offset = _physics.Position;
        }
    }
}
