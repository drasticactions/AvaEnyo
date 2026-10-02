using System;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media.Transformation;
using Avalonia.Styling;

namespace AvaEnyo.Animations;

public static partial class EnyoAnimations
{
    /// <summary>
    /// Rotates a control forever: one turn per second, linear (Onyx's spinner). Dispose the result to stop. When
    /// animations are off the control stays still.
    /// </summary>
    public static IDisposable Spin(Control control, double? periodMs = null)
    {
        AnimationRunner.EnsureInitialized();
        var rotate = TransformOperations.Parse("rotate(0deg)");
        control.RenderTransformOrigin = RelativePoint.Center;
        control.RenderTransform = rotate;
        var cts = new CancellationTokenSource();
        if (IsActive)
        {
            var animation = new Animation
            {
                Duration = TimeSpan.FromMilliseconds((periodMs ?? EnyoDurations.Spinner) * TimeScale),
                IterationCount = IterationCount.Infinite,
                Easing = new Avalonia.Animation.Easings.LinearEasing(),
                Children =
                {
                    // A RotateTransform is not a Visual and cannot be the target of a transform animation, so the
                    // control's RenderTransform is animated with the theme's transform-operations animator.
                    new KeyFrame { Cue = new Cue(0), Setters = { new Setter(Visual.RenderTransformProperty, TransformOperations.Parse("rotate(0deg)")) } },
                    new KeyFrame { Cue = new Cue(1), Setters = { new Setter(Visual.RenderTransformProperty, TransformOperations.Parse("rotate(360deg)")) } },
                },
            };
            _ = animation.RunAsync(control, cts.Token);
        }

        return new SpinHandle(control, rotate, cts);
    }
}
