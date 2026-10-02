using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace AvaEnyo.Animations;

/// <summary>
/// The Enyo Pane fly-in (<c>LeftRightFlyin</c>) as an <see cref="IPageTransition"/>: the incoming view slides in
/// from the right edge of its parent over the outgoing one in 300 ms with a cubic ease-out, then the outgoing view is
/// hidden. Going back, the outgoing view slides off to the right instead.
/// </summary>
public sealed class EnyoFlyInTransition : IPageTransition
{
    /// <inheritdoc/>
    public async Task Start(Visual? from, Visual? to, bool forward, CancellationToken cancellationToken)
    {
        if (to is not null)
        {
            to.IsVisible = true;
        }

        var width = (to ?? from)?.GetVisualParent()?.Bounds.Width ?? 0;
        if (width <= 0)
        {
            width = (to ?? from)?.Bounds.Width ?? 0;
        }

        await EnyoAnimations.FlyIn(from as Control, to as Control, width, forward, cancellationToken).ConfigureAwait(true);
        if (from is not null && !cancellationToken.IsCancellationRequested)
        {
            from.IsVisible = false;
            from.ClearValue(Visual.RenderTransformProperty);
        }
    }
}
