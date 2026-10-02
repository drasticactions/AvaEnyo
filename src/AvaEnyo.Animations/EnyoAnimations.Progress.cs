using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.Primitives;

namespace AvaEnyo.Animations;

public static partial class EnyoAnimations
{
    /// <summary>
    /// Animates a range control (progress bar, slider) to <paramref name="value"/>. 350 ms, cubic ease-out, as
    /// Enyo's animated progress and slider tap.
    /// </summary>
    public static Task<bool> ProgressTo(RangeBase range, double value, double? durationMs = null, CancellationToken cancellationToken = default)
    {
        var from = range.Value;
        return Animate(from, value, System.TimeSpan.FromMilliseconds(durationMs ?? EnyoDurations.Progress), EnyoEasing.CubicOut, v => range.SetCurrentValue(RangeBase.ValueProperty, v), range, cancellationToken);
    }
}
