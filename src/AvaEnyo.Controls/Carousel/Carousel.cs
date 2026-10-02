using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Layout;
using AvaEnyo.Animations;
using AvaEnyo.Controls.Primitives;
using AvaEnyo.Primitives;

namespace AvaEnyo.Controls;

/// <summary>
/// An endless carousel that holds only three views: the one shown and its two neighbors (<c>enyo.Carousel</c>). Set
/// the first view with <see cref="SetCenterView"/>; the carousel asks for the neighbors through
/// <see cref="GetLeft"/> and <see cref="GetRight"/>, and again after every snap, so thousands of items cost three views.
/// A null neighbor is an end.
/// </summary>
/// <remarks>
/// A swipe of more than <see cref="DragSnapThreshold"/> (1 %) of the width snaps to the neighbor with the carousel
/// physics (friction 0.75, spring 0.8). After the snap the views rotate between the holders, so the shown view is
/// always in the middle holder. A drag that starts during a snap is swallowed. In XAML use <c>enyo:Carousel</c>;
/// in C# qualify the name, which Avalonia also uses.
/// </remarks>
[TemplatePart("PART_Strip", typeof(CarouselStrip), IsRequired = true)]
[TemplatePart("PART_Left", typeof(ContentControl), IsRequired = true)]
[TemplatePart("PART_Center", typeof(ContentControl), IsRequired = true)]
[TemplatePart("PART_Right", typeof(ContentControl), IsRequired = true)]
[PseudoClasses(":dragging", ":snapping")]
public class Carousel : TemplatedControl
{
    /// <summary>The holder index of the shown view at rest.</summary>
    public const int CenterIndex = 1;

    /// <summary>Defines the <see cref="Orientation"/> property.</summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<Carousel, Orientation>(nameof(Orientation));

    /// <summary>Defines the <see cref="ContentTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        ContentControl.ContentTemplateProperty.AddOwner<Carousel>();

    /// <summary>Defines the <see cref="DragSnapThreshold"/> property.</summary>
    public static readonly StyledProperty<double> DragSnapThresholdProperty =
        AvaloniaProperty.Register<Carousel, double>(nameof(DragSnapThreshold), 0.01);

    /// <summary>Defines the <see cref="GetLeft"/> event.</summary>
    public static readonly RoutedEvent<CarouselViewRequestedEventArgs> GetLeftEvent =
        RoutedEvent.Register<Carousel, CarouselViewRequestedEventArgs>(nameof(GetLeft), RoutingStrategies.Bubble);

    /// <summary>Defines the <see cref="GetRight"/> event.</summary>
    public static readonly RoutedEvent<CarouselViewRequestedEventArgs> GetRightEvent =
        RoutedEvent.Register<Carousel, CarouselViewRequestedEventArgs>(nameof(GetRight), RoutingStrategies.Bubble);

    /// <summary>Defines the <see cref="Snap"/> event.</summary>
    public static readonly RoutedEvent<SnapEventArgs> SnapEvent =
        RoutedEvent.Register<Carousel, SnapEventArgs>(nameof(Snap), RoutingStrategies.Bubble);

    /// <summary>Defines the <see cref="SnapFinished"/> event.</summary>
    public static readonly RoutedEvent<SnapEventArgs> SnapFinishedEvent =
        RoutedEvent.Register<Carousel, SnapEventArgs>(nameof(SnapFinished), RoutingStrategies.Bubble);

    private readonly SnapPhysics _physics = SnapPhysics.ForCarousel();
    private readonly PhysicsLoop _loop;
    private readonly DragTracker _drag;
    private readonly ContentControl?[] _holders = new ContentControl?[3];
    private CarouselStrip? _strip;
    private bool _swallow;
    private double _startPosition;

    static Carousel()
    {
        ContentTemplateProperty.Changed.AddClassHandler<Carousel>((c, _) => c.SyncHolders());
    }

    /// <summary>Initializes a new instance of the <see cref="Carousel"/> class.</summary>
    public Carousel()
    {
        _loop = new PhysicsLoop(this);
        _drag = new DragTracker(this) { CanStart = CanStartDrag };
        _drag.Started += (_, _) => OnDragStarted();
        _drag.Moved += (_, e) => OnDragMoved(e);
        _drag.Completed += (_, _) => OnDragFinished();
        _drag.Canceled += (_, _) => OnDragFinished();
    }

    /// <summary>The axis the views slide on.</summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>The template for items that are not controls.</summary>
    public IDataTemplate? ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    /// <summary>The fraction of the width a swipe must travel to snap to a neighbor. Default 0.01.</summary>
    public double DragSnapThreshold
    {
        get => GetValue(DragSnapThresholdProperty);
        set => SetValue(DragSnapThresholdProperty, value);
    }

