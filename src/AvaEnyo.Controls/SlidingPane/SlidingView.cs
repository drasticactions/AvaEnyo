using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AvaEnyo.Controls;

/// <summary>
/// A view of a <see cref="SlidingPane"/> (<c>enyo.SlidingView</c>). Views sit side by side and slide left over
/// one another; the selected view slides to the left edge (plus <see cref="PeekWidth"/>). A 20 px shadow falls on the
/// view to the left. The last showing view is fitted to the remaining width.
/// </summary>
/// <remarks>
/// Give a view a <c>Width</c>, or a share of the free space with <c>enyo:Flex.Value</c>. A drag on the view moves it
/// when <see cref="DragAnywhere"/> is on, or from any control marked <c>enyo:Sliding.IsHandle="True"</c>; tapping a
/// handle toggles the view. Set <see cref="IsShowing"/> to false to slide the view off the right edge.
/// </remarks>
[TemplatePart("PART_Shadow", typeof(Control))]
[TemplatePart("PART_Content", typeof(Control), IsRequired = true)]
[TemplatePart("PART_EdgeDragger", typeof(Control))]
[PseudoClasses(":selected", ":dragging", ":first-view")]
public class SlidingView : ContentControl
{
    /// <summary>Defines the <see cref="DragAnywhere"/> property.</summary>
    public static readonly StyledProperty<bool> DragAnywhereProperty =
        AvaloniaProperty.Register<SlidingView, bool>(nameof(DragAnywhere), true);

    /// <summary>Defines the <see cref="EdgeDragging"/> property.</summary>
    public static readonly StyledProperty<bool> EdgeDraggingProperty =
        AvaloniaProperty.Register<SlidingView, bool>(nameof(EdgeDragging));

    /// <summary>Defines the <see cref="FixedWidth"/> property.</summary>
    public static readonly StyledProperty<bool> FixedWidthProperty =
        AvaloniaProperty.Register<SlidingView, bool>(nameof(FixedWidth));

    /// <summary>Defines the <see cref="PeekWidth"/> property.</summary>
    public static readonly StyledProperty<double> PeekWidthProperty =
        AvaloniaProperty.Register<SlidingView, double>(nameof(PeekWidth));

    /// <summary>Defines the <see cref="Dismissible"/> property.</summary>
    public static readonly StyledProperty<bool> DismissibleProperty =
        AvaloniaProperty.Register<SlidingView, bool>(nameof(Dismissible));

    /// <summary>Defines the <see cref="IsShowing"/> property.</summary>
    public static readonly StyledProperty<bool> IsShowingProperty =
        AvaloniaProperty.Register<SlidingView, bool>(nameof(IsShowing), true);

    /// <summary>Defines the <see cref="SlidePosition"/> property.</summary>
    public static readonly DirectProperty<SlidingView, double> SlidePositionProperty =
        AvaloniaProperty.RegisterDirect<SlidingView, double>(nameof(SlidePosition), o => o.SlidePosition);

    /// <summary>Defines the <see cref="Resized"/> event.</summary>
    public static readonly RoutedEvent<SlidingViewResizedEventArgs> ResizedEvent =
        RoutedEvent.Register<SlidingView, SlidingViewResizedEventArgs>(nameof(Resized), RoutingStrategies.Bubble);

    private double _slidePosition;
    private int _index = -1;
    private readonly TranslateTransform _translate = new();

    static SlidingView()
    {
        IsShowingProperty.Changed.AddClassHandler<SlidingView>((v, e) => v.Pane?.OnViewShowingChanged(v, e.GetNewValue<bool>()));
        FixedWidthProperty.Changed.AddClassHandler<SlidingView>((v, _) => v.Pane?.InvalidateViews());
        PeekWidthProperty.Changed.AddClassHandler<SlidingView>((v, _) => v.Pane?.InvalidateViews());
        Button.ClickEvent.AddClassHandler<SlidingView>((v, e) => v.OnHandleClick(e));
        InputElement.TappedEvent.AddClassHandler<SlidingView>((v, e) => v.OnHandleTapped(e));
    }

    /// <summary>Initializes a new instance of the <see cref="SlidingView"/> class.</summary>
    public SlidingView()
    {
        RenderTransform = _translate;
    }

    /// <summary>True (default): the view can be dragged from anywhere a scroller does not take the drag.</summary>
    public bool DragAnywhere
    {
        get => GetValue(DragAnywhereProperty);
        set => SetValue(DragAnywhereProperty, value);
    }

    /// <summary>True: an invisible 40 px handle on the left edge drags and toggles the view.</summary>
    public bool EdgeDragging
    {
        get => GetValue(EdgeDraggingProperty);
        set => SetValue(EdgeDraggingProperty, value);
    }

