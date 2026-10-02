using System;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using AvaEnyo.Primitives;

namespace AvaEnyo.Controls;

/// <summary>
/// The webOS sliding panels (<c>enyo.SlidingPane</c>): <see cref="SlidingView"/>s side by side that slide left over
/// one another. Selecting a view slides it to the left edge in 700 ms with a cubic ease-out, pushing the views on
/// its left along and pulling the ones on its right after it. View 0 never moves.
/// </summary>
/// <remarks>
/// <para>A horizontal drag on a view moves the views; one gesture can push through several. A release selects the
/// view the motion points to. A drag past a view's limit overslides at a quarter strength (full strength when the view
/// is <see cref="SlidingView.Dismissible"/>; past <see cref="DismissDistance"/> it hides).</para>
/// <para>At or below <see cref="MultiViewMinWidth"/> (900 px) of the pane's own width, every view becomes full width
/// (narrow mode), as on a phone. Escape or the system back button select the previous view.</para>
/// </remarks>
[TemplatePart("PART_Panel", typeof(SlidingPanePanel), IsRequired = true)]
[PseudoClasses(":narrow", ":dragging", ":animating")]
public partial class SlidingPane : TemplatedControl
{
    /// <summary>The strength of an overslide drag: 0.25.</summary>
    public static double OverslideStrength = 0.25;

    /// <summary>Defines the <see cref="SelectedView"/> property.</summary>
    public static readonly DirectProperty<SlidingPane, SlidingView?> SelectedViewProperty =
        AvaloniaProperty.RegisterDirect<SlidingPane, SlidingView?>(nameof(SelectedView), o => o.SelectedView, (o, v) => o.SelectView(v));

    /// <summary>Defines the <see cref="SelectedIndex"/> property.</summary>
    public static readonly DirectProperty<SlidingPane, int> SelectedIndexProperty =
        AvaloniaProperty.RegisterDirect<SlidingPane, int>(nameof(SelectedIndex), o => o.SelectedIndex, (o, v) => o.SelectViewByIndex(v), -1);

    /// <summary>Defines the <see cref="MultiView"/> property.</summary>
    public static readonly StyledProperty<bool> MultiViewProperty =
        AvaloniaProperty.Register<SlidingPane, bool>(nameof(MultiView), true);

    /// <summary>Defines the <see cref="MultiViewMinWidth"/> property.</summary>
    public static readonly StyledProperty<double> MultiViewMinWidthProperty =
        AvaloniaProperty.Register<SlidingPane, double>(nameof(MultiViewMinWidth), 900);

    /// <summary>Defines the <see cref="CanAnimate"/> property.</summary>
    public static readonly StyledProperty<bool> CanAnimateProperty =
        AvaloniaProperty.Register<SlidingPane, bool>(nameof(CanAnimate), true);

    /// <summary>Defines the <see cref="FitWhileSliding"/> property.</summary>
    public static readonly StyledProperty<bool> FitWhileSlidingProperty =
        AvaloniaProperty.Register<SlidingPane, bool>(nameof(FitWhileSliding), true);

    /// <summary>Defines the <see cref="DismissDistance"/> property.</summary>
    public static readonly StyledProperty<double> DismissDistanceProperty =
        AvaloniaProperty.Register<SlidingPane, double>(nameof(DismissDistance), 100);

    /// <summary>Defines the <see cref="IsNarrow"/> property.</summary>
    public static readonly DirectProperty<SlidingPane, bool> IsNarrowProperty =
        AvaloniaProperty.RegisterDirect<SlidingPane, bool>(nameof(IsNarrow), o => o.IsNarrow);

    /// <summary>Defines the <see cref="SlideComplete"/> event.</summary>
    public static readonly RoutedEvent<SlideCompleteEventArgs> SlideCompleteEvent =
        RoutedEvent.Register<SlidingPane, SlideCompleteEventArgs>(nameof(SlideComplete), RoutingStrategies.Bubble);

