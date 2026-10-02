using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Metadata;
using AvaEnyo.Animations;

namespace AvaEnyo.Controls;

/// <summary>
/// A stack of views that shows one at a time (<c>enyo.Pane</c>). Selecting a view plays the
/// <see cref="Transition"/> (a 300 ms cubic ease-out fade by default) and records the previous view in a history of
/// up to <see cref="MaxHistory"/> entries, so <see cref="Back"/> returns to it. Requests made while a transition runs
/// are queued, and the pane ignores input until the transition ends.
/// </summary>
[TemplatePart("PART_Host", typeof(PaneHost), IsRequired = true)]
[TemplatePart("PART_InputBlocker", typeof(Control))]
[PseudoClasses(":transitioning")]
public class Pane : TemplatedControl
{
    /// <summary>Defines the <see cref="SelectedView"/> property.</summary>
    public static readonly DirectProperty<Pane, Control?> SelectedViewProperty =
        AvaloniaProperty.RegisterDirect<Pane, Control?>(nameof(SelectedView), o => o.SelectedView, (o, v) => o.SelectView(v));

    /// <summary>Defines the <see cref="SelectedIndex"/> property.</summary>
    public static readonly DirectProperty<Pane, int> SelectedIndexProperty =
        AvaloniaProperty.RegisterDirect<Pane, int>(nameof(SelectedIndex), o => o.SelectedIndex, (o, v) => o.SelectViewByIndex(v), -1);

    /// <summary>Defines the <see cref="CanGoBack"/> property.</summary>
    public static readonly DirectProperty<Pane, bool> CanGoBackProperty =
        AvaloniaProperty.RegisterDirect<Pane, bool>(nameof(CanGoBack), o => o.CanGoBack);

    /// <summary>Defines the <see cref="Transition"/> property.</summary>
    public static readonly StyledProperty<IPageTransition?> TransitionProperty =
        AvaloniaProperty.Register<Pane, IPageTransition?>(nameof(Transition), EnyoPaneTransitions.Fade);

    /// <summary>Defines the <see cref="MaxHistory"/> property.</summary>
    public static readonly StyledProperty<int> MaxHistoryProperty =
        AvaloniaProperty.Register<Pane, int>(nameof(MaxHistory), 40);

    /// <summary>Defines the <see cref="SelectionChanged"/> event.</summary>
    public static readonly RoutedEvent<PaneSelectionChangedEventArgs> SelectionChangedEvent =
        RoutedEvent.Register<Pane, PaneSelectionChangedEventArgs>(nameof(SelectionChanged), RoutingStrategies.Bubble);

    /// <summary>Defines the <see cref="ViewShown"/> event.</summary>
    public static readonly RoutedEvent<PaneViewEventArgs> ViewShownEvent =
        RoutedEvent.Register<Pane, PaneViewEventArgs>(nameof(ViewShown), RoutingStrategies.Bubble);

    /// <summary>Defines the <see cref="ViewHidden"/> event.</summary>
    public static readonly RoutedEvent<PaneViewEventArgs> ViewHiddenEvent =
        RoutedEvent.Register<Pane, PaneViewEventArgs>(nameof(ViewHidden), RoutingStrategies.Bubble);

    /// <summary>Defines the <see cref="CreateView"/> event.</summary>
    public static readonly RoutedEvent<CreateViewEventArgs> CreateViewEvent =
        RoutedEvent.Register<Pane, CreateViewEventArgs>(nameof(CreateView), RoutingStrategies.Bubble);

    private readonly List<Control> _history = new();
    private readonly Queue<(Control To, bool Immediate, bool IsBack)> _queue = new();
    private PaneHost? _host;
    private Control? _blocker;
    private Control? _selected;
    private bool _transitioning;
    private bool _canGoBack;
    private int _generation;
    private CancellationTokenSource? _transitionCts;

    static Pane()
    {
        EnyoBack.IsBackTargetProperty.OverrideDefaultValue<Pane>(true);
        EnyoBack.BackRequestedEvent.AddClassHandler<Pane>((p, e) => p.OnBackRequested(e));
    }

    /// <summary>Initializes a new instance of the <see cref="Pane"/> class.</summary>
    public Pane()
    {
        Views.CollectionChanged += OnViewsChanged;
    }

    /// <summary>The views. The first one is selected when nothing else is.</summary>
    [Content]
    public AvaloniaList<Control> Views { get; } = new();

