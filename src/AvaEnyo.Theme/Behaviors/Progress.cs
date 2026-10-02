using System.Runtime.CompilerServices;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using AvaEnyo.Animations;

namespace AvaEnyo;

/// <summary>
/// Attached properties for range controls. <c>enyo:Progress.Animate="True"</c> makes every value change of a
/// <see cref="ProgressBar"/> (or other <see cref="RangeBase"/>) animate over 350 ms with a cubic ease-out, as Enyo's
/// <c>animatePosition</c>. A change during an animation starts from the value on screen.
/// </summary>
public static class Progress
{
    private static readonly ConditionalWeakTable<RangeBase, State> s_states = new();

    /// <summary>Defines the <c>Animate</c> attached property.</summary>
    public static readonly AttachedProperty<bool> AnimateProperty =
        AvaloniaProperty.RegisterAttached<RangeBase, bool>("Animate", typeof(Progress));

    /// <summary>Gets <c>Animate</c>.</summary>
    public static bool GetAnimate(RangeBase element) => element.GetValue(AnimateProperty);

    /// <summary>Sets <c>Animate</c>.</summary>
    public static void SetAnimate(RangeBase element, bool value) => element.SetValue(AnimateProperty, value);

    internal static void Register()
    {
        RangeBase.ValueProperty.Changed.AddClassHandler<RangeBase>(OnValueChanged);
    }

    private static void OnValueChanged(RangeBase range, AvaloniaPropertyChangedEventArgs e)
    {
        if (!GetAnimate(range) || !EnyoAnimations.IsActive || !Avalonia.VisualTree.VisualExtensions.IsAttachedToVisualTree(range))
        {
            return;
        }

        var state = s_states.GetOrCreateValue(range);
        if (state.Writing)
        {
            return;
        }

        var target = (double)e.NewValue!;
        var from = state.Running ? state.Shown : (double)e.OldValue!;
        state.Cancel?.Cancel();
        var cts = new CancellationTokenSource();
        state.Cancel = cts;
        state.Running = true;
        Write(range, state, from);
        _ = Run(range, state, from, target, cts);
    }

    private static async System.Threading.Tasks.Task Run(RangeBase range, State state, double from, double target, CancellationTokenSource cts)
    {
        var done = await EnyoAnimations.Animate(from, target, System.TimeSpan.FromMilliseconds(EnyoDurations.Progress), EnyoEasing.CubicOut,
            v => Write(range, state, v), range, cts.Token).ConfigureAwait(true);
        if (done && state.Cancel == cts)
        {
            state.Running = false;
        }
    }

    private static void Write(RangeBase range, State state, double value)
    {
        state.Writing = true;
        try
        {
            state.Shown = value;
            range.SetCurrentValue(RangeBase.ValueProperty, value);
        }
        finally
        {
            state.Writing = false;
        }
    }

    private sealed class State
    {
        public bool Writing;
        public bool Running;
        public double Shown;
        public CancellationTokenSource? Cancel;
    }
}
