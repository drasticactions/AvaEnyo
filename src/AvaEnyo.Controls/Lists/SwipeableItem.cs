using System;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using AvaEnyo.Animations;
using AvaEnyo.Primitives;

namespace AvaEnyo.Controls;

/// <summary>
/// A row that can be swiped sideways to delete it (<c>enyo.SwipeableItem</c>). A horizontal drag moves the row 1:1;
/// a release past 35 % of its width raises <see cref="Swiped"/>, then shows the inline <see cref="ConfirmPrompt"/> (or
/// raises <see cref="Confirmed"/> at once when <see cref="ConfirmRequired"/> is off). The prompt shows under the row
/// as it slides; a swiped row slides off and returns when the prompt closes. A shorter drag returns the row.
/// Vertical drags are left to the list's scroller.
/// </summary>
/// <remarks>
/// Enyo snapped the row back at once; AvaEnyo animates it back over <see cref="SnapBackMilliseconds"/> (150 ms, cubic
/// ease-out; 0 snaps). In a <see cref="VirtualList"/> only one row shows its prompt at a time.
/// </remarks>
[TemplatePart("PART_Border", typeof(Control), IsRequired = true)]
[TemplatePart("PART_ConfirmPrompt", typeof(ConfirmPrompt), IsRequired = true)]
[PseudoClasses(":confirming", ":swiping", ":revealing")]
public class SwipeableItem : Item
{
    /// <summary>The fraction of the row width a release must pass to count as a swipe: 0.35 (<c>triggerRatio</c>).</summary>
    public static double TriggerRatio = 0.35;

    /// <summary>How long a short drag takes to return: 150 ms. 0 snaps back as Enyo did.</summary>
    public static double SnapBackMilliseconds = 150;

    /// <summary>Defines the <see cref="Swipeable"/> property.</summary>
    public static readonly StyledProperty<bool> SwipeableProperty =
        AvaloniaProperty.Register<SwipeableItem, bool>(nameof(Swipeable), true);

    /// <summary>Defines the <see cref="ConfirmRequired"/> property.</summary>
    public static readonly StyledProperty<bool> ConfirmRequiredProperty =
        AvaloniaProperty.Register<SwipeableItem, bool>(nameof(ConfirmRequired), true);

    /// <summary>Defines the <see cref="ConfirmCaption"/> property.</summary>
    public static readonly StyledProperty<object?> ConfirmCaptionProperty =
        AvaloniaProperty.Register<SwipeableItem, object?>(nameof(ConfirmCaption), "Delete");

    /// <summary>Defines the <see cref="CancelCaption"/> property.</summary>
    public static readonly StyledProperty<object?> CancelCaptionProperty =
        AvaloniaProperty.Register<SwipeableItem, object?>(nameof(CancelCaption), "Cancel");

    /// <summary>Defines the <see cref="ConfirmShowing"/> property.</summary>
    public static readonly StyledProperty<bool> ConfirmShowingProperty =
        AvaloniaProperty.Register<SwipeableItem, bool>(nameof(ConfirmShowing));

    /// <summary>Defines the <see cref="ConfirmWhenAutoHidden"/> property.</summary>
    public static readonly StyledProperty<bool> ConfirmWhenAutoHiddenProperty =
        AvaloniaProperty.Register<SwipeableItem, bool>(nameof(ConfirmWhenAutoHidden));

    /// <summary>Defines the <see cref="AllowLeft"/> property.</summary>
    public static readonly StyledProperty<bool> AllowLeftProperty =
        AvaloniaProperty.Register<SwipeableItem, bool>(nameof(AllowLeft), true);

    /// <summary>Defines the <see cref="Swiped"/> event.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> SwipedEvent =
        RoutedEvent.Register<SwipeableItem, RoutedEventArgs>(nameof(Swiped), RoutingStrategies.Bubble);