    /// <summary>The selected view.</summary>
    public Control? SelectedView
    {
        get => _selected;
        set => SelectView(value);
    }

    /// <summary>The index of the selected view, or -1.</summary>
    public int SelectedIndex
    {
        get => _selected is null ? -1 : Views.IndexOf(_selected);
        set => SelectViewByIndex(value);
    }

    /// <summary>True when the history has a view to go back to.</summary>
    public bool CanGoBack
    {
        get => _canGoBack;
        private set => SetAndRaise(CanGoBackProperty, ref _canGoBack, value);
    }

    /// <summary>
    /// The view transition: <see cref="EnyoPaneTransitions.Fade"/> (default), <see cref="EnyoPaneTransitions.FlyIn"/>,
    /// <see cref="EnyoPaneTransitions.None"/>, or any <see cref="IPageTransition"/>.
    /// </summary>
    public IPageTransition? Transition
    {
        get => GetValue(TransitionProperty);
        set => SetValue(TransitionProperty, value);
    }

    /// <summary>The number of history entries kept. Default 40.</summary>
    public int MaxHistory
    {
        get => GetValue(MaxHistoryProperty);
        set => SetValue(MaxHistoryProperty, value);
    }

    /// <summary>True while a transition plays.</summary>
    public bool IsTransitioning => _transitioning;

    /// <summary>Raised when the selected view changes.</summary>
    public event EventHandler<PaneSelectionChangedEventArgs>? SelectionChanged
    {
        add => AddHandler(SelectionChangedEvent, value);
        remove => RemoveHandler(SelectionChangedEvent, value);
    }

    /// <summary>Raised when a view becomes visible at the end of a transition.</summary>
    public event EventHandler<PaneViewEventArgs>? ViewShown
    {
        add => AddHandler(ViewShownEvent, value);
        remove => RemoveHandler(ViewShownEvent, value);
    }

    /// <summary>Raised when a view is hidden by a selection.</summary>
    public event EventHandler<PaneViewEventArgs>? ViewHidden
    {
        add => AddHandler(ViewHiddenEvent, value);
        remove => RemoveHandler(ViewHiddenEvent, value);
    }

    /// <summary>Raised when <see cref="SelectViewByName"/> names a view that does not exist.</summary>
    public event EventHandler<CreateViewEventArgs>? CreateView
    {
        add => AddHandler(CreateViewEvent, value);
        remove => RemoveHandler(CreateViewEvent, value);
    }

    /// <summary>Selects <paramref name="view"/>. With <paramref name="immediate"/> no transition plays.</summary>
    public void SelectView(Control? view, bool immediate = false)
    {
        if (view is null || !Views.Contains(view))
        {
            return;
        }

        Enqueue(view, immediate, isBack: false);
    }

    /// <summary>
    /// Selects the view whose <c>Name</c> is <paramref name="name"/>. When none exists, raises
    /// <see cref="CreateView"/> and selects the view it supplies. Returns the view, or null.
    /// </summary>
    public Control? SelectViewByName(string name, bool immediate = false)
    {
        var view = FindView(name);
        if (view is null)
        {
            var args = new CreateViewEventArgs(CreateViewEvent, this, name);
            RaiseEvent(args);
            if (args.View is { } created)
            {
                if (string.IsNullOrEmpty(created.Name))
                {
                    created.Name = name;
                }

                Views.Add(created);
                view = created;
            }
        }

        SelectView(view, immediate);
        return view;
    }

    /// <summary>Selects the view at <paramref name="index"/>. Returns it, or null when out of range.</summary>
    public Control? SelectViewByIndex(int index, bool immediate = false)
    {
        if (index < 0 || index >= Views.Count)
        {
            return null;
        }

        var view = Views[index];
        SelectView(view, immediate);
        return view;
    }

    /// <summary>Returns to the previous view in the history. Returns false when there is none.</summary>
    public bool Back()
    {
        while (_history.Count > 0)
        {
            var last = _history[^1];
            _history.RemoveAt(_history.Count - 1);
            if (Views.Contains(last))
            {
                CanGoBack = _history.Count > 0;
                Enqueue(last, immediate: false, isBack: true);
                return true;
            }
        }

        CanGoBack = false;
        return false;
    }

    /// <summary>Selects the next view, wrapping to the first after the last.</summary>
    public void Next()
    {
        if (Views.Count == 0)
        {
            return;
        }

        var i = SelectedIndex;
        SelectViewByIndex((i + 1) % Views.Count);
    }

