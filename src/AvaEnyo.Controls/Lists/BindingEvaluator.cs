using Avalonia;
using Avalonia.Data;

namespace AvaEnyo.Controls;

/// <summary>Reads a binding's value for an item, by binding a private property with the item as data context.</summary>
internal sealed class BindingEvaluator : StyledElement
{
    public static readonly StyledProperty<object?> ValueProperty =
        AvaloniaProperty.Register<BindingEvaluator, object?>("Value");

    private readonly BindingBase _binding;
    private bool _bound;

    public BindingEvaluator(BindingBase binding)
    {
        _binding = binding;
    }

    public object? Evaluate(object? item)
    {
        DataContext = item;
        if (!_bound)
        {
            Bind(ValueProperty, _binding);
            _bound = true;
        }

        return GetValue(ValueProperty);
    }
}