    /// <summary>Defines the <see cref="Confirmed"/> event.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> ConfirmedEvent =
        RoutedEvent.Register<SwipeableItem, RoutedEventArgs>(nameof(Confirmed), RoutingStrategies.Bubble);

    /// <summary>Defines the <see cref="Cancelled"/> event.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> CancelledEvent =
        RoutedEvent.Register<SwipeableItem, RoutedEventArgs>(nameof(Cancelled), RoutingStrategies.Bubble);

    /// <summary>Defines the <see cref="ConfirmShowingChanged"/> event.</summary>
    public static readonly RoutedEvent<ConfirmShowingChangedEventArgs> ConfirmShowingChangedEvent =
        RoutedEvent.Register<SwipeableItem, ConfirmShowingChangedEventArgs>(nameof(ConfirmShowingChanged), RoutingStrategies.Bubble);

    /// <summary>Defines the <see cref="Dragging"/> event.</summary>
    public static readonly RoutedEvent<SwipeDraggingEventArgs> DraggingEvent =
        RoutedEvent.Register<SwipeableItem, SwipeDraggingEventArgs>(nameof(Dragging), RoutingStrategies.Bubble);

    private readonly DragTracker _drag;
    private readonly TranslateTransform _translate = new();
    private Control? _row;
    private ConfirmPrompt? _prompt;
    private double _startDx;
    private bool _autoHiding;
    private bool _didAutoConfirm;
    private CancellationTokenSource? _snapBack;
    private bool _rowOff;

    static SwipeableItem()
    {
        ConfirmShowingProperty.Changed.AddClassHandler<SwipeableItem>((s, e) => s.OnConfirmShowingChanged(e.GetNewValue<bool>()));
        ConfirmPrompt.ConfirmedEvent.AddClassHandler<SwipeableItem>((s, e) => s.OnPromptConfirmed(e));
        ConfirmPrompt.CancelledEvent.AddClassHandler<SwipeableItem>((s, e) => s.OnPromptCancelled(e));
    }

    /// <summary>Initializes a new instance of the <see cref="SwipeableItem"/> class.</summary>
    public SwipeableItem()
    {
        _drag = new DragTracker(this) { CanStart = CanStartSwipe };
        _drag.Moved += (_, e) => OnSwipeMoved(e.Delta.X);
        _drag.Started += (_, e) => OnSwipeStarted(e.Delta.X);
        _drag.Completed += (_, e) => OnSwipeFinished(e.Delta.X);
        _drag.Canceled += (_, _) => OnSwipeFinished(0);
    }

    /// <summary>True (default): the row can be swiped.</summary>
    public bool Swipeable
    {
        get => GetValue(SwipeableProperty);
        set => SetValue(SwipeableProperty, value);
    }

    /// <summary>True (default): a swipe shows the confirm prompt. False: a swipe raises <see cref="Confirmed"/> at once.</summary>
    public bool ConfirmRequired
    {
        get => GetValue(ConfirmRequiredProperty);
        set => SetValue(ConfirmRequiredProperty, value);
    }

    /// <summary>The Confirm button content. Default "Delete".</summary>
    public object? ConfirmCaption
    {
        get => GetValue(ConfirmCaptionProperty);
        set => SetValue(ConfirmCaptionProperty, value);
    }

    /// <summary>The Cancel button content. Default "Cancel".</summary>
    public object? CancelCaption
    {
        get => GetValue(CancelCaptionProperty);
        set => SetValue(CancelCaptionProperty, value);
    }

    /// <summary>True while the confirm prompt covers the row.</summary>
    public bool ConfirmShowing
    {
        get => GetValue(ConfirmShowingProperty);
        set => SetValue(ConfirmShowingProperty, value);
    }

    /// <summary>True: when another row's prompt hides this row's prompt, this row raises <see cref="Confirmed"/>.</summary>
    public bool ConfirmWhenAutoHidden
    {
        get => GetValue(ConfirmWhenAutoHiddenProperty);
        set => SetValue(ConfirmWhenAutoHiddenProperty, value);
    }

