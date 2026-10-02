using System;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Platform;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AvaEnyo.Services;

/// <summary>
/// The default <see cref="IApplicationEvents"/> for one window. Attach it once per <see cref="TopLevel"/> with
/// <see cref="Attach"/>; later calls return the same instance.
/// </summary>
/// <remarks>
/// Activation comes from the window (desktop) and from the application's <see cref="IActivatableLifetime"/>
/// (mobile and browser single-view lifetimes). Hidden and Shown come from minimizing and restoring a window, and from
/// background activation where the platform reports it; a browser tab's visibility is reported only when Avalonia's
/// browser lifetime raises background activation.
/// </remarks>
public sealed class ApplicationEvents : IApplicationEvents
{
    private static readonly ConditionalWeakTable<TopLevel, ApplicationEvents> s_attached = new();

    private readonly TopLevel _topLevel;
    private readonly IActivatableLifetime? _lifetime;
    private IInputPane? _inputPane;
    private DisplayOrientation? _orientation;
    private bool _minimized;
    private bool _detached;

    private ApplicationEvents(TopLevel topLevel)
    {
        _topLevel = topLevel;
        if (topLevel is WindowBase window)
        {
            window.Activated += OnWindowActivated;
            window.Deactivated += OnWindowDeactivated;
        }

        if (topLevel is Window w)
        {
            _minimized = w.WindowState == WindowState.Minimized;
        }

        _lifetime = Application.Current?.TryGetFeature(typeof(IActivatableLifetime)) as IActivatableLifetime;
        if (_lifetime is not null)
        {
            _lifetime.Activated += OnLifetimeActivated;
            _lifetime.Deactivated += OnLifetimeDeactivated;
        }

        _inputPane = topLevel.InputPane;
        if (_inputPane is not null)
        {
            _inputPane.StateChanged += OnInputPaneStateChanged;
        }

        topLevel.PropertyChanged += OnTopLevelPropertyChanged;
        topLevel.AddHandler(EnyoBack.BackRequestedEvent, OnBackRequested);
        topLevel.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Bubble, handledEventsToo: true);
        topLevel.AddHandler(InputElement.KeyUpEvent, OnKeyUp, RoutingStrategies.Bubble, handledEventsToo: true);
        _orientation = OrientationOf(topLevel.Bounds.Size);
    }

    /// <inheritdoc/>
    public event EventHandler? Activated;

    /// <inheritdoc/>
    public event EventHandler? Deactivated;

    /// <inheritdoc/>
    public event EventHandler<RoutedEventArgs>? BackRequested;

    /// <inheritdoc/>
    public event EventHandler<KeyboardEventArgs>? KeyboardShown;

    /// <inheritdoc/>
    public event EventHandler<KeyboardEventArgs>? KeyboardHidden;

    /// <inheritdoc/>
    public event EventHandler<RelaunchEventArgs>? Relaunched;

    /// <inheritdoc/>
    public event EventHandler<RelaunchEventArgs>? WindowParamsChanged;

    /// <inheritdoc/>
    public event EventHandler<RotatedEventArgs>? Rotated;

    /// <inheritdoc/>
    public event EventHandler? Hidden;

    /// <inheritdoc/>
    public event EventHandler? Shown;

    /// <inheritdoc/>
    public event EventHandler<KeyEventArgs>? KeyDown;

    /// <inheritdoc/>
    public event EventHandler<KeyEventArgs>? KeyUp;

    /// <summary>The window these events belong to.</summary>
    public TopLevel TopLevel => _topLevel;

    /// <summary>The events of <paramref name="topLevel"/>, created on first use.</summary>
    public static ApplicationEvents Attach(TopLevel topLevel)
    {
        ArgumentNullException.ThrowIfNull(topLevel);
        if (s_attached.TryGetValue(topLevel, out var existing))
        {
            return existing;
        }

        var events = new ApplicationEvents(topLevel);
        s_attached.AddOrUpdate(topLevel, events);
        return events;
    }

    /// <inheritdoc/>
    public void Relaunch(object? parameters)
    {
        var args = new RelaunchEventArgs(parameters);
        Relaunched?.Invoke(this, args);
        WindowParamsChanged?.Invoke(this, args);
    }

    /// <summary>Stops listening to the window.</summary>
    public void Detach()
    {
        if (_detached)
        {
            return;
        }

        _detached = true;
        s_attached.Remove(_topLevel);
        if (_topLevel is WindowBase window)
        {
            window.Activated -= OnWindowActivated;
            window.Deactivated -= OnWindowDeactivated;
        }

        if (_lifetime is not null)
        {
            _lifetime.Activated -= OnLifetimeActivated;
            _lifetime.Deactivated -= OnLifetimeDeactivated;
        }

        if (_inputPane is not null)
        {
            _inputPane.StateChanged -= OnInputPaneStateChanged;
            _inputPane = null;
        }

        _topLevel.PropertyChanged -= OnTopLevelPropertyChanged;
        _topLevel.RemoveHandler(EnyoBack.BackRequestedEvent, OnBackRequested);
        _topLevel.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown);
        _topLevel.RemoveHandler(InputElement.KeyUpEvent, OnKeyUp);
    }

    internal static DisplayOrientation OrientationOf(Size size) =>
        size.Height > size.Width ? DisplayOrientation.Portrait : DisplayOrientation.Landscape;

    private void OnWindowActivated(object? sender, EventArgs e) => Activated?.Invoke(this, EventArgs.Empty);

    private void OnWindowDeactivated(object? sender, EventArgs e) => Deactivated?.Invoke(this, EventArgs.Empty);

    private void OnLifetimeActivated(object? sender, ActivatedEventArgs e)
    {
        if (e.Kind == ActivationKind.Background)
        {
            Shown?.Invoke(this, EventArgs.Empty);
        }

        Activated?.Invoke(this, EventArgs.Empty);
    }

    private void OnLifetimeDeactivated(object? sender, ActivatedEventArgs e)
    {
        Deactivated?.Invoke(this, EventArgs.Empty);
        if (e.Kind == ActivationKind.Background)
        {
            Hidden?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnInputPaneStateChanged(object? sender, InputPaneStateEventArgs e)
    {
        var args = new KeyboardEventArgs(e.EndRect);
        if (e.NewState == InputPaneState.Open)
        {
            KeyboardShown?.Invoke(this, args);
        }
        else
        {
            KeyboardHidden?.Invoke(this, args);
        }
    }

    private void OnTopLevelPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Visual.BoundsProperty)
        {
            var size = e.GetNewValue<Rect>().Size;
            if (size.Width <= 0 || size.Height <= 0)
            {
                return;
            }

            var orientation = OrientationOf(size);
            if (_orientation is { } old && old != orientation)
            {
                _orientation = orientation;
                Rotated?.Invoke(this, new RotatedEventArgs(orientation));
            }

            _orientation = orientation;
        }
        else if (e.Property == Window.WindowStateProperty)
        {
            var minimized = e.GetNewValue<WindowState>() == WindowState.Minimized;
            if (minimized != _minimized)
            {
                _minimized = minimized;
                (minimized ? Hidden : Shown)?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void OnBackRequested(object? sender, RoutedEventArgs e)
    {
        if (!e.Handled)
        {
            BackRequested?.Invoke(this, e);
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e) => KeyDown?.Invoke(this, e);

    private void OnKeyUp(object? sender, KeyEventArgs e) => KeyUp?.Invoke(this, e);
}
