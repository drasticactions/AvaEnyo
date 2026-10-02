using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaEnyo.Controls.Primitives;

namespace AvaEnyo.Controls;

/// <summary>
/// The Enyo row (<c>enyo.Item</c>): 10 px padding, a light top edge and a dark bottom edge. With
/// <see cref="TapHighlight"/> a press shows the held gradient at once; otherwise it shows after a 200 ms hold
/// (<c>:held</c>). A press that ends in place raises <see cref="Click"/>; a press that turns into a drag (a scroller
/// or a sliding view takes the pointer) does not.
/// </summary>
/// <remarks>
/// Order pseudo-classes come from an <see cref="OrderedPanel"/>: <c>:first</c> has no top edge, <c>:last</c> no
/// bottom edge. Inside a <see cref="Group"/>, or with the <c>rounded</c> class, the corners are rounded by position.
/// A <c>VirtualList</c> also uses the divider slot (<see cref="ShowDivider"/>) to start a section.
/// </remarks>
[TemplatePart("PART_ContentPresenter", typeof(Avalonia.Controls.Presenters.ContentPresenter), IsRequired = true)]
[TemplatePart("PART_Divider", typeof(ContentControl))]
[PseudoClasses(":pressed", ":held", ":selected", ":first", ":middle", ":last", ":single")]
public class Item : ContentControl, ISelectable
{
    /// <summary>Defines the <see cref="IsSelected"/> property.</summary>
    public static readonly StyledProperty<bool> IsSelectedProperty =
        SelectingItemsControl.IsSelectedProperty.AddOwner<Item>();

    /// <summary>Defines the <see cref="TapHighlight"/> property.</summary>
    public static readonly StyledProperty<bool> TapHighlightProperty =
        AvaloniaProperty.Register<Item, bool>(nameof(TapHighlight), true);

    /// <summary>Defines the <see cref="IsHeld"/> property.</summary>
    public static readonly DirectProperty<Item, bool> IsHeldProperty =
        AvaloniaProperty.RegisterDirect<Item, bool>(nameof(IsHeld), o => o.IsHeld);

    /// <summary>Defines the <see cref="ShowDivider"/> property.</summary>
    public static readonly StyledProperty<bool> ShowDividerProperty =
        AvaloniaProperty.Register<Item, bool>(nameof(ShowDivider));

    /// <summary>Defines the <see cref="DividerCaption"/> property.</summary>
    public static readonly StyledProperty<string?> DividerCaptionProperty =
        AvaloniaProperty.Register<Item, string?>(nameof(DividerCaption));

    /// <summary>Defines the <see cref="DividerKind"/> property.</summary>
    public static readonly StyledProperty<DividerKind> DividerKindProperty =
        AvaloniaProperty.Register<Item, DividerKind>(nameof(DividerKind));

    /// <summary>Defines the <see cref="Click"/> event.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
        RoutedEvent.Register<Item, RoutedEventArgs>(nameof(Click), RoutingStrategies.Bubble);

    private IPointer? _pressed;
    private bool _isHeld;
    private DispatcherTimer? _clickDelay;

    static Item()
    {
        FocusableProperty.OverrideDefaultValue<Item>(true);
        IsSelectedProperty.Changed.AddClassHandler<Item>((x, _) => x.UpdatePseudoClasses());
        EnyoGestures.HoldEvent.AddClassHandler<Item>((x, e) => x.OnHold(e));
    }

    /// <summary>Initializes a new instance of the <see cref="Item"/> class.</summary>
    public Item()
    {
        AddHandler(PointerCaptureLostEvent, (_, e) => OnCaptureLost(e), RoutingStrategies.Direct | RoutingStrategies.Bubble, handledEventsToo: true);
    }

    /// <summary>True when the row is selected; shows the held gradient (<c>enyo-item-selected</c>).</summary>
    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>
    /// True (default): the held gradient shows as soon as the row is pressed. False: only after a 200 ms hold.
    /// </summary>
    public bool TapHighlight
    {
        get => GetValue(TapHighlightProperty);
        set => SetValue(TapHighlightProperty, value);
    }