    /// <summary>True (default): the row can be swiped to the left as well as to the right.</summary>
    public bool AllowLeft
    {
        get => GetValue(AllowLeftProperty);
        set => SetValue(AllowLeftProperty, value);
    }

    /// <summary>The current horizontal offset of the row.</summary>
    public double SwipeOffset => _translate.X;

    /// <summary>Raised when a swipe passes the trigger distance.</summary>
    public event EventHandler<RoutedEventArgs>? Swiped
    {
        add => AddHandler(SwipedEvent, value);
        remove => RemoveHandler(SwipedEvent, value);
    }

    /// <summary>Raised when Confirm is pressed, or on a swipe when no confirmation is required.</summary>
    public event EventHandler<RoutedEventArgs>? Confirmed
    {
        add => AddHandler(ConfirmedEvent, value);
        remove => RemoveHandler(ConfirmedEvent, value);
    }

    /// <summary>Raised when Cancel is pressed.</summary>
    public event EventHandler<RoutedEventArgs>? Cancelled
    {
        add => AddHandler(CancelledEvent, value);
        remove => RemoveHandler(CancelledEvent, value);
    }

    /// <summary>Raised when the confirm prompt shows or hides.</summary>
    public event EventHandler<ConfirmShowingChangedEventArgs>? ConfirmShowingChanged
    {
        add => AddHandler(ConfirmShowingChangedEvent, value);
        remove => RemoveHandler(ConfirmShowingChangedEvent, value);
    }

    /// <summary>Raised as the row is dragged, with its offset from rest.</summary>
    public event EventHandler<SwipeDraggingEventArgs>? Dragging
    {
        add => AddHandler(DraggingEvent, value);
        remove => RemoveHandler(DraggingEvent, value);
    }

    /// <inheritdoc/>
    protected override bool CanClick => !ConfirmShowing;

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _row?.ClearValue(RenderTransformProperty);
        _row = e.NameScope.Find<Control>("PART_Border");
        _prompt = e.NameScope.Find<ConfirmPrompt>("PART_ConfirmPrompt");
        if (_row is not null)
        {
            _row.RenderTransform = _translate;
        }