    /// <summary>True: the view keeps its own width as the last showing view. Applies only when the pane's <see cref="SlidingPane.FitWhileSliding"/> is false.</summary>
    public bool FixedWidth
    {
        get => GetValue(FixedWidthProperty);
        set => SetValue(FixedWidthProperty, value);
    }

    /// <summary>How far from the left edge the view stops when selected, to reveal the view beneath.</summary>
    public double PeekWidth
    {
        get => GetValue(PeekWidthProperty);
        set => SetValue(PeekWidthProperty, value);
    }

    /// <summary>True: dragging the view right past the pane's dismiss distance hides it, and the drag is not damped.</summary>
    public bool Dismissible
    {
        get => GetValue(DismissibleProperty);
        set => SetValue(DismissibleProperty, value);
    }

    /// <summary>
    /// Enyo's <c>showing</c>: false slides the view (and the views after it) off the right edge, true slides it back.
    /// </summary>
    public bool IsShowing
    {
        get => GetValue(IsShowingProperty);
        set => SetValue(IsShowingProperty, value);
    }

    /// <summary>The horizontal offset from the view's natural place. Negative slides it left.</summary>
    public double SlidePosition
    {
        get => _slidePosition;
        private set => SetAndRaise(SlidePositionProperty, ref _slidePosition, value);
    }

    /// <summary>The position in the pane's views.</summary>
    public int Index
    {
        get => _index;
        internal set
        {
            _index = value;
            PseudoClasses.Set(":first-view", value == 0);
        }
    }

    /// <summary>The left edge of the view before sliding, measured by the pane's last layout.</summary>
    public double NaturalLeft { get; internal set; }

    /// <summary>The width of the view before fitting.</summary>
    public double NaturalWidth { get; internal set; }

    /// <summary>The width the pane fitted the view to, or null.</summary>
    public double? FitWidth
    {
        get => _fitWidth;
        internal set
        {
            if (_fitWidth == value)
            {
                return;
            }

            _fitWidth = value;
            if (value is not null && FitCachedWidth is null && !double.IsNaN(Width))
            {
                FitCachedWidth = Width;
                SetCurrentValue(WidthProperty, double.NaN);
            }
            else if (value is null && FitCachedWidth is { } w)
            {
                FitCachedWidth = null;
                SetCurrentValue(WidthProperty, w);
            }
        }
    }

    private double? _fitWidth;

    /// <summary>The view's own width while a fitted width overrides it.</summary>
    internal double? FitCachedWidth { get; private set; }

    /// <summary>The view's own width, ignoring any fitted width.</summary>
    internal double DeclaredWidth => FitCachedWidth ?? Width;

    /// <summary>Raised when the pane fits the view to a new width.</summary>
    public event EventHandler<SlidingViewResizedEventArgs>? Resized
    {
        add => AddHandler(ResizedEvent, value);
        remove => RemoveHandler(ResizedEvent, value);
    }

    /// <summary>Selects this view.</summary>
    public void Select() => Pane?.SelectView(this);

    /// <summary>Selects the previous view.</summary>
    public void SelectPrevious()
    {
        if (Previous is { } p)
        {
            Pane?.SelectView(p);
        }
    }

    /// <summary>Selects the next view.</summary>
    public void SelectNext()
    {
        if (Next is { } n)
        {
            Pane?.SelectView(n);
        }
    }

    /// <summary>Selects the previous view when this one is selected, otherwise selects this one.</summary>
    public void ToggleSelected()
    {
        if (Pane?.SelectedView == this)
        {
            SelectPrevious();
        }
        else
        {
            Select();
        }
    }

    internal SlidingPane? Pane { get; set; }

    /// <summary>The natural width the view was last laid out for.</summary>
    internal double MeasuredWidth { get; set; } = double.NaN;

    /// <summary>The view's own width while narrow mode overrides it.</summary>
    internal double? CachedWidth { get; set; }

    internal double LastSlidePosition { get; set; }

    internal bool OverSliding { get; set; }

    internal double DragMin { get; private set; }

    internal double DragMax { get; private set; }

    internal double DragStart { get; set; }

    internal double? LastDragDx { get; set; }

    internal bool ShouldDragSelect { get; set; }

    internal SlidingView? Previous => Pane is { } p && Index > 0 ? p.ViewAt(Index - 1) : null;

    internal SlidingView? Next => Pane is { } p ? p.ViewAt(Index + 1) : null;

    internal SlidingView? First => Pane?.ViewAt(0);

