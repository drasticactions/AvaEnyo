using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace AvaEnyo.Animations;

public static partial class EnyoAnimations
{
    /// <summary>
    /// Slides a toaster in from <paramref name="edge"/> by <paramref name="distance"/> (its own size along that
    /// axis). 350 ms, cubic ease-out.
    /// </summary>
    public static Task<bool> ToasterIn(Control control, ToasterEdge edge, double distance, CancellationToken cancellationToken = default)
    {
        var (x, y) = Offset(edge, distance);
        var from = control.RenderTransform is null ? AnimationRunner.Translate(x, y) : AnimationRunner.Current(control);
        return AnimationRunner.Run(control, [AnimationRunner.Transform(from, AnimationRunner.Identity, 0, EnyoDurations.Toaster, EnyoEasing.CubicOut)], cancellationToken);
    }

    /// <summary>Slides a toaster out toward <paramref name="edge"/> by <paramref name="distance"/>. 350 ms, cubic ease-out.</summary>
    public static Task<bool> ToasterOut(Control control, ToasterEdge edge, double distance, CancellationToken cancellationToken = default)
    {
        var (x, y) = Offset(edge, distance);
        return AnimationRunner.Run(control, [AnimationRunner.Transform(AnimationRunner.Current(control), AnimationRunner.Translate(x, y), 0, EnyoDurations.Toaster, EnyoEasing.CubicOut)], cancellationToken);
    }

    private static (double X, double Y) Offset(ToasterEdge edge, double distance) => edge switch
    {
        ToasterEdge.Top => (0, -distance),
        ToasterEdge.Left => (-distance, 0),
        ToasterEdge.Right => (distance, 0),
        _ => (0, distance),
    };
}
