using System;

namespace AvaEnyo.Animations;

/// <summary>
/// The easing curves of Enyo 1.0 (<c>enyo.easing</c>). Each curve exists as an Avalonia easing and as a pure
/// function, so physics code and tests can use it without Avalonia.
/// </summary>
public static class EnyoEasing
{
    /// <summary><c>linear</c>: n.</summary>
    public static readonly Func<double, double> LinearFunction = n => n;

    /// <summary><c>cubicIn</c>: n³.</summary>
    public static readonly Func<double, double> CubicInFunction = n => n * n * n;

    /// <summary><c>cubicOut</c>: (n − 1)³ + 1. Most Enyo motion uses it.</summary>
    public static readonly Func<double, double> CubicOutFunction = n =>
    {
        var m = n - 1;
        return (m * m * m) + 1;
    };

    /// <summary><c>expoOut</c>: 1 at n = 1, else 1 − 2^(−10n).</summary>
    public static readonly Func<double, double> ExpoOutFunction = n => n >= 1 ? 1 : 1 - Math.Pow(2, -10 * n);

    /// <summary><c>quadInOut</c>: (2n)²/2 for the first half, −((2n−1)(2n−3) − 1)/2 for the second.</summary>
    public static readonly Func<double, double> QuadInOutFunction = n =>
    {
        var m = n * 2;
        if (m < 1)
        {
            return m * m / 2;
        }

        m -= 1;
        return -((m * (m - 2)) - 1) / 2;
    };

    /// <summary>
    /// CSS <c>ease</c>, <c>cubic-bezier(0.25, 0.1, 0.25, 1)</c>: the timing of Onyx's CSS opacity transitions
    /// (scrim, ModalDialog), which name no timing function.
    /// </summary>
    public static readonly Avalonia.Animation.Easings.Easing CssEase = new Avalonia.Animation.Easings.SplineEasing(0.25, 0.1, 0.25, 1.0);

    /// <summary>The <c>linear</c> curve as an Avalonia easing.</summary>
    public static readonly EnyoEasingFunction Linear = new("linear", LinearFunction);

    /// <summary>The <c>cubicIn</c> curve as an Avalonia easing.</summary>
    public static readonly EnyoEasingFunction CubicIn = new("cubicIn", CubicInFunction);

    /// <summary>The <c>cubicOut</c> curve as an Avalonia easing.</summary>
    public static readonly EnyoEasingFunction CubicOut = new("cubicOut", CubicOutFunction);

    /// <summary>The <c>expoOut</c> curve as an Avalonia easing.</summary>
    public static readonly EnyoEasingFunction ExpoOut = new("expoOut", ExpoOutFunction);

    /// <summary>The <c>quadInOut</c> curve as an Avalonia easing.</summary>
    public static readonly EnyoEasingFunction QuadInOut = new("quadInOut", QuadInOutFunction);
}
