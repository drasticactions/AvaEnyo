using System;
using Avalonia;

namespace AvaEnyo.Controls;

/// <summary>A <see cref="Picker"/> of the integers from <see cref="Minimum"/> to <see cref="Maximum"/> (<c>enyo.IntegerPicker</c>).</summary>
public class IntegerPicker : Picker
{
    /// <summary>Defines the <see cref="Minimum"/> property.</summary>
    public static readonly StyledProperty<int> MinimumProperty =
        AvaloniaProperty.Register<IntegerPicker, int>(nameof(Minimum));

    /// <summary>Defines the <see cref="Maximum"/> property.</summary>
    public static readonly StyledProperty<int> MaximumProperty =
        AvaloniaProperty.Register<IntegerPicker, int>(nameof(Maximum), 9);

    static IntegerPicker()
    {
        MinimumProperty.Changed.AddClassHandler<IntegerPicker>((p, _) => p.Rebuild());
        MaximumProperty.Changed.AddClassHandler<IntegerPicker>((p, _) => p.Rebuild());
    }

    /// <summary>Initializes a new instance of the <see cref="IntegerPicker"/> class.</summary>
    public IntegerPicker()
    {
        Rebuild();
    }

    /// <summary>The smallest value. Default 0.</summary>
    public int Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    /// <summary>The largest value. Default 9.</summary>
    public int Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    private void Rebuild()
    {
        var max = Math.Max(Minimum, Maximum);
        var current = Value is int v ? Math.Clamp(v, Minimum, max) : Minimum;
        Items.Clear();
        for (var i = Minimum; i <= max; i++)
        {
            Items.Add(i);
        }

        SetCurrentValue(ValueProperty, current);
    }
}