    /// <summary>The last view before the first one that is not showing.</summary>
    internal SlidingView? LastShowingSibling
    {
        get
        {
            if (Pane is not { } p)
            {
                return null;
            }

            var count = p.Views.Count;
            for (var i = 0; i < count; i++)
            {
                if (!p.ViewAt(i)!.IsShowing)
                {
                    return p.ViewAt(Math.Max(0, i - 1));
                }
            }

            return count > 0 ? p.ViewAt(count - 1) : null;
        }
    }

    // ---- Enyo's position model (SlidingView.js) ----

    internal double CalcSlide()
    {
        var p = Pane!;
        var selected = p.SelectedView;
        if (ShouldSlideHidden() || selected is null)
        {
            return selected is null ? 0 : CalcSlideHidden();
        }

        var si = selected.Index;
        return Index == si ? CalcSlideSelected() : Index < si ? CalcSlideBefore() : CalcSlideAfter();
    }

    internal double CalcSlideMin() => PeekWidthEffective - NaturalLeft;

    internal double CalcSlideMax() => Previous?.SlidePosition ?? 0;

    internal double CalcSlideBefore()
    {
        var m = CalcSlideMin();
        if (Pane!.IsDragging && Next is { } dragged)
        {
            return Math.Max(m, dragged.SlidePosition);
        }

        if (Pane.IsSlideAnimating && Next is { } next)
        {
            // Follows the moving view only on the way to its own target.
            var target = CalcSlideTarget();
            var follow = Math.Max(m, next.SlidePosition);
            return Math.Clamp(follow, Math.Min(SlidePosition, target), Math.Max(SlidePosition, target));
        }

        return m;
    }

    /// <summary>Where the view rests for the current selection.</summary>
    internal double CalcSlideTarget()
    {
        var selected = Pane!.SelectedView;
        if (ShouldSlideHidden() || selected is null)
        {
            return selected is null ? 0 : CalcSlideHidden();
        }

        return Index <= selected.Index ? CalcSlideMin() : selected.CalcSlideMin();
    }

    internal double CalcSlideSelected() => CalcSlideMin();

    internal double CalcSlideAfter()
    {
        if (Pane!.IsSlideAnimating || Pane.IsDragging)
        {
            return CalcSlideMax();
        }

        return Pane.SelectedView?.CalcSlideMin() ?? 0;
    }

    internal double CalcSlideHidden() => (Pane?.PaneWidth ?? 0) - NaturalLeft;

    internal bool ShouldSlideHidden()
    {
        for (var v = this; v is not null; v = v.Previous)
        {
            if (!v.IsShowing)
            {
                return true;
            }
        }

        return false;
    }

    internal void Move(double slide)
    {
        ApplySlide(slide);
        Next?.ValidateSlide();
    }

    internal void ApplySlide(double slide)
    {
        // View 0 never moves.
        if (slide != SlidePosition && Index != 0)
        {
            LastSlidePosition = SlidePosition;
            SlidePosition = slide;
            SetTranslate(slide);
        }
    }

    internal void ValidateSlide() => Move(CalcSlide());

    internal void ValidateSlideBefore()
    {
        for (var s = First; s is not null && s.Index != Index; s = s.Next)
        {
            s.ApplySlide(s.CalcSlide());
        }
    }

    internal bool CanAnimate() => Index != 0 && SlidePosition != CalcSlide();

    internal void AnimateMove(double slide, bool overSliding)
    {
        Move(slide);
        if (!overSliding)
        {
            for (var p = Previous; p is not null; p = p.Previous)
            {
                p.ApplySlide(p.CalcSlideBefore());
            }
        }
    }

    internal bool CanDrag(double delta)
    {
        DragMin = CalcSlideMin();
        DragMax = CalcSlideMax();
        var si = Pane!.SelectedView?.Index ?? 0;
        if (Index > 0 && IsShowing && Index >= si)
        {
            var x = SlidePosition + delta;
            return DragMax != DragMin && x >= DragMin && x <= DragMax;
        }

        return false;
    }

    internal void BeginDrag(double dx, double startDx)
    {
        ValidateSlideBefore();
        LastDragDx = dx;
        DragStart = SlidePosition - startDx;
    }

    internal bool IsMovingToSelect => SlidePosition < LastSlidePosition;