    /// <summary>Defines the <see cref="SelectionChanged"/> event.</summary>
    public static readonly RoutedEvent<PaneSelectionChangedEventArgs> SelectionChangedEvent =
        RoutedEvent.Register<SlidingPane, PaneSelectionChangedEventArgs>(nameof(SelectionChanged), RoutingStrategies.Bubble);

    private SlidingPanePanel? _panel;
    private SlidingView? _selected;
    private bool _isNarrow;

    static SlidingPane()
    {
        EnyoBack.IsBackTargetProperty.OverrideDefaultValue<SlidingPane>(true);
        EnyoBack.BackRequestedEvent.AddClassHandler<SlidingPane>((p, e) => p.OnBackRequested(e));
        MultiViewProperty.Changed.AddClassHandler<SlidingPane>((p, _) => p.UpdateNarrow(p.PaneWidth));
        MultiViewMinWidthProperty.Changed.AddClassHandler<SlidingPane>((p, _) => p.UpdateNarrow(p.PaneWidth));
        FitWhileSlidingProperty.Changed.AddClassHandler<SlidingPane>((p, _) => p.InvalidateViews());
        ClipToBoundsProperty.OverrideDefaultValue<SlidingPane>(true);
    }

    /// <summary>Initializes a new instance of the <see cref="SlidingPane"/> class.</summary>
    public SlidingPane()
    {
        Views.CollectionChanged += OnViewsChanged;
        InitializeDrag();
    }

    /// <summary>The views, left to right.</summary>
    [Content]
    public AvaloniaList<SlidingView> Views { get; } = new();

    /// <summary>The selected view: the one slid to the left edge.</summary>
    public SlidingView? SelectedView
    {
        get => _selected;
        set => SelectView(value);
    }

    /// <summary>The index of the selected view, or -1.</summary>
    public int SelectedIndex
    {
        get => _selected?.Index ?? -1;
        set => SelectViewByIndex(value);
    }

    /// <summary>True (default): views sit side by side when the pane is wider than <see cref="MultiViewMinWidth"/>.</summary>
    public bool MultiView
    {
        get => GetValue(MultiViewProperty);
        set => SetValue(MultiViewProperty, value);
    }

    /// <summary>The pane width at or below which every view is full width. Default 900.</summary>
    public double MultiViewMinWidth
    {
        get => GetValue(MultiViewMinWidthProperty);
        set => SetValue(MultiViewMinWidthProperty, value);
    }

    /// <summary>True (default): selection slides. False: views jump.</summary>
    public bool CanAnimate
    {
        get => GetValue(CanAnimateProperty);
        set => SetValue(CanAnimateProperty, value);
    }

    /// <summary>True (default): views fill the space opened while sliding, then settle back. False: views are fitted only when a slide completes, and <see cref="SlidingView.FixedWidth"/> applies.</summary>
    public bool FitWhileSliding
    {
        get => GetValue(FitWhileSlidingProperty);
        set => SetValue(FitWhileSlidingProperty, value);
    }

    /// <summary>How far right a dismissible view must be dragged to hide it. Default 100.</summary>
    public double DismissDistance
    {
        get => GetValue(DismissDistanceProperty);
        set => SetValue(DismissDistanceProperty, value);
    }

    /// <summary>True in narrow mode: every view is full width.</summary>
    public bool IsNarrow
    {
        get => _isNarrow;
        private set => SetAndRaise(IsNarrowProperty, ref _isNarrow, value);
    }

    /// <summary>Raised when a slide ends.</summary>
    public event EventHandler<SlideCompleteEventArgs>? SlideComplete
    {
        add => AddHandler(SlideCompleteEvent, value);
        remove => RemoveHandler(SlideCompleteEvent, value);
    }

    /// <summary>Raised when the selected view changes.</summary>
    public event EventHandler<PaneSelectionChangedEventArgs>? SelectionChanged
    {
        add => AddHandler(SelectionChangedEvent, value);
        remove => RemoveHandler(SelectionChangedEvent, value);
    }

