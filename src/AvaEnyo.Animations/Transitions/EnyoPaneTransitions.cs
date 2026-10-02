using Avalonia.Animation;

namespace AvaEnyo.Animations;

/// <summary>The view transitions of the Enyo Pane, usable by any <see cref="IPageTransition"/> host.</summary>
public static class EnyoPaneTransitions
{
    /// <summary>The Pane default: a 300 ms cubic ease-out fade.</summary>
    public static IPageTransition Fade { get; } = new EnyoFadeTransition();

    /// <summary>A 300 ms cubic ease-out slide in from the right.</summary>
    public static IPageTransition FlyIn { get; } = new EnyoFlyInTransition();

    /// <summary>No animation.</summary>
    public static IPageTransition None { get; } = new EnyoNoTransition();
}
