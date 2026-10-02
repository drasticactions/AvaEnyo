using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;

namespace AvaEnyo.Animations;

/// <summary>Enyo's <c>transitions.Simple</c>: hides the outgoing view and shows the incoming one at once.</summary>
public sealed class EnyoNoTransition : IPageTransition
{
    /// <inheritdoc/>
    public Task Start(Visual? from, Visual? to, bool forward, CancellationToken cancellationToken)
    {
        if (from is not null)
        {
            from.IsVisible = false;
        }

        if (to is not null)
        {
            to.IsVisible = true;
        }

        return Task.CompletedTask;
    }
}