    /// <summary>Moves the view for a drag. Returns the view to select when the drag reached a boundary.</summary>
    internal (bool Boundary, SlidingView? Select) Drag(double dx)
    {
        var x0 = dx + DragStart;
        if (x0 == SlidePosition)
        {
            return (false, null);
        }

        ShouldDragSelect = x0 < SlidePosition;
        if (x0 < DragMin || (x0 > DragMax && !OverSliding) || (x0 < DragMax && OverSliding))
        {
            return (true, GetDragSelect());
        }

        var ldx = LastDragDx ?? 0;
        x0 = OverSliding && !Dismissible
            ? ((dx - ldx) * SlidingPane.OverslideStrength) + SlidePosition
            : (dx - ldx) + SlidePosition;
        LastDragDx = dx;
        var x = Math.Max(DragMin, Math.Min(x0, OverSliding ? 1e9 : DragMax));
        Move(x);
        return (false, null);
    }

    internal SlidingView? GetDragSelect()
    {
        if (ShouldDragSelect && !OverSliding)
        {
            return this;
        }

        var p = Previous;
        return p is not null && (p.SlidePosition < p.CalcSlideMax() || p.Index == 0) ? p : null;
    }

    internal double CalcFitWidth()
    {
        var pw = Pane?.PaneWidth ?? 0;
        var s = Math.Min(SlidePosition, 0);
        return Math.Max(0, Math.Min(pw, pw - NaturalLeft - s));
    }

    /// <summary>The view's current left edge, after sliding.</summary>
    internal double CurrentLeft => NaturalLeft + (Index == 0 ? 0 : SlidePosition);

    /// <summary>Applies or clears the fitted width (Enyo <c>applySize</c>). With <paramref name="edge"/>, the view reaches only that far.</summary>
    internal void ApplySize(bool suggestFit, double? edge = null)
    {
        double? width;
        if ((Pane?.PaneWidth ?? 0) <= 0)
        {
            // No layout yet: fitting to a zero-width pane would squeeze the view until the next fit.
            return;
        }

        if (suggestFit && !FixedWidthEffective)
        {
            width = CalcFitWidth();
            if (edge is { } e)
            {
                width = Math.Min(width.Value, Math.Max(NaturalWidth, e - CurrentLeft));
            }
        }
        else if (FitWidth is not null)
        {
            width = null;
        }
        else
        {
            return;
        }

        if (width == FitWidth)
        {
            return;
        }

        FitWidth = width;
        InvalidateMeasure();
        (this.GetVisualParent() as Layoutable)?.InvalidateMeasure();
        RaiseEvent(new SlidingViewResizedEventArgs(ResizedEvent, this, width));
    }

    internal void SetSlideImmediate(double slide)
    {
        LastSlidePosition = SlidePosition;
        SlidePosition = slide;
        SetTranslate(Index == 0 ? 0 : slide);
    }

    /// <summary>Sets the view's translation and re-arranges it.</summary>
    private void SetTranslate(double x)
    {
        if (_translate.X != x)
        {
            _translate.X = x;
            InvalidateArrange();
        }
    }

    internal double PeekWidthEffective => Pane?.IsNarrow == true ? 0 : PeekWidth;

    internal bool FixedWidthEffective => Pane?.IsNarrow == true || (FixedWidth && Pane?.FitWhileSliding == false);

    /// <summary>The handle under <paramref name="source"/>, or null (Enyo <c>findSlidingHandler</c>).</summary>
    internal Control? FindHandle(object? source)
    {
        for (var c = source as Visual; c is not null && c != this; c = c.GetVisualParent())
        {
            if (c is Control control && Sliding.GetIsHandle(control) is { } isHandle)
            {
                return isHandle ? control : null;
            }
        }

        return null;
    }

    /// <summary>True when a drag that started on <paramref name="source"/> may move views (Enyo <c>isDraggableEvent</c>).</summary>
    internal bool IsDraggableFrom(object? source)
    {
        for (var c = source as Visual; c is not null && c != this; c = c.GetVisualParent())
        {
            if (c is Control control && Sliding.GetIsHandle(control) is { } isHandle)
            {
                return isHandle;
            }
        }

        return DragAnywhere;
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        if (FitWidth is { } w)
        {
            availableSize = availableSize.WithWidth(w);
        }

        return base.MeasureOverride(availableSize);
    }

    private void OnHandleClick(RoutedEventArgs e)
    {
        if (!e.Handled && FindHandle(e.Source) is Button)
        {
            ToggleSelected();
            e.Handled = true;
        }
    }

    private void OnHandleTapped(TappedEventArgs e)
    {
        if (!e.Handled && FindHandle(e.Source) is { } handle && handle is not Button)
        {
            ToggleSelected();
            e.Handled = true;
        }
    }

    internal void SetSelected(bool selected) => PseudoClasses.Set(":selected", selected);

    internal void SetDragging(bool dragging) => PseudoClasses.Set(":dragging", dragging);
}
