using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AvaEnyo.Controls.Primitives;

/// <summary>
/// A full-size panel in the window's overlay layer that hosts open popups (Enyo's popup layer). Popups stack in
/// open order with z-order count·2 + 121; one shared <see cref="Scrim"/> sits just below the topmost popup that wants
/// one. A transparent layer below the topmost popup takes outside clicks: it closes a popup that is dismissed with a
/// click, and swallows the click under a modal one. Popups are kept inside the window minus the safe area and the
/// on-screen keyboard.
/// </summary>
internal sealed class OverlayHost : Panel
{
    private static readonly ConditionalWeakTable<TopLevel, OverlayHost> s_hosts = new();

    private readonly List<PopupBase> _stack = new();
    private readonly Scrim _scrim = new();
    private readonly Border _clickEater = new() { Background = Avalonia.Media.Brushes.Transparent, IsVisible = false };
    private readonly TopLevel _top;
    private readonly OverlayLayer _layer;
    private IDisposable? _backInterceptor;

    private OverlayHost(TopLevel top, OverlayLayer layer)
    {
        _top = top;
        _layer = layer;
        Children.Add(_clickEater);
        Children.Add(_scrim);
        _clickEater.AddHandler(PointerPressedEvent, OnOutsidePressed, RoutingStrategies.Bubble, handledEventsToo: true);
        _scrim.AddHandler(PointerPressedEvent, OnOutsidePressed, RoutingStrategies.Bubble, handledEventsToo: true);
        layer.PropertyChanged += OnLayerPropertyChanged;
        if (top is WindowBase window)
        {
            window.Deactivated += (_, _) => CloseAutoClose();
        }

        SyncSize();
    }

    /// <summary>The popups, bottom to top.</summary>
    public IReadOnlyList<PopupBase> Stack => _stack;

    /// <summary>The topmost popup, or null.</summary>
    public PopupBase? Topmost => _stack.Count > 0 ? _stack[^1] : null;

    /// <summary>The shared scrim.</summary>
    public Scrim SharedScrim => _scrim;

    /// <summary>The host of the window that contains <paramref name="visual"/>, created on first use.</summary>
    public static OverlayHost? For(Visual visual)
    {
        if (TopLevel.GetTopLevel(visual) is not { } top)
        {
            return null;
        }

        if (s_hosts.TryGetValue(top, out var existing))
        {
            return existing;
        }

        if (OverlayLayer.GetOverlayLayer(visual) is not { } layer)
        {
            return null;
        }

        var host = new OverlayHost(top, layer);
        layer.Children.Add(host);
        s_hosts.AddOrUpdate(top, host);
        return host;
    }

    /// <summary>The host of <paramref name="top"/> if one exists.</summary>
    public static OverlayHost? Existing(TopLevel top) => s_hosts.TryGetValue(top, out var host) ? host : null;

    /// <summary>The area popups may use: the host minus the safe area and the on-screen keyboard.</summary>
    public Rect Viewport => ViewportFor(Bounds.Size);

    private Rect ViewportFor(Size size)
    {
        {
            var rect = new Rect(size);
            if (_top.InsetsManager?.SafeAreaPadding is { } safe)
            {
                rect = rect.Deflate(safe);
            }

            if (_top.InputPane is { State: Avalonia.Controls.Platform.InputPaneState.Open } pane && pane.OccludedRect.Height > 0)
            {
                var occludedTop = pane.OccludedRect.Top;
                if (occludedTop > rect.Top && occludedTop < rect.Bottom)
                {
                    rect = rect.WithHeight(occludedTop - rect.Top);
                }
            }

            return rect;
        }
    }

    /// <summary>Adds a popup's presenter on top.</summary>
    public void Push(PopupBase popup)
    {
        if (_stack.Contains(popup) || popup.Presenter is not { } presenter)
        {
            return;
        }

        _stack.Add(popup);
        Children.Add(presenter);
        _backInterceptor ??= EnyoBack.AddInterceptor(OnBack);
        UpdateLayers();
        InvalidateMeasure();
    }

