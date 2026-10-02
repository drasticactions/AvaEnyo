using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation.Easings;

namespace AvaEnyo.Animations;

/// <summary>
/// The motion of Enyo 1.0 as <see cref="Task"/>-returning helpers. Each helper writes plain end values when it
/// finishes, so a control carries no live animation afterwards. A new animation on a property that is still
/// animating starts from the value on screen and supersedes the old one.
/// </summary>
public static partial class EnyoAnimations
{
    /// <summary>
    /// Turns every animation off when false: the helpers jump to their end states. Physics jumps to rest.
    /// </summary>
    public static bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Multiplies every duration. 1 is real time, 5 is five times slower, 0 jumps to the end. Tests use 0.
    /// </summary>
    public static double TimeScale { get; set; } = 1;

    /// <summary>True when animations run: enabled and a time scale above 0.</summary>
    public static bool IsActive => IsEnabled && TimeScale > 0;

    /// <summary>
    /// The general Enyo animator: calls <paramref name="onAnimate"/> with a value from <paramref name="from"/> to
    /// <paramref name="to"/> once per frame, eased over <paramref name="duration"/>. The last call always gets
    /// <paramref name="to"/>. Returns true at the end and false when canceled.
    /// </summary>
    public static Task<bool> Animate(double from, double to, TimeSpan duration, Easing easing, Action<double> onAnimate, Visual? host = null, CancellationToken cancellationToken = default) =>
        Animate(from, to, duration, easing, (v, _) => onAnimate(v), host, cancellationToken);

    /// <summary>
    /// As <see cref="Animate(double, double, TimeSpan, Easing, Action{double}, Visual?, CancellationToken)"/>, with the
    /// eased progress (0..1) passed as the second argument, as Enyo's <c>onAnimate(value, progress)</c>.
    /// </summary>
    public static Task<bool> Animate(double from, double to, TimeSpan duration, Easing easing, Action<double, double> onAnimate, Visual? host, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(easing);
        ArgumentNullException.ThrowIfNull(onAnimate);
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(false);
        }

        var total = duration.TotalMilliseconds * Math.Max(0, TimeScale);
        if (!IsActive || total <= 0)
        {
            onAnimate(to, 1);
            return Task.FromResult(true);
        }

        var tcs = new TaskCompletionSource<bool>();
        var t = 0d;
        CancellationTokenRegistration registration = default;
        FrameClock? clock = null;
        clock = new FrameClock(host, elapsed =>
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }

            t += elapsed.TotalMilliseconds;
            if (t >= total)
            {
                onAnimate(to, 1);
                registration.Dispose();
                tcs.TrySetResult(true);
                return false;
            }

            var p = easing.Ease(t / total);
            onAnimate(from + ((to - from) * p), p);
            return true;
        });

        registration = cancellationToken.Register(() =>
        {
            clock.Dispose();
            tcs.TrySetResult(false);
        });

        // The first frame shows the start value, as Enyo's play() called animate() synchronously.
        onAnimate(from, 0);
        return tcs.Task;
    }
}
