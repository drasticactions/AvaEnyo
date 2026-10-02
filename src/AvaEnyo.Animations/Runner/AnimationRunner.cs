using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace AvaEnyo.Animations;

/// <summary>
/// Runs <see cref="AnimationTrack"/>s as Avalonia <see cref="Animation"/>s. Afterwards the control has plain property
/// values and no live animation.
/// </summary>
internal static class AnimationRunner
{
    public static readonly TransformOperations Identity = TransformOperations.Identity;

    static AnimationRunner()
    {
        // Avalonia 12.1 has no keyframe animator for ITransform (only TransformOperationsTransition). Without this
        // registration, RenderTransform keyframes throw "No animator registered".
        Animation.RegisterCustomAnimator<ITransform?, TransformInterpolator>();
    }

    private sealed class TransformInterpolator : InterpolatingAnimator<ITransform?>
    {
        public override ITransform? Interpolate(double progress, ITransform? oldValue, ITransform? newValue) =>
            TransformOperations.Interpolate(
                oldValue as TransformOperations ?? TransformOperations.Identity,
                newValue as TransformOperations ?? TransformOperations.Identity,
                progress);
    }

    /// <summary>Makes sure the static constructor ran, so the transform animator is registered.</summary>
    public static void EnsureInitialized()
    {
    }

    public static TransformOperations Translate(double x, double y)
    {
        var b = TransformOperations.CreateBuilder(1);
        b.AppendTranslate(x, y);
        return b.Build();
    }

    public static TransformOperations Scale(double s)
    {
        var b = TransformOperations.CreateBuilder(1);
        b.AppendScale(s, s);
        return b.Build();
    }

    /// <summary>Milliseconds scaled by <see cref="EnyoAnimations.TimeScale"/>.</summary>
    public static TimeSpan Ms(double ms) => TimeSpan.FromMilliseconds(ms * Math.Max(0, EnyoAnimations.TimeScale));

    public static bool ShouldSkip(Visual control) =>
        !EnyoAnimations.IsEnabled || EnyoAnimations.TimeScale <= 0 || !control.IsAttachedToVisualTree();

    /// <summary>
    /// Runs every track on the control at the same time, then writes the final values.
    /// </summary>
    public static Task<bool> Run(Control control, params AnimationTrack[] tracks) => Run(control, tracks, CancellationToken.None);

    /// <summary>
    /// Runs the tracks with cancellation. A canceled run stops the live animations at once and does not write the end
    /// values. Returns true when the run completed and false when it was canceled or superseded.
    /// </summary>
    /// <remarks>
    /// A control has one live run per property. Starting a run on a property that is still animating supersedes the
    /// earlier run (it is canceled and reports false), and the new run starts from the value on screen at that
    /// moment instead of its own From, as a CSS transition would. Without this, the runs would stack: Avalonia
    /// layers animations on a property, and once the top one finished the ones beneath would show through and
    /// replay until the last of them ended.
    /// </remarks>
    public static async Task<bool> Run(Control control, IReadOnlyList<AnimationTrack> tracks, CancellationToken cancellationToken)
    {
        if (tracks.Count == 0)
        {
            return true;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return false;
        }

        if (ShouldSkip(control))
        {
            CancelLive(control, tracks);
            foreach (var t in tracks)
            {
                ApplyFinal(control, t);
            }

            return true;
        }

        var run = Supersede(control, tracks);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, run.Cancellation.Token);
        var token = cts.Token;

        // Write the end values as the base values first. Avalonia disposes a finished animation before this method
        // resumes, and a render can run in between. With the base already at the end state, that frame is the same
        // as the last keyframe instead of a flash of the pre-animation look.
        foreach (var t in run.Tracks)
        {
            ApplyFinal(control, t);
        }

        var runs = new Task[run.Tracks.Length];
        for (var i = 0; i < run.Tracks.Length; i++)
        {
            var t = run.Tracks[i];
            var animation = new Animation
            {
                Delay = t.Delay,
                Duration = t.Duration,
                Easing = t.Easing,
                FillMode = FillMode.Both,
            };
            animation.Children.Add(new KeyFrame { Cue = new Cue(0), Setters = { new Setter(t.Property, t.From) } });
            animation.Children.Add(new KeyFrame { Cue = new Cue(1), Setters = { new Setter(t.Property, t.To) } });
            runs[i] = animation.RunAsync(control, token);
        }

        try
        {
            await Task.WhenAll(runs).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            Release(control, run);
        }

        if (token.IsCancellationRequested)
        {
            return false;
        }

        foreach (var t in run.Tracks)
        {
            ApplyFinal(control, t);
        }