    /// <summary>The view named <paramref name="name"/>, or null.</summary>
    public Control? FindView(string name)
    {
        foreach (var v in Views)
        {
            if (v.Name == name)
            {
                return v;
            }
        }

        return null;
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _host?.SetViews(Array.Empty<Control>());
        _host = e.NameScope.Find<PaneHost>("PART_Host");
        _blocker = e.NameScope.Find<Control>("PART_InputBlocker");
        _host?.SetViews(Views);
        if (_selected is null && Views.Count > 0)
        {
            Show(Views[0]);
        }

        UpdateVisibility();
    }

    private void OnViewsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (Control v in e.OldItems)
            {
                LogicalChildren.Remove(v);
                _history.RemoveAll(h => h == v);
                if (v == _selected)
                {
                    _selected = null;
                }
            }
        }

        if (e.NewItems is not null)
        {
            foreach (Control v in e.NewItems)
            {
                v.IsVisible = false;
                LogicalChildren.Add(v);
            }
        }

        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            LogicalChildren.Clear();
            LogicalChildren.AddRange(Views);
            _history.Clear();
            _selected = null;
        }

        _host?.SetViews(Views);
        CanGoBack = _history.Count > 0;
        if (_selected is null && Views.Count > 0)
        {
            Show(Views[0]);
        }

        UpdateVisibility();
    }

    private void Show(Control view)
    {
        _selected = view;
        (view as PaneView)?.Realize();
        RaisePropertyChanged(SelectedViewProperty, null, view);
        RaisePropertyChanged(SelectedIndexProperty, -1, SelectedIndex);
    }

    private void Enqueue(Control to, bool immediate, bool isBack)
    {
        if (_transitioning)
        {
            _queue.Enqueue((to, immediate, isBack));
            return;
        }

        _ = Transit(to, immediate, isBack);
    }

    private async Task Transit(Control to, bool immediate, bool isBack)
    {
        var from = _selected;
        if (to == from)
        {
            RaiseEvent(new PaneSelectionChangedEventArgs(SelectionChangedEvent, this, to, from));
            return;
        }

        var generation = ++_generation;
        if (from is not null && !isBack)
        {
            _history.Add(from);
            var max = Math.Max(0, MaxHistory);
            while (_history.Count > max)
            {
                _history.RemoveAt(0);
            }
        }

        CanGoBack = _history.Count > 0;
        var oldIndex = SelectedIndex;
        _selected = to;
        (to as PaneView)?.Realize();
        RaisePropertyChanged(SelectedViewProperty, from, to);
        RaisePropertyChanged(SelectedIndexProperty, oldIndex, SelectedIndex);
        RaiseEvent(new PaneSelectionChangedEventArgs(SelectionChangedEvent, this, to, from));
        if (from is not null)
        {
            RaiseEvent(new PaneViewEventArgs(ViewHiddenEvent, this, from));
        }

        var transition = immediate || from is null || _host is null || !EnyoAnimations.IsActive ? null : Transition;
        if (transition is not null)
        {
            SetTransitioning(true);
            _transitionCts = new CancellationTokenSource();
            try
            {
                await transition.Start(from, to, !isBack, _transitionCts.Token).ConfigureAwait(true);
            }
            finally
            {
                _transitionCts.Dispose();
                _transitionCts = null;
            }
        }

        if (generation != _generation)
        {
            return;
        }

        UpdateVisibility();
        SetTransitioning(false);
        RaiseEvent(new PaneViewEventArgs(ViewShownEvent, this, to));
        if (_queue.Count > 0)
        {
            var (next, nextImmediate, nextBack) = _queue.Dequeue();
            _ = Transit(next, nextImmediate, nextBack);
        }
    }

    private void SetTransitioning(bool value)
    {
        _transitioning = value;
        PseudoClasses.Set(":transitioning", value);
        if (_blocker is not null)
        {
            _blocker.IsVisible = value;
        }
    }

    private void UpdateVisibility()
    {
        foreach (var v in Views)
        {
            v.IsVisible = v == _selected;
            if (v != _selected)
            {
                v.ClearValue(OpacityProperty);
                v.ClearValue(RenderTransformProperty);
            }
        }
    }

    private void OnBackRequested(RoutedEventArgs e)
    {
        if (!e.Handled && CanGoBack && IsEffectivelyVisible)
        {
            e.Handled = Back();
        }
    }
}
