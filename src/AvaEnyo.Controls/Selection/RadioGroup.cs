using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using AvaEnyo.Controls.Primitives;

namespace AvaEnyo.Controls;

/// <summary>
/// A segmented row of choices (<c>enyo.RadioGroup</c>). Items become <see cref="RadioSegment"/>s in an
/// <see cref="OrderedPanel"/>. <see cref="Value"/> is the selected segment's <see cref="RadioSegment.Value"/>, or its
/// index when the segment has none. The <c>dark</c> class is for dark backgrounds.
/// </summary>
public class RadioGroup : SelectingItemsControl
{
    /// <summary>Defines the <see cref="Value"/> property.</summary>
    public static readonly StyledProperty<object?> ValueProperty =
        AvaloniaProperty.Register<RadioGroup, object?>(nameof(Value), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Defines the <see cref="ValueChanged"/> event.</summary>
    public static readonly RoutedEvent<RadioGroupValueChangedEventArgs> ValueChangedEvent =
        RoutedEvent.Register<RadioGroup, RadioGroupValueChangedEventArgs>(nameof(ValueChanged), RoutingStrategies.Bubble);

    private static readonly FuncTemplate<Panel?> s_panel = new(() => new OrderedPanel { Orientation = Avalonia.Layout.Orientation.Horizontal });
    private bool _syncing;

    static RadioGroup()
    {
        ItemsPanelProperty.OverrideDefaultValue<RadioGroup>(s_panel);
        SelectionModeProperty.OverrideDefaultValue<RadioGroup>(SelectionMode.Single | SelectionMode.AlwaysSelected);

        // A segmented row has no scroll of its own; bringing the selected segment into view would scroll the page
        // that contains the group instead.
        AutoScrollToSelectedItemProperty.OverrideDefaultValue<RadioGroup>(false);
        ValueProperty.Changed.AddClassHandler<RadioGroup>((g, e) => g.OnValueChanged(e));
        SelectedIndexProperty.Changed.AddClassHandler<RadioGroup>((g, _) => g.SyncValueFromSelection());
    }

    /// <summary>The selected value: the segment's <see cref="RadioSegment.Value"/>, or its index.</summary>
    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>Raised when <see cref="Value"/> changes.</summary>
    public event EventHandler<RadioGroupValueChangedEventArgs>? ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) =>
        NeedsContainer<RadioSegment>(item, out recycleKey);

    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => new RadioSegment();

    /// <inheritdoc/>
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is RadioSegment segment)
        {
            // Segments of one group exclude each other through the selection, not through RadioButton group names.
            segment.GroupName = "enyo-radiogroup-" + GetHashCode();
            if (Classes.Contains("dark"))
            {
                segment.Classes.Add("dark");
            }
        }
    }

    /// <summary>The value of the segment at <paramref name="index"/>.</summary>
    protected object? ValueAt(int index)
    {
        if (index < 0)
        {
            return null;
        }

        return (ContainerFromIndex(index) as RadioSegment)?.Value ?? (Items.Count > index && Items[index] is RadioSegment s && s.Value is not null ? s.Value : index);
    }

    private void SyncValueFromSelection()
    {
        if (_syncing)
        {
            return;
        }

        _syncing = true;
        try
        {
            SetCurrentValue(ValueProperty, ValueAt(SelectedIndex));
        }
        finally
        {
            _syncing = false;
        }
    }

    private void OnValueChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (!_syncing)
        {
            _syncing = true;
            try
            {
                for (var i = 0; i < ItemCount; i++)
                {
                    if (Equals(ValueAt(i), e.NewValue))
                    {
                        SelectedIndex = i;
                        break;
                    }
                }
            }
            finally
            {
                _syncing = false;
            }
        }

        RaiseEvent(new RadioGroupValueChangedEventArgs(ValueChangedEvent, this, e.OldValue, e.NewValue));
    }
}
