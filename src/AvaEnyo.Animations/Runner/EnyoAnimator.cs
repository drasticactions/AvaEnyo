using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation.Easings;

namespace AvaEnyo.Animations;

/// <summary>
/// Enyo's <c>enyo.Animator</c>: runs one value animation at a time and calls back on every frame with the value and
/// the eased progress. <see cref="Play"/> stops the animation already playing, whose task then completes with false.
/// </summary>
public sealed class EnyoAnimator
{
    private CancellationTokenSource? _current;

    /// <summary>The duration. Default 350 ms (the Enyo default).</summary>
    public TimeSpan Duration { get; set; } = TimeSpan.FromMilliseconds(EnyoDurations.Default);

    /// <summary>The easing. Default <see cref="EnyoEasing.CubicOut"/> (the Enyo default).</summary>
    public Easing Easing { get; set; } = EnyoEasing.CubicOut;

    /// <summary>The visual whose top level drives the frames. Optional.</summary>
    public Visual? Host { get; set; }

    /// <summary>Called on every frame with the value and the eased progress (Enyo <c>onAnimate</c>).</summary>
    public event Action<double, double>? Animating;

    /// <summary>True while an animation plays.</summary>
    public bool IsAnimating => _current is not null;

    /// <summary>
    /// Animates from <paramref name="start"/> to <paramref name="end"/>. Returns true when the animation reached the
    /// end and false when <see cref="Stop"/> or a later <see cref="Play"/> stopped it.
    /// </summary>
    public async Task<bool> Play(double start, double end)
    {
        Stop();
        var cts = new CancellationTokenSource();
        _current = cts;
        try
        {
            return await EnyoAnimations.Animate(start, end, Duration, Easing, (v, p) => Animating?.Invoke(v, p), Host, cts.Token)
                .ConfigureAwait(true);
        }
        finally
        {
            if (_current == cts)
            {
                _current = null;
            }

            cts.Dispose();
        }
    }

    /// <summary>Stops the animation where it is.</summary>
    public void Stop()
    {
        var c = _current;
        _current = null;
        c?.Cancel();
    }
}
