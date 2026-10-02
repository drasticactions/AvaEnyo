using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace AvaEnyo.Animations;

public static partial class EnyoAnimations
{
    /// <summary>
    /// Opens a drawer: animates <see cref="Layoutable.Height"/> from 0 to the natural height of the control, then
    /// clears it so the control sizes itself again. 250 ms, cubic ease-out.
    /// </summary>
    public static async Task<bool> ExpandHeight(Control control, double? durationMs = null, CancellationToken cancellationToken = default)
    {
        control.IsVisible = true;
        var from = double.IsNaN(control.Height) ? (control.IsArrangeValid ? control.Bounds.Height : 0) : control.Height;
        var target = NaturalHeight(control);
        control.ClipToBounds = true;
        var done = await AnimationRunner.Run(control, [AnimationRunner.Double(Layoutable.HeightProperty, from, target, 0, durationMs ?? EnyoDurations.DrawerOpen, EnyoEasing.CubicOut)], cancellationToken).ConfigureAwait(true);
        if (done)
        {
            control.ClearValue(Layoutable.HeightProperty);
            control.ClearValue(Visual.ClipToBoundsProperty);
        }

        return done;
    }

    /// <summary>
    /// Closes a drawer: animates <see cref="Layoutable.Height"/> from the current height to 0, then hides the control.
    /// 100 ms, cubic ease-out.
    /// </summary>
    public static async Task<bool> CollapseHeight(Control control, double? durationMs = null, CancellationToken cancellationToken = default)
    {
        var from = double.IsNaN(control.Height) ? control.Bounds.Height : control.Height;
        control.ClipToBounds = true;
        var done = await AnimationRunner.Run(control, [AnimationRunner.Double(Layoutable.HeightProperty, from, 0d, 0, durationMs ?? EnyoDurations.DrawerClose, EnyoEasing.CubicOut)], cancellationToken).ConfigureAwait(true);
        if (done)
        {
            control.IsVisible = false;
            control.ClearValue(Layoutable.HeightProperty);
            control.ClearValue(Visual.ClipToBoundsProperty);
        }

        return done;
    }

    /// <summary>The height the control wants at its current width when nothing constrains it.</summary>
    internal static double NaturalHeight(Control control)
    {
        var width = control.Bounds.Width > 0 ? control.Bounds.Width : double.PositiveInfinity;
        if (control.GetVisualParentLayoutable() is { } parent && parent.Bounds.Width > 0 && double.IsInfinity(width))
        {
            width = parent.Bounds.Width;
        }

        var height = control.Height;
        control.ClearValue(Layoutable.HeightProperty);
        control.Measure(new Size(width, double.PositiveInfinity));
        var desired = control.DesiredSize.Height;
        if (!double.IsNaN(height))
        {
            control.Height = height;
        }

        return desired;
    }

    private static Layoutable? GetVisualParentLayoutable(this Control control) => control.Parent as Layoutable;
}
