using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaEnyo.Controls.Primitives;

namespace AvaEnyo.Controls;

/// <summary>
/// The base of every AvaEnyo popup (Enyo <c>BasicPopup</c> and <c>Popup</c>). The declared control is a zero-size
/// placeholder; when it opens, its <see cref="PopupPresenter"/> is added to the window's overlay layer, above the
/// rest of the window. This works the same on desktop and in the browser.
/// </summary>
/// <remarks>
/// <para>Content can be set directly or built lazily from <see cref="ContentTemplate"/> on first open. Placement is
/// chosen by the <c>Open*</c> methods and is always kept inside the window, the safe area and above the on-screen
/// keyboard; it is recomputed when the window resizes or an ancestor scroller of the anchor moves.</para>
/// <para>A click outside closes the popup when <see cref="DismissWithClick"/> is on and it is not modal; Escape or
/// back close the topmost popup when <see cref="DismissWithEscape"/> is on. Focus moves into the popup and returns
/// when it closes.</para>
/// </remarks>
public partial class PopupBase : TemplatedControl
{
    /// <summary>Defines the <see cref="IsOpen"/> property.</summary>
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<PopupBase, bool>(nameof(IsOpen), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Defines the <see cref="IsModal"/> property.</summary>
    public static readonly StyledProperty<bool> IsModalProperty =
        AvaloniaProperty.Register<PopupBase, bool>(nameof(IsModal));

    /// <summary>Defines the <see cref="DismissWithClick"/> property.</summary>
    public static readonly StyledProperty<bool> DismissWithClickProperty =
        AvaloniaProperty.Register<PopupBase, bool>(nameof(DismissWithClick), true);

    /// <summary>Defines the <see cref="DismissWithEscape"/> property.</summary>
    public static readonly StyledProperty<bool> DismissWithEscapeProperty =
        AvaloniaProperty.Register<PopupBase, bool>(nameof(DismissWithEscape), true);

    /// <summary>Defines the <see cref="ShowScrim"/> property.</summary>
    public static readonly StyledProperty<bool> ShowScrimProperty =
        AvaloniaProperty.Register<PopupBase, bool>(nameof(ShowScrim));

    /// <summary>Defines the <see cref="ScrimWhenModal"/> property.</summary>
    public static readonly StyledProperty<bool> ScrimWhenModalProperty =
        AvaloniaProperty.Register<PopupBase, bool>(nameof(ScrimWhenModal), true);

    /// <summary>Defines the <see cref="ShareScrim"/> property.</summary>
    public static readonly StyledProperty<bool> ShareScrimProperty =
        AvaloniaProperty.Register<PopupBase, bool>(nameof(ShareScrim), true);

    /// <summary>Defines the <see cref="AutoClose"/> property.</summary>
    public static readonly StyledProperty<bool> AutoCloseProperty =
        AvaloniaProperty.Register<PopupBase, bool>(nameof(AutoClose));

    /// <summary>Defines the <see cref="Content"/> property.</summary>
    public static readonly StyledProperty<object?> ContentProperty =
        ContentControl.ContentProperty.AddOwner<PopupBase>();

    /// <summary>Defines the <see cref="ContentTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        ContentControl.ContentTemplateProperty.AddOwner<PopupBase>();

    /// <summary>Defines the <see cref="BeforeOpen"/> event.</summary>
    public static readonly RoutedEvent<PopupOpeningEventArgs> BeforeOpenEvent =
        RoutedEvent.Register<PopupBase, PopupOpeningEventArgs>(nameof(BeforeOpen), RoutingStrategies.Bubble);

    /// <summary>Defines the <see cref="Opened"/> event.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> OpenedEvent =
        RoutedEvent.Register<PopupBase, RoutedEventArgs>(nameof(Opened), RoutingStrategies.Bubble);

    /// <summary>Defines the <see cref="Closed"/> event.</summary>
    public static readonly RoutedEvent<PopupClosedEventArgs> ClosedEvent =
        RoutedEvent.Register<PopupBase, PopupClosedEventArgs>(nameof(Closed), RoutingStrategies.Bubble);

    private bool _syncingIsOpen;
    private bool _everOpened;
    private int _generation;
    private object? _builtContent;
    private IInputElement? _restoreFocus;
    private OverlayHost? _host;

    static PopupBase()
    {
        IsOpenProperty.Changed.AddClassHandler<PopupBase>((p, e) => p.OnIsOpenChanged(e.GetNewValue<bool>()));
        IsVisibleProperty.Changed.AddClassHandler<PopupBase>((p, e) =>
        {
            if (!e.GetNewValue<bool>() && p.IsOpen)
            {
                p.CloseCore(PopupCloseReason.Hidden, null);
            }
        });
        Button.ClickEvent.AddClassHandler<PopupPresenter>((presenter, e) => presenter.Owner?.OnPresenterClick(e));
    }

    /// <summary>True while the popup is shown. Setting it opens (at the last placement) or closes the popup.</summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>True: outside clicks do not close the popup, and a scrim shows when <see cref="ScrimWhenModal"/>.</summary>
    public bool IsModal
    {
        get => GetValue(IsModalProperty);
        set => SetValue(IsModalProperty, value);
    }

    /// <summary>True (default): a click outside closes the popup, unless it is modal.</summary>
    public bool DismissWithClick
    {
        get => GetValue(DismissWithClickProperty);
        set => SetValue(DismissWithClickProperty, value);
    }

    /// <summary>True (default): Escape or back close the popup when it is the topmost one.</summary>
    public bool DismissWithEscape
    {
        get => GetValue(DismissWithEscapeProperty);
        set => SetValue(DismissWithEscapeProperty, value);
    }

    /// <summary>True: a scrim dims the window below the popup.</summary>
    public bool ShowScrim
    {
        get => GetValue(ShowScrimProperty);
        set => SetValue(ShowScrimProperty, value);
    }

    /// <summary>True (default): a modal popup shows a scrim.</summary>
    public bool ScrimWhenModal
    {
        get => GetValue(ScrimWhenModalProperty);
        set => SetValue(ScrimWhenModalProperty, value);
    }

    /// <summary>True (default): popups share one scrim, which moves below the topmost popup that wants one.</summary>
    public bool ShareScrim
    {
        get => GetValue(ShareScrimProperty);
        set => SetValue(ShareScrimProperty, value);
    }

    /// <summary>True: the popup closes when the window is deactivated (Enyo <c>autoHide</c>).</summary>
    public bool AutoClose
    {
        get => GetValue(AutoCloseProperty);
        set => SetValue(AutoCloseProperty, value);
    }

    /// <summary>The content.</summary>
    [Content]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>A template for content built on first open (Enyo <c>lazy</c>) when <see cref="Content"/> is not set.</summary>
    public IDataTemplate? ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    /// <summary>Raised before the popup opens; cancelable.</summary>
    public event EventHandler<PopupOpeningEventArgs>? BeforeOpen
    {
        add => AddHandler(BeforeOpenEvent, value);
        remove => RemoveHandler(BeforeOpenEvent, value);
    }

    /// <summary>Raised when the popup has opened.</summary>
    public event EventHandler<RoutedEventArgs>? Opened
    {
        add => AddHandler(OpenedEvent, value);
        remove => RemoveHandler(OpenedEvent, value);
    }

    /// <summary>Raised when the popup has closed, with the reason.</summary>
    public event EventHandler<PopupClosedEventArgs>? Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    /// <summary>The presenter while the popup is open or closing.</summary>
    public PopupPresenter? Presenter { get; private set; }

    internal Scrim? OwnScrim { get; set; }

    internal bool WantsScrim => ShowScrim || (IsModal && ScrimWhenModal);

    /// <summary>Opens the popup at its last placement, centered the first time.</summary>
    public void Open()
    {
        _placement ??= PlaceAtCenter;
        OpenCore();
    }

    /// <summary>Closes the popup with <see cref="PopupCloseReason.Code"/> and an optional value.</summary>
    public void Close(object? value = null) => CloseCore(PopupCloseReason.Code, value);

    /// <summary>Creates the presenter. Popup types override it to use their own presenter and theme.</summary>
    protected virtual PopupPresenter CreatePresenter() => new PopupPresenter();

    /// <summary>Plays the opening motion. Default none.</summary>
    protected virtual Task AnimateOpenAsync(PopupPresenter presenter) => Task.CompletedTask;

    /// <summary>Plays the closing motion. Default none.</summary>
    protected virtual Task AnimateCloseAsync(PopupPresenter presenter) => Task.CompletedTask;

    /// <summary>Called when the presenter is in the overlay, before <see cref="Opened"/>.</summary>
    protected virtual void OnPresenterAttached(PopupPresenter presenter)
    {
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize) => default;

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize) => finalSize;

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (IsOpen)
        {
            CloseCore(PopupCloseReason.Hidden, null);
        }