    /// <summary>The holder the carousel rests on: 1 normally, 0 or 2 at an end.</summary>
    public int Index { get; private set; } = CenterIndex;

    /// <summary>The view shown now.</summary>
    public object? CenterView => Slots[Index];

    /// <summary>True while a snap glides.</summary>
    public bool IsSnapping => _loop.IsRunning;

    /// <summary>The current scroll position of the strip.</summary>
    public double Position => _physics.Position;

    /// <summary>Raised to get the view left of the shown one.</summary>
    public event EventHandler<CarouselViewRequestedEventArgs>? GetLeft
    {
        add => AddHandler(GetLeftEvent, value);
        remove => RemoveHandler(GetLeftEvent, value);
    }

    /// <summary>Raised to get the view right of the shown one.</summary>
    public event EventHandler<CarouselViewRequestedEventArgs>? GetRight
    {
        add => AddHandler(GetRightEvent, value);
        remove => RemoveHandler(GetRightEvent, value);
    }

    /// <summary>Raised when a swipe chooses a neighbor.</summary>
    public event EventHandler<SnapEventArgs>? Snap
    {
        add => AddHandler(SnapEvent, value);
        remove => RemoveHandler(SnapEvent, value);
    }

    /// <summary>Raised when the snap ends and the views have rotated.</summary>
    public event EventHandler<SnapEventArgs>? SnapFinished
    {
        add => AddHandler(SnapFinishedEvent, value);
        remove => RemoveHandler(SnapFinishedEvent, value);
    }

    /// <summary>The contents of the three holders, left to right.</summary>
    protected object?[] Slots { get; } = new object?[3];

    /// <summary>Whether each holder shows.</summary>
    protected bool[] Shown { get; } = new bool[3];

    /// <summary>Shows <paramref name="view"/> and asks for its neighbors (Enyo <c>setCenterView</c>).</summary>
    public void SetCenterView(object? view)
    {
        StopSnap();
        var left = RequestSide(GetLeftEvent, false);
        var right = RequestSide(GetRightEvent, false);
        SetSlots(left, view, right);
        Index = CenterIndex;
        Rest();
    }

    /// <summary>Glides to the right neighbor, if there is one.</summary>
    public void Next()
    {
        if (!IsSnapping && Index < 2 && Shown[Index + 1])
        {
            SnapToSlot(Index + 1);
        }
    }

    /// <summary>Glides to the left neighbor, if there is one.</summary>
    public void Previous()
    {
        if (!IsSnapping && Index > 0 && Shown[Index - 1])
        {
            SnapToSlot(Index - 1);
        }
    }

    /// <summary>
    /// Updates the holders after a snap from <paramref name="oldIndex"/> to <paramref name="newIndex"/> (Enyo
    /// <c>adjustViews</c>). The base asks for the next view on the side moved toward and rotates the views so the shown
    /// one is in the middle again; at an end it stays.
    /// </summary>
    protected virtual void AdjustViews(int oldIndex, int newIndex)
    {
        var goRight = newIndex > oldIndex;
        var info = RequestSide(goRight ? GetRightEvent : GetLeftEvent, true);
        if (newIndex != CenterIndex)
        {
            if (info is not null)
            {
                if (goRight)
                {
                    SetSlots(Slots[1], Slots[2], info);
                }
                else
                {
                    SetSlots(info, Slots[0], Slots[1]);
                }

                Index = CenterIndex;
            }
        }
        else
        {
            var side = goRight ? 2 : 0;
            var slots = (object?[])Slots.Clone();
            slots[side] = info;
            SetSlots(slots[0], slots[1], slots[2]);
        }
    }

    /// <summary>Fills the holders; a null item hides its holder.</summary>
    protected void SetSlots(object? left, object? center, object? right) =>
        SetSlots(left, center, right, left is not null, center is not null, right is not null);

    /// <summary>Fills the holders with explicit visibility.</summary>
    protected void SetSlots(object? left, object? center, object? right, bool showLeft, bool showCenter, bool showRight)
    {
        Slots[0] = left;
        Slots[1] = center;
        Slots[2] = right;
        Shown[0] = showLeft;
        Shown[1] = showCenter;
        Shown[2] = showRight;
        SyncHolders();
    }

    /// <summary>Sets the resting holder without moving the views.</summary>
    protected void SetIndex(int index) => Index = index;

    /// <summary>Puts the strip at the resting position of <see cref="Index"/>.</summary>
    protected void Rest()
    {
        _physics.SetPosition(PositionOf(Index));
        Apply();
    }

