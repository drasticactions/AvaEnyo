using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;

namespace AvaEnyo.Controls.Primitives;

/// <summary>
/// A virtualizing grid of equal cells, filled row by row. Only the rows inside the scroll viewport (plus one row on
/// each side) have containers; the others are recycled. The number of columns is <see cref="ItemsPerRow"/>, or as
/// many <see cref="ItemWidth"/> cells as fit. Cells are square unless <see cref="ItemHeight"/> is set.
/// </summary>
/// <remarks>Used by <see cref="VirtualList"/> with <c>Layout="Grid"</c>, as in Enyo's selectable virtual grid.</remarks>
public class VirtualizingUniformGridPanel : VirtualizingPanel
{
    /// <summary>Defines the <see cref="ItemsPerRow"/> property.</summary>
    public static readonly StyledProperty<int> ItemsPerRowProperty =
        AvaloniaProperty.Register<VirtualizingUniformGridPanel, int>(nameof(ItemsPerRow));

    /// <summary>Defines the <see cref="ItemWidth"/> property.</summary>
    public static readonly StyledProperty<double> ItemWidthProperty =
        AvaloniaProperty.Register<VirtualizingUniformGridPanel, double>(nameof(ItemWidth), double.NaN);

    /// <summary>Defines the <see cref="ItemHeight"/> property.</summary>
    public static readonly StyledProperty<double> ItemHeightProperty =
        AvaloniaProperty.Register<VirtualizingUniformGridPanel, double>(nameof(ItemHeight), double.NaN);

    private readonly Dictionary<int, Control> _realized = new();
    private readonly Dictionary<Control, object?> _recycleKeys = new();
    private readonly Dictionary<object, Stack<Control>> _pool = new();
    private Rect _viewport;
    private int _columns = 1;
    private double _cellWidth;
    private double _cellHeight;

    static VirtualizingUniformGridPanel()
    {
        AffectsMeasure<VirtualizingUniformGridPanel>(ItemsPerRowProperty, ItemWidthProperty, ItemHeightProperty);
    }

    /// <summary>Initializes a new instance of the <see cref="VirtualizingUniformGridPanel"/> class.</summary>
    public VirtualizingUniformGridPanel()
    {
        EffectiveViewportChanged += OnEffectiveViewportChanged;
    }

    /// <summary>The number of columns. 0 (default) derives it from <see cref="ItemWidth"/>.</summary>
    public int ItemsPerRow
    {
        get => GetValue(ItemsPerRowProperty);
        set => SetValue(ItemsPerRowProperty, value);
    }

    /// <summary>The cell width used when <see cref="ItemsPerRow"/> is 0. NaN means 100 px.</summary>
    public double ItemWidth
    {
        get => GetValue(ItemWidthProperty);
        set => SetValue(ItemWidthProperty, value);
    }

