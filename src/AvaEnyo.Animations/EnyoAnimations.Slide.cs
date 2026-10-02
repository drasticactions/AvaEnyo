using System.Threading;
using System.Threading.Tasks;
using Avalonia.Animation.Easings;
using Avalonia.Controls;

namespace AvaEnyo.Animations;

public static partial class EnyoAnimations
{
    /// <summary>
    /// The Pane fly-in (<c>enyo.transitions.LeftRightFlyin</c>). Forward: the incoming control slides from
    /// <paramref name="distance"/> (the pane width) to 0 over the outgoing one. Backward (invented, Enyo always flew in
    /// from the right): the outgoing control slides off to the right over the incoming one. 300 ms, cubic ease-out.
    /// </summary>
    public static async Task<bool> FlyIn(Control? from, Control? to, double distance, bool forward = true, CancellationToken cancellationToken = default)
    {
        var moving = forward ? to : from;
        var still = forward ? from : to;
        if (moving is null)
        {
            return true;
        }

        if (still is not null)
        {
            still.ZIndex = 1;
        }

        moving.ZIndex = 2;
        var start = forward ? AnimationRunner.Translate(distance, 0) : AnimationRunner.Identity;
        var end = forward ? AnimationRunner.Identity : AnimationRunner.Translate(distance, 0);
        var done = await AnimationRunner.Run(moving, [AnimationRunner.Transform(start, end, 0, EnyoDurations.PaneFlyIn, EnyoEasing.CubicOut)], cancellationToken).ConfigureAwait(true);
        if (!forward && done)
        {
            moving.ClearValue(Control.RenderTransformProperty);
        }

        still?.ClearValue(Control.ZIndexProperty);
        moving.ClearValue(Control.ZIndexProperty);
        return done;
    }

    /// <summary>
    /// Slides a control horizontally to <paramref name="x"/> (a translate). Default 700 ms, cubic ease-out (the
    /// SlidingPane slide).
    /// </summary>
    public static Task<bool> SlideTo(Control control, double x, double? durationMs = null, Easing? easing = null, CancellationToken cancellationToken = default)
    {
        var to = x == 0 ? AnimationRunner.Identity : AnimationRunner.Translate(x, 0);
        return AnimationRunner.Run(control, [AnimationRunner.Transform(AnimationRunner.Current(control), to, 0, durationMs ?? EnyoDurations.SlidingPane, easing ?? EnyoEasing.CubicOut)], cancellationToken);
    }
}