    internal double PaneWidth { get; private set; }

    internal SlidingView? ViewAt(int index) => index >= 0 && index < Views.Count ? Views[index] : null;

    /// <summary>Selects <paramref name="view"/>; with <paramref name="immediate"/> it jumps.</summary>
    public void SelectView(SlidingView? view, bool immediate = false)
    {
        if (view is null || view.Pane != this)
        {
            return;
        }

        if (immediate)
        {
            var can = _immediate;
            _immediate = true;
            try
            {
                ReallySelectView(view);
            }
            finally
            {
                _immediate = can;
            }
        }
        else
        {
            ReallySelectView(view);
        }
    }

    /// <summary>Selects the view named <paramref name="name"/>. Returns it, or null.</summary>
    public SlidingView? SelectViewByName(string name, bool immediate = false)
    {
        foreach (var v in Views)
        {
            if (v.Name == name)
            {
                SelectView(v, immediate);
                return v;
            }
        }

        return null;
    }

    /// <summary>Selects the view at <paramref name="index"/>. Returns it, or null.</summary>
    public SlidingView? SelectViewByIndex(int index, bool immediate = false)
    {
        var v = ViewAt(index);
        SelectView(v, immediate);
        return v;
    }

    /// <summary>Selects the previous view. Returns false when the first view is selected.</summary>
    public bool Back()
    {
        if (_selected is { Index: > 0 } s && s.Previous is { } p)
        {
            SelectView(p);
            return true;
        }

        return false;
    }

    /// <summary>Selects the next view, wrapping to the first.</summary>
    public void Next()
    {
        if (Views.Count > 0)
        {
            SelectViewByIndex(((_selected?.Index ?? -1) + 1) % Views.Count);
        }
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _panel?.SetViews(Array.Empty<SlidingView>());
        _panel = e.NameScope.Find<SlidingPanePanel>("PART_Panel");
        if (_panel is not null)
        {
            _panel.Pane = this;
            _panel.SetViews(Views);
        }
    }

    private void OnViewsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (SlidingView v in e.OldItems)
            {
                LogicalChildren.Remove(v);
                v.Pane = null;
                if (v == _selected)
                {
                    _selected = null;
                }
            }
        }

        if (e.NewItems is not null)
        {
            foreach (SlidingView v in e.NewItems)
            {
                v.Pane = this;
                LogicalChildren.Add(v);
                ApplyNarrow(v, IsNarrow);
            }
        }

        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            LogicalChildren.Clear();
            foreach (var v in Views)
            {
                v.Pane = this;
                LogicalChildren.Add(v);
            }
        }

        for (var i = 0; i < Views.Count; i++)
        {
            Views[i].Index = i;
        }

        _panel?.SetViews(Views);
        if (_selected is null && Views.Count > 0)
        {
            _selected = Views[0];
            UpdateSelectedPseudoClasses();
        }

        InvalidateViews();
    }

    private void ReallySelectView(SlidingView view)
    {
        if (view == _selected)
        {
            if (!IsDragging)
            {
                AnimateSelected();
            }

            return;
        }

        var last = _selected;
        var oldIndex = last?.Index ?? -1;
        _selected = view;
        UpdateSelectedPseudoClasses();
        if (!IsDragging)
        {
            AnimateSelected();
        }

        RaisePropertyChanged(SelectedViewProperty, last, view);
        RaisePropertyChanged(SelectedIndexProperty, oldIndex, view.Index);
        RaiseEvent(new PaneSelectionChangedEventArgs(SelectionChangedEvent, this, view, last));
    }

    private void UpdateSelectedPseudoClasses()
    {
        foreach (var v in Views)
        {
            v.SetSelected(v == _selected);
        }
    }

    private void OnBackRequested(RoutedEventArgs e)
    {
        if (!e.Handled && IsEffectivelyVisible && _selected is { Index: > 0 })
        {
            e.Handled = Back();
        }
    }
}