    /// <summary>Removes a popup's presenter.</summary>
    public void Remove(PopupBase popup)
    {
        if (!_stack.Remove(popup))
        {
            return;
        }

        if (popup.Presenter is { } presenter)
        {
            Children.Remove(presenter);
        }

        if (popup.OwnScrim is { } own)
        {
            Children.Remove(own);
            popup.OwnScrim = null;
        }

        if (_stack.Count == 0)
        {
            _backInterceptor?.Dispose();
            _backInterceptor = null;
        }

        UpdateLayers();
        InvalidateMeasure();
    }

    /// <summary>Recomputes z-order, the scrim and the click layer.</summary>
    public void UpdateLayers()
    {
        var scrimBelow = -1;
        for (var i = 0; i < _stack.Count; i++)
        {
            var popup = _stack[i];
            var z = ((i + 1) * 2) + 121;
            if (popup.Presenter is { } presenter)
            {
                presenter.ZIndex = z;
            }

            if (popup.WantsScrim)
            {
                if (popup.ShareScrim)
                {
                    scrimBelow = z - 1;
                }
                else
                {
                    if (popup.OwnScrim is null)
                    {
                        popup.OwnScrim = new Scrim();
                        popup.OwnScrim.AddHandler(PointerPressedEvent, OnOutsidePressed, RoutingStrategies.Bubble, handledEventsToo: true);
                        Children.Add(popup.OwnScrim);
                    }

                    popup.OwnScrim.ZIndex = z - 1;
                    popup.OwnScrim.IsOpen = true;
                }
            }
        }

        if (scrimBelow >= 0)
        {
            _scrim.ZIndex = scrimBelow;
            _scrim.IsOpen = true;
        }
        else
        {
            _scrim.IsOpen = false;
        }

        _clickEater.IsVisible = _stack.Count > 0;
        _clickEater.ZIndex = _stack.Count > 0 ? (_stack.Count * 2) + 121 - 1 : 0;
        IsHitTestVisible = _stack.Count > 0 || _scrim.IsOpen;
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        var viewport = ViewportFor(new Size(
            double.IsInfinity(availableSize.Width) ? _layer.Bounds.Width : availableSize.Width,
            double.IsInfinity(availableSize.Height) ? _layer.Bounds.Height : availableSize.Height));
        foreach (var child in Children)
        {
            if (child is PopupPresenter p)
            {
                p.MaxWidth = Math.Max(0, viewport.Width);
                p.MaxHeight = Math.Max(0, viewport.Height);
                p.Measure(viewport.Size);
            }
            else
            {
                child.Measure(availableSize);
            }
        }

        return new Size(double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width, double.IsInfinity(availableSize.Height) ? 0 : availableSize.Height);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var viewport = ViewportFor(finalSize);
        foreach (var child in Children)
        {
            if (child is PopupPresenter { Owner: { } popup } p)
            {
                var size = p.DesiredSize;
                var position = popup.ComputePosition(size, viewport, this);
                var x = Math.Clamp(position.X, viewport.Left, Math.Max(viewport.Left, viewport.Right - size.Width));
                var y = Math.Clamp(position.Y, viewport.Top, Math.Max(viewport.Top, viewport.Bottom - size.Height));
                p.Arrange(new Rect(new Point(x, y), size));
            }
            else
            {
                child.Arrange(new Rect(finalSize));
            }
        }

        return finalSize;
    }

    private void OnLayerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == BoundsProperty)
        {
            SyncSize();
        }
    }

    private void SyncSize()
    {
        Width = _layer.Bounds.Width;
        Height = _layer.Bounds.Height;
        InvalidateMeasure();
    }

    private void OnOutsidePressed(object? sender, PointerPressedEventArgs e)
    {
        e.Handled = true;
        if (Topmost is { } top && top.DismissWithClick && !top.IsModal)
        {
            top.CloseCore(PopupCloseReason.Click, null);
        }
    }

    private bool OnBack(TopLevel top, BackSource source)
    {
        if (top != _top || Topmost is not { } popup || !popup.DismissWithEscape)
        {
            return false;
        }

        popup.CloseCore(source == BackSource.Escape ? PopupCloseReason.Escape : PopupCloseReason.Back, null);
        return true;
    }

    private void CloseAutoClose()
    {
        for (var i = _stack.Count - 1; i >= 0; i--)
        {
            if (_stack[i].AutoClose)
            {
                _stack[i].CloseCore(PopupCloseReason.AutoClose, null);
            }
        }
    }
}