    /// <summary>The cell height. NaN (default) makes cells square.</summary>
    public double ItemHeight
    {
        get => GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    /// <summary>The number of columns of the last layout.</summary>
    public int Columns => _columns;

    /// <summary>The index of the first realized item, or -1.</summary>
    public int FirstRealizedIndex
    {
        get
        {
            var min = -1;
            foreach (var i in _realized.Keys)
            {
                if (min < 0 || i < min)
                {
                    min = i;
                }
            }

            return min;
        }
    }

    /// <summary>The index of the last realized item, or -1.</summary>
    public int LastRealizedIndex
    {
        get
        {
            var max = -1;
            foreach (var i in _realized.Keys)
            {
                max = Math.Max(max, i);
            }

            return max;
        }
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        var items = Items;
        var width = double.IsInfinity(availableSize.Width) ? Math.Max(_viewport.Width, 1) : availableSize.Width;
        _columns = ItemsPerRow > 0 ? ItemsPerRow : Math.Max(1, (int)Math.Floor(width / (double.IsNaN(ItemWidth) ? 100 : ItemWidth)));
        _cellWidth = ItemsPerRow > 0 || double.IsNaN(ItemWidth) ? width / _columns : ItemWidth;
        _cellHeight = double.IsNaN(ItemHeight) ? _cellWidth : ItemHeight;
        var count = items.Count;
        var rows = (count + _columns - 1) / _columns;

        var viewport = _viewport;
        if (viewport.Height <= 0)
        {
            viewport = new Rect(0, 0, width, double.IsInfinity(availableSize.Height) ? _cellHeight * 4 : availableSize.Height);
        }

        var firstRow = Math.Max(0, (int)Math.Floor(viewport.Top / Math.Max(1, _cellHeight)) - 1);
        var lastRow = Math.Min(rows - 1, (int)Math.Ceiling(viewport.Bottom / Math.Max(1, _cellHeight)));
        var first = firstRow * _columns;
        var last = Math.Min(count - 1, ((lastRow + 1) * _columns) - 1);

        var stale = new List<int>();
        foreach (var index in _realized.Keys)
        {
            if (index < first || index > last || index >= count)
            {
                stale.Add(index);
            }
        }

        foreach (var index in stale)
        {
            Recycle(index);
        }

        for (var i = first; i <= last && i < count; i++)
        {
            var element = GetOrCreate(items, i);
            element.Measure(new Size(_cellWidth, _cellHeight));
        }

        return new Size(width, rows * _cellHeight);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (var (index, element) in _realized)
        {
            var row = index / _columns;
            var col = index % _columns;
            element.Arrange(new Rect(col * _cellWidth, row * _cellHeight, _cellWidth, _cellHeight));
        }

        return finalSize;
    }

    /// <inheritdoc/>
    protected override void OnItemsChanged(IReadOnlyList<object?> items, NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(items, e);
        foreach (var index in new List<int>(_realized.Keys))
        {
            Recycle(index, itemRemoved: true);
        }

        InvalidateMeasure();
    }

    /// <inheritdoc/>
    protected override Control? ScrollIntoView(int index)
    {
        if (index < 0 || index >= Items.Count)
        {
            return null;
        }

        var element = GetOrCreate(Items, index);
        element.Measure(new Size(_cellWidth, _cellHeight));
        var row = index / Math.Max(1, _columns);
        var col = index % Math.Max(1, _columns);
        element.Arrange(new Rect(col * _cellWidth, row * _cellHeight, _cellWidth, _cellHeight));
        element.BringIntoView();
        return element;
    }

    /// <inheritdoc/>
    protected override Control? ContainerFromIndex(int index) => _realized.TryGetValue(index, out var c) ? c : null;

    /// <inheritdoc/>
    protected override int IndexFromContainer(Control container)
    {
        foreach (var (index, element) in _realized)
        {
            if (element == container)
            {
                return index;
            }
        }

        return -1;
    }

    /// <inheritdoc/>
    protected override IEnumerable<Control>? GetRealizedContainers() => _realized.Values;

    /// <inheritdoc/>
    protected override IInputElement? GetControl(NavigationDirection direction, IInputElement? from, bool wrap)
    {
        var count = Items.Count;
        var index = from is Control c ? IndexFromContainer(c) : -1;
        var next = direction switch
        {
            NavigationDirection.Left or NavigationDirection.Previous => index - 1,
            NavigationDirection.Right or NavigationDirection.Next => index + 1,
            NavigationDirection.Up => index - _columns,
            NavigationDirection.Down => index + _columns,
            NavigationDirection.First => 0,
            NavigationDirection.Last => count - 1,
            _ => -1,
        };

        if (next < 0 || next >= count)
        {
            return null;
        }

        return ScrollIntoView(next);
    }

    private Control GetOrCreate(IReadOnlyList<object?> items, int index)
    {
        if (_realized.TryGetValue(index, out var existing))
        {
            return existing;
        }

        var generator = ItemContainerGenerator!;
        var item = items[index];
        Control element;
        if (generator.NeedsContainer(item, index, out var recycleKey))
        {
            if (recycleKey is not null && _pool.TryGetValue(recycleKey, out var pool) && pool.Count > 0)
            {
                element = pool.Pop();
                element.SetCurrentValue(IsVisibleProperty, true);
                generator.PrepareItemContainer(element, item, index);
                AddInternalChild(element);
                generator.ItemContainerPrepared(element, item, index);
            }
            else
            {
                element = generator.CreateContainer(item, index, recycleKey);
                generator.PrepareItemContainer(element, item, index);
                AddInternalChild(element);
                generator.ItemContainerPrepared(element, item, index);
            }

            _recycleKeys[element] = recycleKey;
        }
        else
        {
            element = (Control)item!;
            if (!_recycleKeys.ContainsKey(element))
            {
                generator.PrepareItemContainer(element, element, index);
                AddInternalChild(element);
                generator.ItemContainerPrepared(element, item, index);
                _recycleKeys[element] = s_ownContainer;
            }

            element.SetCurrentValue(IsVisibleProperty, true);
        }

        _realized[index] = element;
        return element;
    }

    private static readonly object s_ownContainer = new();

    private void Recycle(int index, bool itemRemoved = false)
    {
        if (!_realized.Remove(index, out var element))
        {
            return;
        }

        var generator = ItemContainerGenerator!;
        _recycleKeys.TryGetValue(element, out var key);
        if (key == s_ownContainer)
        {
            if (itemRemoved)
            {
                RemoveInternalChild(element);
                _recycleKeys.Remove(element);
            }
            else
            {
                element.SetCurrentValue(IsVisibleProperty, false);
            }

            return;
        }

        generator.ClearItemContainer(element);
        RemoveInternalChild(element);
        _recycleKeys.Remove(element);
        if (key is not null)
        {
            if (!_pool.TryGetValue(key, out var pool))
            {
                pool = new Stack<Control>();
                _pool[key] = pool;
            }

            element.SetCurrentValue(IsVisibleProperty, false);
            pool.Push(element);
        }
    }

    private void OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        var viewport = e.EffectiveViewport.Intersect(new Rect(Bounds.Size));
        if (viewport != _viewport)
        {
            var oldTop = (int)Math.Floor(_viewport.Top / Math.Max(1, _cellHeight));
            var oldBottom = (int)Math.Ceiling(_viewport.Bottom / Math.Max(1, _cellHeight));
            _viewport = viewport;
            var newTop = (int)Math.Floor(_viewport.Top / Math.Max(1, _cellHeight));
            var newBottom = (int)Math.Ceiling(_viewport.Bottom / Math.Max(1, _cellHeight));
            if (oldTop != newTop || oldBottom != newBottom || _realized.Count == 0)
            {
                InvalidateMeasure();
            }
        }
    }
}
