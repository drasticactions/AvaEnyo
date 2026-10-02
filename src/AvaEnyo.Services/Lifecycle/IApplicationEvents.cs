using System;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AvaEnyo.Services;

/// <summary>
/// App-level events in the shape of Enyo's <c>ApplicationEvents</c>: activation, back, the on-screen keyboard,
/// relaunch, rotation, visibility and keys.
/// </summary>
public interface IApplicationEvents
{
    /// <summary>The app or window became active.</summary>
    event EventHandler? Activated;

    /// <summary>The app or window lost activation. Popups with <c>AutoClose</c> close.</summary>
    event EventHandler? Deactivated;

    /// <summary>The AvaEnyo back chain reached the window and nothing handled it.</summary>
    event EventHandler<RoutedEventArgs>? BackRequested;

    /// <summary>The on-screen keyboard opened.</summary>
    event EventHandler<KeyboardEventArgs>? KeyboardShown;

    /// <summary>The on-screen keyboard closed.</summary>
    event EventHandler<KeyboardEventArgs>? KeyboardHidden;

    /// <summary>The app was launched again while running (Enyo <c>onApplicationRelaunch</c>).</summary>
    event EventHandler<RelaunchEventArgs>? Relaunched;

    /// <summary>The launch parameters changed (Enyo <c>onWindowParamsChange</c>).</summary>
    event EventHandler<RelaunchEventArgs>? WindowParamsChanged;

    /// <summary>The window turned between portrait and landscape.</summary>
    event EventHandler<RotatedEventArgs>? Rotated;

    /// <summary>The window was minimized or sent to the background.</summary>
    event EventHandler? Hidden;

    /// <summary>The window came back from minimized or the background.</summary>
    event EventHandler? Shown;

    /// <summary>A key went down anywhere in the window.</summary>
    event EventHandler<KeyEventArgs>? KeyDown;

    /// <summary>A key went up anywhere in the window.</summary>
    event EventHandler<KeyEventArgs>? KeyUp;

    /// <summary>Reports a relaunch with new parameters: raises <see cref="Relaunched"/> and <see cref="WindowParamsChanged"/>.</summary>
    void Relaunch(object? parameters);
}
