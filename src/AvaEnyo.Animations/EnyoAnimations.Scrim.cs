using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace AvaEnyo.Animations;

public static partial class EnyoAnimations
{
    /// <summary>The opacity of a dim scrim: 0.65 (<c>enyo-scrim-dim</c>).</summary>
    public static double ScrimOpacity = 0.65;

    /// <summary>Fades a scrim in to <paramref name="opacity"/> (default 0.65). 500 ms, CSS ease, as the CSS transition.</summary>
    public static Task<bool> ScrimShow(Control scrim, double? opacity = null, CancellationToken cancellationToken = default)
    {
        scrim.IsVisible = true;
        return AnimationRunner.Run(scrim, [AnimationRunner.Opacity(scrim.Opacity >= 1 && !AnimationRunner.IsAnimating(scrim, Control.OpacityProperty) ? 0 : scrim.Opacity, opacity ?? ScrimOpacity, 0, EnyoDurations.ScrimFade, EnyoEasing.CssEase)], cancellationToken);
    }

    /// <summary>Fades a scrim out to 0, then hides it. 500 ms, CSS ease.</summary>
    public static async Task<bool> ScrimHide(Control scrim, CancellationToken cancellationToken = default)
    {
        var done = await AnimationRunner.Run(scrim, [AnimationRunner.Opacity(scrim.Opacity, 0, 0, EnyoDurations.ScrimFade, EnyoEasing.CssEase)], cancellationToken).ConfigureAwait(true);
        if (done)
        {
            scrim.IsVisible = false;
        }

        return done;
    }
}