        UpdateLayers();
    }

    /// <summary>Hides the prompt because another row showed one; auto-confirms when asked to.</summary>
    internal bool AutoHide()
    {
        if (!ConfirmShowing)
        {
            return false;
        }

        _autoHiding = true;
        try
        {
            ConfirmShowing = false;
        }
        finally
        {
            _autoHiding = false;
        }

        if (ConfirmWhenAutoHidden)
        {
            RaiseEvent(new RoutedEventArgs(ConfirmedEvent, this));
            return true;
        }

        return false;
    }

    /// <summary>Puts the row back at rest without events (a recycled container).</summary>
    internal void ResetSwipe()
    {
        _drag.Reset();
        _snapBack?.Cancel();
        _translate.X = 0;
        _rowOff = false;
        _autoHiding = true;
        try
        {
            SetCurrentValue(ConfirmShowingProperty, false);
        }
        finally
        {
            _autoHiding = false;
        }

        UpdateLayers();
    }

    private bool CanStartSwipe(DragStartInfo info) => Swipeable && info.IsHorizontal && !ConfirmShowing;

    private double EffectiveDx(double dx) => dx > 0 || AllowLeft ? dx : 0;

    private void OnSwipeStarted(double dx)
    {
        _snapBack?.Cancel();
        CancelPress();
        _startDx = EffectiveDx(dx);
        PseudoClasses.Set(":swiping", true);
        OnSwipeMoved(dx);
    }

    private void OnSwipeMoved(double dx)
    {
        var offset = EffectiveDx(dx) - _startDx;
        if (!AllowLeft)
        {
            offset = Math.Max(0, offset);
        }

        SetOffset(offset);
    }

    private void SetOffset(double offset)
    {
        _translate.X = offset;
        UpdateLayers();
        RaiseEvent(new SwipeDraggingEventArgs(DraggingEvent, this, offset));
    }

    /// <summary>Shows the prompt under a moved row, and hides a row that slid off.</summary>
    private void UpdateLayers()
    {
        var revealing = ConfirmShowing || (ConfirmRequired && _translate.X != 0);
        PseudoClasses.Set(":revealing", revealing);
        if (_prompt is not null)
        {
            _prompt.IsVisible = revealing;
            _prompt.IsHitTestVisible = ConfirmShowing;
        }

        if (_row is not null)
        {
            var hidden = ConfirmShowing && (_rowOff || _translate.X == 0);
            _row.Opacity = hidden ? 0 : 1;
            _row.IsHitTestVisible = !hidden;
        }
    }

    private void OnSwipeFinished(double dx)
    {
        PseudoClasses.Set(":swiping", false);
        var trigger = Math.Floor(Bounds.Width * TriggerRatio);
        var swiped = Math.Abs(EffectiveDx(dx)) > trigger && trigger > 0;
        if (swiped && ConfirmRequired)
        {
            RaiseEvent(new RoutedEventArgs(SwipedEvent, this));
            SlideOff();
            ConfirmShowing = true;
            return;
        }

        SnapBack();
        if (swiped)
        {
            RaiseEvent(new RoutedEventArgs(SwipedEvent, this));
            RaiseEvent(new RoutedEventArgs(ConfirmedEvent, this));
        }
    }

    private void SnapBack() => SlideTo(0, null);

    private void SlideOff()
    {
        var edge = Math.Max(Bounds.Width, Math.Abs(_translate.X));
        SlideTo(_translate.X < 0 ? -edge : edge, () =>
        {
            _rowOff = true;
            UpdateLayers();
        });
    }

    private void SlideTo(double to, Action? done)
    {
        _snapBack?.Cancel();
        _rowOff = false;
        var from = _translate.X;
        if (from == to || SnapBackMilliseconds <= 0)
        {
            SetOffset(to);
            done?.Invoke();
            return;
        }

        var cts = new CancellationTokenSource();
        _snapBack = cts;
        _ = AnimateTo(from, to, done, cts);
    }

    private async System.Threading.Tasks.Task AnimateTo(double from, double to, Action? done, CancellationTokenSource cts)
    {
        await EnyoAnimations.Animate(from, to, TimeSpan.FromMilliseconds(SnapBackMilliseconds), EnyoEasing.CubicOut, SetOffset, this, cts.Token);
        if (!cts.IsCancellationRequested)
        {
            done?.Invoke();
        }
    }

    private void OnConfirmShowingChanged(bool show)
    {
        PseudoClasses.Set(":confirming", show);
        if (!show && _translate.X != 0)
        {
            SnapBack();
        }

        UpdateLayers();
        _didAutoConfirm = false;
        if (show && !_autoHiding)
        {
            _didAutoConfirm = this.FindAncestorOfType<VirtualList>()?.OnRowConfirming(this) ?? false;
        }

        RaiseEvent(new ConfirmShowingChangedEventArgs(ConfirmShowingChangedEvent, this, show, _didAutoConfirm));
    }

    private void OnPromptConfirmed(RoutedEventArgs e)
    {
        if (e.Source != _prompt || e.Handled)
        {
            return;
        }

        e.Handled = true;
        ConfirmShowing = false;
        RaiseEvent(new RoutedEventArgs(ConfirmedEvent, this));
    }

    private void OnPromptCancelled(RoutedEventArgs e)
    {
        if (e.Source != _prompt || e.Handled)
        {
            return;
        }

        e.Handled = true;
        ConfirmShowing = false;
        RaiseEvent(new RoutedEventArgs(CancelledEvent, this));
    }
}