        base.OnDetachedFromVisualTree(e);
    }

    private void OnIsOpenChanged(bool open)
    {
        if (_syncingIsOpen)
        {
            return;
        }

        if (open)
        {
            _placement ??= PlaceAtCenter;
            OpenCore();
        }
        else
        {
            CloseCore(PopupCloseReason.Code, null);
        }
    }

    private void SetIsOpen(bool value)
    {
        _syncingIsOpen = true;
        try
        {
            SetCurrentValue(IsOpenProperty, value);
        }
        finally
        {
            _syncingIsOpen = false;
        }
    }

    private void OpenCore()
    {
        if (Presenter is not null && _host is not null && System.Linq.Enumerable.Contains(_host.Stack, this))
        {
            // Already open: only the placement changed.
            _host.InvalidateArrange();
            return;
        }

        var args = new PopupOpeningEventArgs(BeforeOpenEvent, this, !_everOpened);
        RaiseEvent(args);
        if (args.Cancel)
        {
            SetIsOpen(false);
            return;
        }

        _host = OverlayHost.For(this);
        if (_host is null)
        {
            SetIsOpen(false);
            return;
        }

        _everOpened = true;
        var generation = ++_generation;
        var presenter = CreatePresenter();
        presenter.Owner = this;
        presenter.Bind(DataContextProperty, this.GetObservable(DataContextProperty));
        presenter.Content = Content ?? (_builtContent ??= ContentTemplate?.Build(DataContext));
        Presenter = presenter;
        _restoreFocus = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement();
        SubscribeAnchorScrollers();
        _host.Push(this);
        SetIsOpen(true);
        OnPresenterAttached(presenter);
        _host.UpdateLayout();
        FocusInto(presenter);
        _ = AnimateOpenAsync(presenter);
        if (generation == _generation)
        {
            RaiseEvent(new RoutedEventArgs(OpenedEvent, this));
        }
    }

    /// <summary>Closes the popup with a reason; the base of every close path.</summary>
    internal async void CloseCore(PopupCloseReason reason, object? value)
    {
        if (Presenter is not { } presenter)
        {
            SetIsOpen(false);
            return;
        }

        var generation = ++_generation;
        SetIsOpen(false);
        UnsubscribeAnchorScrollers();
        await AnimateCloseAsync(presenter).ConfigureAwait(true);
        if (generation != _generation)
        {
            return;
        }

        _host?.Remove(this);
        presenter.Content = null;
        presenter.Owner = null;
        Presenter = null;
        if (_restoreFocus is { } focus && focus is Visual v && v.IsAttachedToVisualTree())
        {
            focus.Focus();
        }

        _restoreFocus = null;
        RaiseEvent(new PopupClosedEventArgs(ClosedEvent, this, reason, value));
    }

    private void OnPresenterClick(RoutedEventArgs e)
    {
        for (var v = e.Source as Visual; v is not null && v != Presenter; v = v.GetVisualParent())
        {
            if (v is Control c && c.IsSet(Popup.CloseValueProperty))
            {
                CloseCore(PopupCloseReason.Handle, c.GetValue(Popup.CloseValueProperty));
                return;
            }
        }
    }

    private static void FocusInto(PopupPresenter presenter)
    {
        foreach (var d in presenter.GetVisualDescendants())
        {
            if (d is InputElement { Focusable: true, IsEffectivelyEnabled: true, IsEffectivelyVisible: true } input)
            {
                input.Focus();
                return;
            }
        }

        presenter.Focusable = true;
        presenter.Focus();
    }
}
