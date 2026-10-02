using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace AvaEnyo.Controls;

/// <summary>
/// A fixed-size list whose items are loaded a page at a time by an async loader. Items of pages that are not loaded
/// read as <c>default</c>. A <see cref="VirtualList"/> bound to it loads the pages near the visible rows and
/// releases the rest, as Enyo's buffered lists did.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
public sealed class PagedCollection<T> : IList<T?>, IReadOnlyList<T?>, IList, INotifyCollectionChanged, INotifyPropertyChanged, IPagedItemsSource
{
    private readonly T?[] _items;
    private readonly Func<int, CancellationToken, Task<IReadOnlyList<T>>> _loader;
    private readonly Dictionary<int, CancellationTokenSource> _loading = new();
    private readonly HashSet<int> _loaded = new();

    /// <summary>Initializes a new instance of the <see cref="PagedCollection{T}"/> class.</summary>
    /// <param name="count">The total number of items.</param>
    /// <param name="loader">Loads page <c>n</c>: up to <paramref name="pageSize"/> items starting at <c>n × pageSize</c>.</param>
    /// <param name="pageSize">The items per page. Default 10 (Enyo's page size).</param>
    public PagedCollection(int count, Func<int, CancellationToken, Task<IReadOnlyList<T>>> loader, int pageSize = 10)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        _items = new T?[count];
        _loader = loader ?? throw new ArgumentNullException(nameof(loader));
        PageSize = Math.Max(1, pageSize);
    }

    /// <inheritdoc/>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public int PageSize { get; }

    /// <summary>The number of pages.</summary>
    public int PageCount => (_items.Length + PageSize - 1) / PageSize;

    /// <summary>The pages whose items are loaded.</summary>
    public IReadOnlyCollection<int> LoadedPages => _loaded;

    /// <summary>The pages being loaded or loaded.</summary>
    public IReadOnlyCollection<int> RequestedPages
    {
        get
        {
            var set = new HashSet<int>(_loaded);
            set.UnionWith(_loading.Keys);
            return set;
        }
    }

    /// <inheritdoc/>
    public int Count => _items.Length;

    /// <inheritdoc/>
    public bool IsReadOnly => true;

    bool IList.IsFixedSize => true;

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => this;

    /// <inheritdoc/>
    public T? this[int index]
    {
        get => _items[index];
        set => throw new NotSupportedException();
    }

    object? IList.this[int index]
    {
        get => _items[index];
        set => throw new NotSupportedException();
    }

    /// <inheritdoc/>
    public bool AcquirePage(int page)
    {
        if (page < 0 || page >= PageCount)
        {
            return false;
        }

        if (_loaded.Contains(page) || _loading.ContainsKey(page))
        {
            return true;
        }

        var cts = new CancellationTokenSource();
        _loading[page] = cts;
        _ = LoadAsync(page, cts);
        return true;
    }

    /// <inheritdoc/>
    public void DiscardPage(int page)
    {
        if (_loading.Remove(page, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }

        if (_loaded.Remove(page))
        {
            var start = page * PageSize;
            var end = Math.Min(_items.Length, start + PageSize);
            for (var i = start; i < end; i++)
            {
                Replace(i, default);
            }
        }
    }

    private async Task LoadAsync(int page, CancellationTokenSource cts)
    {
        IReadOnlyList<T> result;
        try
        {
            result = await _loader(page, cts.Token).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        if (!_loading.TryGetValue(page, out var current) || current != cts)
        {
            return;
        }

        _loading.Remove(page);
        cts.Dispose();
        _loaded.Add(page);
        var start = page * PageSize;
        for (var i = 0; i < result.Count && start + i < _items.Length; i++)
        {
            Replace(start + i, result[i]);
        }
    }

    private void Replace(int index, T? value)
    {
        var old = _items[index];
        _items[index] = value;
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, old, index));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
    }

    /// <inheritdoc/>
    public IEnumerator<T?> GetEnumerator() => ((IEnumerable<T?>)_items).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

    /// <inheritdoc/>
    public int IndexOf(T? item) => Array.IndexOf(_items, item);

    /// <inheritdoc/>
    public bool Contains(T? item) => IndexOf(item) >= 0;

    /// <inheritdoc/>
    public void CopyTo(T?[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

    void ICollection.CopyTo(Array array, int index) => _items.CopyTo(array, index);

    int IList.IndexOf(object? value) => value is T t ? IndexOf(t) : -1;

    bool IList.Contains(object? value) => value is T t && Contains(t);

    void ICollection<T?>.Add(T? item) => throw new NotSupportedException();

    void ICollection<T?>.Clear() => throw new NotSupportedException();

    bool ICollection<T?>.Remove(T? item) => throw new NotSupportedException();

    void IList<T?>.Insert(int index, T? item) => throw new NotSupportedException();

    void IList<T?>.RemoveAt(int index) => throw new NotSupportedException();

    int IList.Add(object? value) => throw new NotSupportedException();

    void IList.Clear() => throw new NotSupportedException();

    void IList.Insert(int index, object? value) => throw new NotSupportedException();

    void IList.Remove(object? value) => throw new NotSupportedException();

    void IList.RemoveAt(int index) => throw new NotSupportedException();
}
