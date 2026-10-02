using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Interactivity;

namespace AvaEnyo.Controls;

public partial class VirtualList
{
    /// <summary>Defines the <see cref="PageSize"/> property.</summary>
    public static readonly StyledProperty<int> PageSizeProperty =
        AvaloniaProperty.Register<VirtualList, int>(nameof(PageSize), 10);

    /// <summary>Defines the <see cref="LookAhead"/> property.</summary>
    public static readonly StyledProperty<int> LookAheadProperty =
        AvaloniaProperty.Register<VirtualList, int>(nameof(LookAhead), 2);

    /// <summary>Defines the <see cref="AcquirePage"/> event.</summary>
    public static readonly RoutedEvent<PageEventArgs> AcquirePageEvent =
        RoutedEvent.Register<VirtualList, PageEventArgs>(nameof(AcquirePage), RoutingStrategies.Bubble);

    /// <summary>Defines the <see cref="DiscardPage"/> event.</summary>
    public static readonly RoutedEvent<PageEventArgs> DiscardPageEvent =
        RoutedEvent.Register<VirtualList, PageEventArgs>(nameof(DiscardPage), RoutingStrategies.Bubble);

    private readonly SortedSet<int> _pages = new();
    private int _endPage = int.MaxValue;

    /// <summary>
    /// Items per page when the source does not say: 10, Enyo's buffer page size.
    /// </summary>
    public int PageSize
    {
        get => GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    /// <summary>The pages kept loaded on each side of the visible rows: 2, as Enyo's buffer did.</summary>
    public int LookAhead
    {
        get => GetValue(LookAheadProperty);
        set => SetValue(LookAheadProperty, value);
    }

    /// <summary>The pages the list currently holds, in order.</summary>
    public IReadOnlyCollection<int> AcquiredPages => _pages;

    /// <summary>Raised when the list needs a page of a paged source (Enyo <c>onAcquirePage</c>).</summary>
    public event EventHandler<PageEventArgs>? AcquirePage
    {
        add => AddHandler(AcquirePageEvent, value);
        remove => RemoveHandler(AcquirePageEvent, value);
    }

    /// <summary>Raised when a page of a paged source scrolled far away (Enyo <c>onDiscardPage</c>).</summary>
    public event EventHandler<PageEventArgs>? DiscardPage
    {
        add => AddHandler(DiscardPageEvent, value);
        remove => RemoveHandler(DiscardPageEvent, value);
    }

    private IPagedItemsSource? PagedSource => ItemsSource as IPagedItemsSource;

    private int EffectivePageSize => PagedSource is { PageSize: > 0 } s ? s.PageSize : Math.Max(1, PageSize);

    private void InitializePaging()
    {
        LayoutUpdated += (_, _) => UpdatePages();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ItemsSourceProperty)
        {
            if (change.OldValue is IPagedItemsSource old)
            {
                foreach (var page in _pages)
                {
                    old.DiscardPage(page);
                }
            }

            _pages.Clear();
            _endPage = int.MaxValue;
        }
    }

    /// <summary>Acquires the pages around the realized rows and discards the rest.</summary>
    private void UpdatePages()
    {
        if (PagedSource is not { } source)
        {
            return;
        }

        int min = int.MaxValue, max = -1;
        foreach (var container in GetRealizedContainers())
        {
            if (!container.IsVisible)
            {
                continue;
            }

            var index = IndexFromContainer(container);
            if (index >= 0)
            {
                min = Math.Min(min, index);
                max = Math.Max(max, index);
            }
        }

        if (max < 0)
        {
            if (ItemCount > 0 || _pages.Count > 0)
            {
                return;
            }

            min = max = 0;
        }

        var size = EffectivePageSize;
        var first = Math.Max(0, (min / size) - Math.Max(0, LookAhead));
        var last = (max / size) + Math.Max(0, LookAhead);

        foreach (var page in new List<int>(_pages))
        {
            if (page < first || page > last)
            {
                _pages.Remove(page);
                source.DiscardPage(page);
                RaiseEvent(new PageEventArgs(DiscardPageEvent, this, page));
            }
        }

        for (var page = first; page <= last && page < _endPage; page++)
        {
            if (_pages.Contains(page))
            {
                continue;
            }

            var args = new PageEventArgs(AcquirePageEvent, this, page);
            var exists = source.AcquirePage(page);
            args.Exists = exists;
            RaiseEvent(args);
            if (!exists || !args.Exists)
            {
                _endPage = page;
                break;
            }

            _pages.Add(page);
        }
    }

    /// <summary>Asks the source for every held page again (Enyo's buffer <c>refresh</c>).</summary>
    private void RefreshPages()
    {
        if (PagedSource is not { } source)
        {
            return;
        }

        foreach (var page in _pages)
        {
            source.AcquirePage(page);
            RaiseEvent(new PageEventArgs(AcquirePageEvent, this, page));
        }
    }

    private void ResetPages()
    {
        if (PagedSource is { } source)
        {
            foreach (var page in _pages)
            {
                source.DiscardPage(page);
                RaiseEvent(new PageEventArgs(DiscardPageEvent, this, page));
            }
        }

        _pages.Clear();
        _endPage = int.MaxValue;
    }
}
