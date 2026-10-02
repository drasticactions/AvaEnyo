using System;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AvaEnyo.Services;

/// <summary>A test double of <see cref="IApplicationEvents"/> whose events are raised by its <c>Raise*</c> methods.</summary>
public sealed class TestApplicationEvents : IApplicationEvents
{
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

    /// <inheritdoc/>
    public void Relaunch(object? parameters)
    {
        var args = new RelaunchEventArgs(parameters);
        Relaunched?.Invoke(this, args);
        WindowParamsChanged?.Invoke(this, args);
    }

    /// <summary>Raises <see cref="Activated"/>.</summary>
    public void RaiseActivated() => Activated?.Invoke(this, EventArgs.Empty);

    /// <summary>Raises <see cref="Deactivated"/>.</summary>
    public void RaiseDeactivated() => Deactivated?.Invoke(this, EventArgs.Empty);

    /// <summary>Raises <see cref="BackRequested"/>.</summary>
    public void RaiseBackRequested() => BackRequested?.Invoke(this, new RoutedEventArgs(EnyoBack.BackRequestedEvent));

    /// <summary>Raises <see cref="KeyboardShown"/> or <see cref="KeyboardHidden"/>.</summary>
    public void RaiseKeyboard(bool shown, Rect occludedRect = default) =>
        (shown ? KeyboardShown : KeyboardHidden)?.Invoke(this, new KeyboardEventArgs(occludedRect));

    /// <summary>Raises <see cref="Rotated"/>.</summary>
    public void RaiseRotated(DisplayOrientation orientation) => Rotated?.Invoke(this, new RotatedEventArgs(orientation));

    /// <summary>Raises <see cref="Hidden"/> or <see cref="Shown"/>.</summary>
    public void RaiseVisibility(bool shown) => (shown ? Shown : Hidden)?.Invoke(this, EventArgs.Empty);

    /// <summary>Raises <see cref="KeyDown"/>.</summary>
    public void RaiseKeyDown(KeyEventArgs e) => KeyDown?.Invoke(this, e);

    /// <summary>Raises <see cref="KeyUp"/>.</summary>
    public void RaiseKeyUp(KeyEventArgs e) => KeyUp?.Invoke(this, e);
}
