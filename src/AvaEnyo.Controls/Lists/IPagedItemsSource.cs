namespace AvaEnyo.Controls;

/// <summary>
/// An items source that loads its items in pages (Enyo's <c>Buffer</c> with <c>onAcquirePage</c> and
/// <c>onDiscardPage</c>). A <see cref="VirtualList"/> asks for the pages around the visible rows and releases the pages
/// that scroll far away. <see cref="PagedCollection{T}"/> is a ready-made implementation.
/// </summary>
public interface IPagedItemsSource
{
    /// <summary>The number of items per page. 0 or less uses the list's <see cref="VirtualList.PageSize"/>.</summary>
    int PageSize { get; }

    /// <summary>Loads <paramref name="page"/>. Returns false when the page does not exist (past the end).</summary>
    bool AcquirePage(int page);

    /// <summary>Releases <paramref name="page"/>; its items may go back to placeholders.</summary>
    void DiscardPage(int page);
}
