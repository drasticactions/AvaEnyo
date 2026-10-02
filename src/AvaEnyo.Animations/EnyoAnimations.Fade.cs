using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Animation.Easings;
using Avalonia.Controls;

namespace AvaEnyo.Animations;

public static partial class EnyoAnimations
{
    /// <summary>Fades the controls in from opacity 0 to 1. Default 300 ms, cubic ease-out (the Pane fade).</summary>
    public static Task<bool> FadeIn(IEnumerable<Control> controls, double? durationMs = null, Easing? easing = null, CancellationToken cancellationToken = default) =>
        AnimationRunner.WhenAll(AnimationRunner.ToList(controls), c => FadeIn(c, durationMs, easing, cancellationToken));

    /// <summary>Fades one control in from opacity 0 to 1.</summary>
    public static Task<bool> FadeIn(Control control, double? durationMs = null, Easing? easing = null, CancellationToken cancellationToken = default) =>
        AnimationRunner.Run(control, [AnimationRunner.Opacity(0, 1, 0, durationMs ?? EnyoDurations.PaneFade, easing ?? EnyoEasing.CubicOut)], cancellationToken);

    /// <summary>Fades the controls out from their current opacity to 0. Default 300 ms, cubic ease-out.</summary>
    public static Task<bool> FadeOut(IEnumerable<Control> controls, double? durationMs = null, Easing? easing = null, CancellationToken cancellationToken = default) =>
        AnimationRunner.WhenAll(AnimationRunner.ToList(controls), c => FadeOut(c, durationMs, easing, cancellationToken));

    /// <summary>Fades one control out from its current opacity to 0.</summary>
    public static Task<bool> FadeOut(Control control, double? durationMs = null, Easing? easing = null, CancellationToken cancellationToken = default) =>
        AnimationRunner.Run(control, [AnimationRunner.Opacity(control.Opacity, 0, 0, durationMs ?? EnyoDurations.PaneFade, easing ?? EnyoEasing.CubicOut)], cancellationToken);

    /// <summary>
    /// The Pane fade (<c>enyo.transitions.Fade</c>): the incoming control fades in from 0 over the outgoing one,
    /// which stays fully visible underneath. 300 ms, cubic ease-out. The caller hides the outgoing control afterwards.
    /// </summary>
    public static Task<bool> CrossFade(Control? from, Control? to, CancellationToken cancellationToken = default)
    {
        if (to is null)
        {
            return Task.FromResult(true);
        }

        if (from is not null)
        {
            from.ZIndex = 1;
        }

        to.ZIndex = 2;
        return Finish(FadeIn(to, EnyoDurations.PaneFade, EnyoEasing.CubicOut, cancellationToken), from, to);

        static async Task<bool> Finish(Task<bool> run, Control? from, Control to)
        {
            var done = await run.ConfigureAwait(true);
            from?.ClearValue(Control.ZIndexProperty);
            to.ClearValue(Control.ZIndexProperty);
            return done;
        }
    }
}