    /// <summary>True while the row shows the held gradient because of a press or hold.</summary>
    public bool IsHeld
    {
        get => _isHeld;
        private set => SetAndRaise(IsHeldProperty, ref _isHeld, value);
    }

    /// <summary>True to show a divider above the row (a list section start).</summary>
    public bool ShowDivider
    {
        get => GetValue(ShowDividerProperty);
        set => SetValue(ShowDividerProperty, value);
    }

    /// <summary>The caption of the divider above the row.</summary>
    public string? DividerCaption
    {
        get => GetValue(DividerCaptionProperty);
        set => SetValue(DividerCaptionProperty, value);
    }

    /// <summary>The kind of divider above the row.</summary>
    public DividerKind DividerKind
    {
        get => GetValue(DividerKindProperty);
        set => SetValue(DividerKindProperty, value);
    }

    /// <summary>Raised when the row is tapped or activated with Enter or Space.</summary>
    public event EventHandler<RoutedEventArgs>? Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!IsEffectivelyEnabled || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        _pressed = e.Pointer;
        ((IPseudoClasses)Classes).Set(":pressed", true);
        if (TapHighlight)
        {
            IsHeld = true;
        }

        UpdatePseudoClasses();
    }

    /// <inheritdoc/>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.Pointer != _pressed)
        {
            return;
        }

        var inside = new Rect(Bounds.Size).Contains(e.GetPosition(this));
        EndPress();
        if (inside && e.InitialPressMouseButton == MouseButton.Left && !e.Handled)
        {
            e.Handled = true;
            RaiseClickSoon();
        }
    }

    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!e.Handled && (e.Key == Key.Enter || e.Key == Key.Space))
        {
            e.Handled = true;
            RaiseClick();
        }
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdatePseudoClasses();
    }

    /// <summary>Raises <see cref="Click"/>, as a tap would.</summary>
    public void RaiseClick()
    {
        if (IsEffectivelyEnabled && CanClick)
        {
            OnClick();
            RaiseEvent(new RoutedEventArgs(ClickEvent, this));
        }
    }

    /// <summary>False while the row must not click, such as while a swipe confirm prompt covers it.</summary>
    protected virtual bool CanClick => true;

    /// <summary>Called before <see cref="Click"/> is raised.</summary>
    protected virtual void OnClick()
    {
    }

    private void RaiseClickSoon()
    {
        if (!ThemeSettings.DelayItemClick)
        {
            RaiseClick();
            return;
        }

        // Enyo delayed the click 100 ms so the held gradient was seen before the handler ran.
        IsHeld = true;
        UpdatePseudoClasses();
        _clickDelay?.Stop();
        _clickDelay = new DispatcherTimer(TimeSpan.FromMilliseconds(ThemeSettings.ItemClickDelay), DispatcherPriority.Input, (_, _) =>
        {
            _clickDelay?.Stop();
            _clickDelay = null;
            IsHeld = false;
            UpdatePseudoClasses();
            RaiseClick();
        });
        _clickDelay.Start();
    }

    private void OnHold(HoldEventArgs e)
    {
        if (_pressed is not null && IsEffectivelyEnabled)
        {
            IsHeld = true;
            UpdatePseudoClasses();
        }
    }

    private void OnCaptureLost(PointerCaptureLostEventArgs e)
    {
        if (e.Pointer != _pressed)
        {
            return;
        }

        // The pointer was taken by a drag (a scroller, a sliding view, a swipe): no click.
        if (e.Pointer.Captured is Visual v && (v == this || this.IsVisualAncestorOf(v)))
        {
            return;
        }

        EndPress();
    }

    /// <summary>Ends the current press without a click.</summary>
    protected void CancelPress() => EndPress();

    private void EndPress()
    {
        _pressed = null;
        IsHeld = false;
        ((IPseudoClasses)Classes).Set(":pressed", false);
        UpdatePseudoClasses();
    }

    /// <summary>Updates <c>:held</c> and <c>:selected</c>.</summary>
    protected void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":held", IsHeld);
        PseudoClasses.Set(":selected", IsSelected);
    }
}