    /// <summary>Stops a running snap.</summary>
    protected void StopSnap()
    {
        _loop.Stop();
        PseudoClasses.Set(":snapping", false);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        foreach (var h in _holders)
        {
            if (h is not null)
            {
                h.Content = null;
            }
        }

        _strip = e.NameScope.Find<CarouselStrip>("PART_Strip");
        _holders[0] = e.NameScope.Find<ContentControl>("PART_Left");
        _holders[1] = e.NameScope.Find<ContentControl>("PART_Center");
        _holders[2] = e.NameScope.Find<ContentControl>("PART_Right");
        SyncHolders();
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        // Bounds still holds the previous size during arrange, so positions use the size being arranged.
        _arrangedSize = finalSize;
        var size = base.ArrangeOverride(finalSize);
        if (!_drag.IsDragging && !_loop.IsRunning)
        {
            var p = PositionOf(Index);
            if (p != _physics.Position)
            {
                _physics.SetPosition(p);
                Apply();
            }
        }

        return size;
    }

    private object? RequestSide(RoutedEvent<CarouselViewRequestedEventArgs> routedEvent, bool isSnap)
    {
        var args = new CarouselViewRequestedEventArgs(routedEvent, this, isSnap);
        RaiseEvent(args);
        return args.View;
    }

    private void SyncHolders()
    {
        // A control can be in one holder at a time, so every holder lets go before any takes a new view.
        foreach (var h in _holders)
        {
            if (h is not null)
            {
                h.Content = null;
            }
        }

        for (var i = 0; i < 3; i++)
        {
            if (_holders[i] is { } h)
            {
                // A view that is a control is shown as is; only data views go through ContentTemplate.
                h.ContentTemplate = Slots[i] is Control ? null : ContentTemplate;
                h.Content = Slots[i];
                h.IsVisible = Shown[i];
            }
        }

        InvalidateArrange();
        _strip?.InvalidateArrange();
    }

    private Size _arrangedSize;

    private double ViewSize
    {
        get
        {
            var size = _arrangedSize.Width > 0 || _arrangedSize.Height > 0 ? _arrangedSize : Bounds.Size;
            return Orientation == Orientation.Horizontal ? size.Width : size.Height;
        }
    }

    private double PositionOf(int slot)
    {
        var k = 0;
        for (var i = 0; i < slot; i++)
        {
            if (Shown[i])
            {
                k++;
            }
        }

        return k * ViewSize;
    }

    private void UpdateBounds()
    {
        var first = Array.IndexOf(Shown, true);
        var last = Array.LastIndexOf(Shown, true);
        _physics.Min = first < 0 ? 0 : PositionOf(first);
        _physics.Max = last < 0 ? 0 : PositionOf(last);
    }

    private bool CanStartDrag(DragStartInfo info) =>
        Orientation == Orientation.Horizontal ? info.IsHorizontal : info.IsVertical;

    private void OnDragStarted()
    {
        _swallow = IsSnapping;
        if (_swallow)
        {
            return;
        }

        PseudoClasses.Set(":dragging", true);
        UpdateBounds();
        _startPosition = _physics.Position;
        _physics.BeginDrag();
    }

    private void OnDragMoved(DragTrackerEventArgs e)
    {
        if (_swallow)
        {
            return;
        }

        _physics.Drag(-(Orientation == Orientation.Horizontal ? e.Delta.X : e.Delta.Y));
        Apply();
    }

    private void OnDragFinished()
    {
        if (_swallow)
        {
            _swallow = false;
            return;
        }

        PseudoClasses.Set(":dragging", false);
        _physics.EndDrag(0);
        var travel = _physics.Position - _startPosition;
        var target = Index;
        if (Math.Abs(travel) > DragSnapThreshold * ViewSize)
        {
            var next = Index + Math.Sign(travel);
            if (next >= 0 && next <= 2 && Shown[next])
            {
                target = next;
            }
        }

        SnapToSlot(target);
    }

    private void SnapToSlot(int slot)
    {
        var old = Index;
        Index = slot;
        if (slot != old)
        {
            RaiseEvent(new SnapEventArgs(SnapEvent, this, slot));
        }

        UpdateBounds();
        var target = PositionOf(slot);
        _physics.SnapTo(target);
        PseudoClasses.Set(":snapping", true);
        _loop.Start(
            elapsed =>
            {
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
                if (slot != old)
                {
                    AdjustViews(old, slot);
                }

                Rest();
                RaiseEvent(new SnapEventArgs(SnapFinishedEvent, this, Index));
            });
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
