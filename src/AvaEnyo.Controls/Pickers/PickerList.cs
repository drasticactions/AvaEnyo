using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

namespace AvaEnyo.Controls;

/// <summary>
/// The list a <see cref="Picker"/> shows in its popup (Enyo's <c>PopupList</c>): one <see cref="PickerItem"/> per item,
/// the selected one bold on white. A tap on a row selects it and raises <see cref="Picked"/>.
/// </summary>
public class PickerList : SelectingItemsControl
{
    /// <summary>Defines the <see cref="TextAlign"/> property.</summary>
    public static readonly StyledProperty<TextAlignment> TextAlignProperty =
        Picker.TextAlignProperty.AddOwner<PickerList>();

    /// <summary>Defines the <see cref="Picked"/> event.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> PickedEvent =
        RoutedEvent.Register<PickerList, RoutedEventArgs>(nameof(Picked), RoutingStrategies.Bubble);

    static PickerList()
    {
        Item.ClickEvent.AddClassHandler<PickerList>((l, e) => l.OnRowClick(e));
    }

    /// <summary>The alignment of the rows' text.</summary>
    public TextAlignment TextAlign
    {
        get => GetValue(TextAlignProperty);
        set => SetValue(TextAlignProperty, value);
    }

    /// <summary>Raised when a row is tapped.</summary>
    public event EventHandler<RoutedEventArgs>? Picked
    {
        add => AddHandler(PickedEvent, value);
        remove => RemoveHandler(PickedEvent, value);
    }

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) =>
        NeedsContainer<PickerItem>(item, out recycleKey);

    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => new PickerItem();

    /// <inheritdoc/>
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is ContentControl c)
        {
            c.HorizontalContentAlignment = TextAlign switch
            {
                TextAlignment.Left or TextAlignment.Start => HorizontalAlignment.Left,
                TextAlignment.Right or TextAlignment.End => HorizontalAlignment.Right,
                _ => HorizontalAlignment.Center,
            };
        }
    }

    private void OnRowClick(RoutedEventArgs e)
    {
        if (e.Source is not Control c)
        {
            return;
        }

        var index = IndexFromContainer(c);
        if (index < 0)
        {
            return;
        }

        e.Handled = true;
        SelectedIndex = index;
        RaiseEvent(new RoutedEventArgs(PickedEvent, this));
    }
}
