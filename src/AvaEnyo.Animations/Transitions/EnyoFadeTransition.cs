using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;

namespace AvaEnyo.Animations;

/// <summary>
/// The Enyo Pane fade as an <see cref="IPageTransition"/>: the incoming view fades in over the outgoing one in
/// 300 ms with a cubic ease-out, then the outgoing view is hidden.
/// </summary>
public sealed class EnyoFadeTransition : IPageTransition
{
    /// <inheritdoc/>
    public async Task Start(Visual? from, Visual? to, bool forward, CancellationToken cancellationToken)
    {
        if (to is not null)
        {
            to.IsVisible = true;
        }

        await EnyoAnimations.CrossFade(from as Control, to as Control, cancellationToken).ConfigureAwait(true);
        if (from is not null && !cancellationToken.IsCancellationRequested)
        {
            from.IsVisible = false;
        }
    }
}
