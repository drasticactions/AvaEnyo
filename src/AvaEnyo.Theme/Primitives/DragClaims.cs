using System.Runtime.CompilerServices;
using Avalonia.Input;

namespace AvaEnyo.Primitives;

/// <summary>
/// Which <see cref="DragTracker"/> owns a pointer. Pointer events bubble from the innermost element, so an inner
/// tracker claims first; an outer tracker sees the claim and stays out. A vertical scroller inside a horizontal
/// SlidingPane works this way: each takes only the drags on its own axis.
/// </summary>
internal static class DragClaims
{
    private static readonly ConditionalWeakTable<IPointer, DragTracker> s_claims = new();

    public static DragTracker? OwnerOf(IPointer pointer) => s_claims.TryGetValue(pointer, out var t) ? t : null;

    public static bool Claim(IPointer pointer, DragTracker tracker)
    {
        if (OwnerOf(pointer) is { } owner && owner != tracker)
        {
            return false;
        }

        s_claims.AddOrUpdate(pointer, tracker);
        return true;
    }

    public static void Release(IPointer pointer, DragTracker tracker)
    {
        if (OwnerOf(pointer) == tracker)
        {
            s_claims.Remove(pointer);
        }
    }
}
