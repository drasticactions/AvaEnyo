using System;
using System.Collections;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Metadata;

namespace AvaEnyo.Controls;

/// <summary>
/// A list of choices in a menu-framed popup (the base of <see cref="PopupSelect"/> and <see cref="PopupList"/>).
/// Tapping a row selects it, raises <see cref="Selected"/>, and closes the popup with reason
/// <see cref="PopupCloseReason.Handle"/> and the item. The selected row is bold on white.
/// </summary>
public abstract class PopupListBase : PopupBase
{
    /// <summary>Defines the <see cref="ItemsSource"/> property.</summary>
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<PopupListBase, IEnumerable?>(nameof(ItemsSource));

    /// <summary>Defines the <see cref="ItemTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<PopupListBase, IDataTemplate?>(nameof(ItemTemplate));

    /// <summary>Defines the <see cref="SelectedIndex"/> property.</summary>
    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<PopupListBase, int>(nameof(SelectedIndex), -1, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Defines the <see cref="SelectedItem"/> property.</summary>
    public static readonly StyledProperty<object?> SelectedItemProperty =
        AvaloniaProperty.Register<PopupListBase, object?>(nameof(SelectedItem), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Defines the <see cref="Selected"/> event.</summary>
    public static readonly RoutedEvent<PopupSelectedEventArgs> SelectedEvent =
        RoutedEvent.Register<PopupListBase, PopupSelectedEventArgs>(nameof(Selected), RoutingStrategies.Bubble);

    private PopupListHost? _host;
    private bool _syncing;

    static PopupListBase()
    {
        ItemsSourceProperty.Changed.AddClassHandler<PopupListBase>((p, _) => p.SyncHost());
        ItemTemplateProperty.Changed.AddClassHandler<PopupListBase>((p, _) => p.SyncHost());
        SelectedIndexProperty.Changed.AddClassHandler<PopupListBase>((p, _) => p.OnSelectedIndexChanged());
        SelectedItemProperty.Changed.AddClassHandler<PopupListBase>((p, _) => p.OnSelectedItemChanged());
    }

    /// <summary>The choices.</summary>
    [Content]
    public AvaloniaList<object> Items { get; } = new();

    /// <summary>Choices from a collection; when set, used instead of <see cref="Items"/>.</summary>
    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>The template of a row's content.</summary>
    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>The index of the selected choice, or -1.</summary>
    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    /// <summary>The selected choice, or null.</summary>
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>Raised when the user picks a row.</summary>
    public event EventHandler<PopupSelectedEventArgs>? Selected
    {
        add => AddHandler(SelectedEvent, value);
        remove => RemoveHandler(SelectedEvent, value);
    }

    /// <summary>The items in use: <see cref="ItemsSource"/>, else <see cref="Items"/>.</summary>
    protected IEnumerable EffectiveItems => ItemsSource ?? Items;

    /// <summary>The panel the rows sit in.</summary>
    protected abstract ITemplate<Panel?> CreateItemsPanel();

    /// <summary>Picks the row at <paramref name="index"/>, as a tap does.</summary>
    public void Select(int index)
    {
        var previous = SelectedItem;
        SetCurrentValue(SelectedIndexProperty, index);
        var item = ItemAt(index);
        RaiseEvent(new PopupSelectedEventArgs(SelectedEvent, this, index, item, previous));
        CloseCore(PopupCloseReason.Handle, item);
    }

    /// <inheritdoc/>
    protected override PopupPresenter CreatePresenter()
    {
        SyncHost();
        Content = _host;
        return new PopupListPresenter();
    }

    internal object? ItemAt(int index)
    {
        if (index < 0)
        {
            return null;
        }

        var i = 0;
        foreach (var item in EffectiveItems)
        {
            if (i++ == index)
            {
                return item;
            }
        }

        return null;
    }

    private int IndexOf(object? value)
    {
        var i = 0;
        foreach (var item in EffectiveItems)
        {
            if (Equals(item, value))
            {
                return i;
            }

            i++;
        }

        return -1;
    }

    private void SyncHost()
    {
        _host ??= new PopupListHost { Owner = this, ItemsPanel = CreateItemsPanel() };
        _host.ItemsSource = EffectiveItems;
        _host.ItemTemplate = ItemTemplate;
    }

    private void OnSelectedIndexChanged()
    {
        if (_syncing)
        {
            return;
        }

        _syncing = true;
        try
        {
            SetCurrentValue(SelectedItemProperty, ItemAt(SelectedIndex));
        }
        finally
        {
            _syncing = false;
        }

        _host?.UpdateSelection(SelectedIndex);
    }

    private void OnSelectedItemChanged()
    {
        if (_syncing)
        {
            return;
        }

        _syncing = true;
        try
        {
            SetCurrentValue(SelectedIndexProperty, IndexOf(SelectedItem));
        }
        finally
        {
            _syncing = false;
        }

        _host?.UpdateSelection(SelectedIndex);
    }
}
