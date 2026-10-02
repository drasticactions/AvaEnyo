using System;
using Avalonia.Animation.Easings;

namespace AvaEnyo.Animations;

/// <summary>
/// An Avalonia <see cref="Easing"/> over a pure function of progress. The <see cref="EnyoEasing"/> curves use it.
/// </summary>
public sealed class EnyoEasingFunction : Easing
{
    private readonly Func<double, double> _function;

    /// <summary>Initializes a new instance of the <see cref="EnyoEasingFunction"/> class.</summary>
    /// <param name="name">The Enyo name of the curve, for diagnostics.</param>
    /// <param name="function">The curve. It maps progress 0..1 to eased progress.</param>
    public EnyoEasingFunction(string name, Func<double, double> function)
    {
        Name = name;
        _function = function ?? throw new ArgumentNullException(nameof(function));
    }

    /// <summary>The Enyo name of the curve, for example <c>cubicOut</c>.</summary>
    public string Name { get; }

    /// <summary>The pure function behind the curve.</summary>
    public Func<double, double> Function => _function;

    /// <inheritdoc/>
    public override double Ease(double progress) => _function(progress);

    /// <inheritdoc/>
    public override string ToString() => Name;
}
