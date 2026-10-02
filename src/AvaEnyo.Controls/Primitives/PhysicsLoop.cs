using System;
using Avalonia;
using AvaEnyo.Animations;

namespace AvaEnyo.Controls.Primitives;

/// <summary>
/// Drives a scroll simulation once per frame until it rests. Real frame time is divided by
/// <see cref="EnyoAnimations.TimeScale"/>; with animations off the simulation is settled at once.
/// </summary>
internal sealed class PhysicsLoop
{
    private readonly Visual _host;
    private FrameClock? _clock;
    private Func<TimeSpan, bool>? _step;
    private Action? _rest;

    public PhysicsLoop(Visual host)
    {
        _host = host;
    }

    /// <summary>True while frames run.</summary>
    public bool IsRunning => _clock is not null;

    /// <summary>
    /// Starts stepping. <paramref name="step"/> advances the simulation by the scaled elapsed time and returns true
    /// while it moves; <paramref name="settle"/> runs it to rest at once when animations are off; <paramref name="rest"/>
    /// runs when it stops.
    /// </summary>
    public void Start(Func<TimeSpan, bool> step, Action settle, Action rest)
    {
        Stop();
        if (!EnyoAnimations.IsActive)
        {
            settle();
            rest();
            return;
        }

        _step = step;
        _rest = rest;
        _clock = new FrameClock(_host, OnFrame);
    }

    /// <summary>Stops without calling the rest callback.</summary>
    public void Stop()
    {
        _clock?.Dispose();
        _clock = null;
        _step = null;
        _rest = null;
    }

    private bool OnFrame(TimeSpan elapsed)
    {
        var scaled = TimeSpan.FromMilliseconds(elapsed.TotalMilliseconds / Math.Max(1e-6, EnyoAnimations.TimeScale));
        if (_step is { } step && step(scaled))
        {
            return true;
        }

        var rest = _rest;
        _clock = null;
        _step = null;
        _rest = null;
        rest?.Invoke();
        return false;
    }
}