        return true;
    }

    /// <summary>The live runs of a control, kept only while they run.</summary>
    private sealed class LiveRun(AnimationTrack[] tracks)
    {
        public AnimationTrack[] Tracks { get; } = tracks;
        public CancellationTokenSource Cancellation { get; } = new();
    }

    private static readonly ConditionalWeakTable<Control, List<LiveRun>> s_live = new();

    /// <summary>True while a run animates <paramref name="property"/> on the control.</summary>
    public static bool IsAnimating(Control control, AvaloniaProperty property)
    {
        if (!s_live.TryGetValue(control, out var live))
        {
            return false;
        }

        foreach (var run in live)
        {
            if (Animates(run.Tracks, property))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Cancels every live run on the control that animates one of the track properties.</summary>
    private static void CancelLive(Control control, IReadOnlyList<AnimationTrack> tracks)
    {
        if (!s_live.TryGetValue(control, out var live))
        {
            return;
        }

        for (var i = live.Count - 1; i >= 0; i--)
        {
            if (Overlaps(live[i].Tracks, tracks))
            {
                var previous = live[i];
                live.RemoveAt(i);
                previous.Cancellation.Cancel();
            }
        }
    }

    /// <summary>
    /// Registers a run on the control. Any live run that animates one of the same properties is canceled first, and
    /// the new tracks on those properties pick up the value showing at that moment as their From.
    /// </summary>
    private static LiveRun Supersede(Control control, IReadOnlyList<AnimationTrack> tracks)
    {
        var live = s_live.GetOrCreateValue(control);
        var resumed = new AnimationTrack[tracks.Count];
        for (var i = 0; i < resumed.Length; i++)
        {
            resumed[i] = tracks[i];
        }

        for (var i = live.Count - 1; i >= 0; i--)
        {
            var previous = live[i];
            if (!Overlaps(previous.Tracks, tracks))
            {
                continue;
            }

            // The animated value is only readable while the animation is bound, so it is taken before the cancel.
            for (var j = 0; j < resumed.Length; j++)
            {
                var t = resumed[j];
                if (Animates(previous.Tracks, t.Property) && LiveValue(control, t) is { } from)
                {
                    resumed[j] = t with { From = from };
                }
            }

            live.RemoveAt(i);
            previous.Cancellation.Cancel();
        }

        var run = new LiveRun(resumed);
        live.Add(run);
        return run;
    }

    private static void Release(Control control, LiveRun run)
    {
        if (s_live.TryGetValue(control, out var live))
        {
            live.Remove(run);
        }

        run.Cancellation.Dispose();
    }

    private static bool Overlaps(AnimationTrack[] previous, IReadOnlyList<AnimationTrack> next)
    {
        for (var i = 0; i < next.Count; i++)
        {
            if (Animates(previous, next[i].Property))
            {
                return true;
            }
        }

        return false;
    }

    private static bool Animates(AnimationTrack[] tracks, AvaloniaProperty property)
    {
        foreach (var t in tracks)
        {
            if (t.Property == property)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>The value a track's property shows right now: the interpolated value of a live animation.</summary>
    private static object? LiveValue(Control control, AnimationTrack t)
    {
        if (t.Property == Visual.RenderTransformProperty)
        {
            return control.RenderTransform as TransformOperations ?? Identity;
        }

        return control.GetValue(t.Property);
    }

    /// <summary>
    /// Writes the end value of a track. An identity transform or opacity 1 clears the local value, so styles and
    /// the theme's setters apply again.
    /// </summary>
    public static void ApplyFinal(Control control, AnimationTrack t)
    {
        if (t.Property == Visual.RenderTransformProperty)
        {
            if (t.To is TransformOperations { IsIdentity: true })
            {
                control.ClearValue(Visual.RenderTransformProperty);
            }
            else
            {
                control.SetValue(Visual.RenderTransformProperty, (ITransform?)t.To);
            }
        }
        else if (t.Property == Visual.OpacityProperty)
        {
            var v = (double)t.To;
            if (v >= 1)
            {
                control.ClearValue(Visual.OpacityProperty);
            }
            else
            {
                control.SetValue(Visual.OpacityProperty, v);
            }
        }
        else
        {
            control.SetValue(t.Property, t.To);
        }
    }

    /// <summary>
    /// Keeps an eased opacity inside 0..1. Avalonia renders an opacity above 1 as wrapped alpha, so it is clamped.
    /// </summary>
    private sealed class ClampedEasing(Easing inner) : Easing
    {
        public override double Ease(double progress) => Math.Clamp(inner.Ease(progress), 0, 1);
    }

    public static AnimationTrack Opacity(double from, double to, double delayMs, double durationMs, Easing easing) =>
        new(Visual.OpacityProperty, from, to, Ms(delayMs), Ms(durationMs), new ClampedEasing(easing));

    public static AnimationTrack Transform(TransformOperations from, TransformOperations to, double delayMs, double durationMs, Easing easing) =>
        new(Visual.RenderTransformProperty, from, to, Ms(delayMs), Ms(durationMs), easing);

    public static AnimationTrack Double(AvaloniaProperty property, double from, double to, double delayMs, double durationMs, Easing easing) =>
        new(property, from, to, Ms(delayMs), Ms(durationMs), easing);

    /// <summary>The current <see cref="TransformOperations"/> of the control, or identity when it has none.</summary>
    public static TransformOperations Current(Control control) =>
        control.RenderTransform as TransformOperations ?? Identity;

    public static IReadOnlyList<Control> ToList(IEnumerable<Control>? controls)
    {
        if (controls is null)
        {
            return Array.Empty<Control>();
        }

        if (controls is IReadOnlyList<Control> list)
        {
            return list;
        }

        return new List<Control>(controls);
    }

    /// <summary>Runs one task per control and awaits them all. True when every run completed.</summary>
    public static async Task<bool> WhenAll(IReadOnlyList<Control> controls, Func<Control, Task<bool>> run)
    {
        if (controls.Count == 0)
        {
            return true;
        }

        var tasks = new Task<bool>[controls.Count];
        for (var i = 0; i < controls.Count; i++)
        {
            tasks[i] = run(controls[i]);
        }

        var results = await Task.WhenAll(tasks).ConfigureAwait(true);
        return Array.TrueForAll(results, r => r);
    }
}
