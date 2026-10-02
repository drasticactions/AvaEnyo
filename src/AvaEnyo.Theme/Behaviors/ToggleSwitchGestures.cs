using System;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaEnyo.Primitives;

namespace AvaEnyo;

/// <summary>
/// Gives a <see cref="ToggleSwitch"/> the input of Enyo's <c>ToggleButton</c>: a tap toggles it; a horizontal drag
/// sets it on (rightward) or off (leftward) each time it moves 15 px; a horizontal flick sets it in the direction of
/// travel. A release after a drag does not also toggle.
/// </summary>
internal static class ToggleSwitchGestures
{
    /// <summary>The drag distance that switches the state: 15 px (Enyo <c>ToggleButton.dragHandler</c>).</summary>
    public const double DragDistance = 15;

    /// <summary>The release speed above which a horizontal release counts as a flick, in px/ms.</summary>
    public const double FlickVelocity = 0.5;

    private static readonly ConditionalWeakTable<ToggleSwitch, State> s_states = new();

    public static void Register()
    {
        TemplatedControl.TemplateAppliedEvent.AddClassHandler<ToggleSwitch>((toggle, _) => Attach(toggle));
        InputElement.PointerReleasedEvent.AddClassHandler<ToggleSwitch>(OnReleasedTunnel, RoutingStrategies.Tunnel);
    }

    private static void Attach(ToggleSwitch toggle)
    {
        if (s_states.TryGetValue(toggle, out _))
        {
            return;
        }

        var state = new State();
        var tracker = new DragTracker(toggle) { CanStart = info => info.IsHorizontal };
        state.Tracker = tracker;
        tracker.Started += (_, e) => state.Dx0 = e.Delta.X;
        tracker.Moved += (_, e) =>
        {
            var d = e.Delta.X - state.Dx0;
            if (Math.Abs(d) > DragDistance)
            {
                Set(toggle, d > 0);
                state.Dx0 = e.Delta.X;
            }
        };
        tracker.Completed += (_, e) =>
        {
            if (Math.Abs(e.Velocity.X) > Math.Abs(e.Velocity.Y) && Math.Abs(e.Velocity.X) > FlickVelocity)
            {
                Set(toggle, e.Velocity.X > 0);
            }
        };
        s_states.Add(toggle, state);
    }

    private static void OnReleasedTunnel(ToggleSwitch toggle, PointerReleasedEventArgs e)
    {
        // A drag owns the release: keep the button from also clicking.
        if (s_states.TryGetValue(toggle, out var state) && state.Tracker is { IsDragging: true })
        {
            e.Handled = true;
        }
    }

    private static void Set(ToggleSwitch toggle, bool value)
    {
        if (toggle.IsEffectivelyEnabled && toggle.IsChecked != value)
        {
            toggle.SetCurrentValue(ToggleButton.IsCheckedProperty, value);
        }
    }

    private sealed class State
    {
        public DragTracker? Tracker;
        public double Dx0;
    }
}
