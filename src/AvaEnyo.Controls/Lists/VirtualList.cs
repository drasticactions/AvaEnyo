using System;
using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AvaEnyo.Controls.Primitives;

namespace AvaEnyo.Controls;

/// <summary>
/// The Enyo virtual list (<c>enyo.VirtualList</c>): a virtualizing, selectable list of <see cref="Item"/> rows that
/// scrolls with Enyo's physics. Rows are created only for the visible part and recycled as they scroll out.
/// </summary>
/// <remarks>
/// <para>Sections: with <see cref="DividerKeySelector"/> or <see cref="DividerMemberBinding"/>, a row whose key differs
/// from the previous item's shows a divider above itself, as Enyo apps did in <c>onSetupRow</c>; the dividers stay
/// right as rows are recycled.</para>
/// <para>Paging: when <see cref="ItemsControl.ItemsSource"/> is an <see cref="IPagedItemsSource"/>, the list asks for
/// the pages of the visible rows plus <see cref="LookAhead"/> pages on each side (<see cref="AcquirePage"/>) and
/// releases the others (<see cref="DiscardPage"/>).</para>
/// <para>With <see cref="Swipeable"/> the rows are <see cref="SwipeableItem"/>s, and only one shows its confirm prompt at
/// a time. <see cref="CanReorder"/> adds drag reordering after a hold, an AvaEnyo extension.</para>
/// </remarks>
[TemplatePart("PART_ScrollViewer", typeof(ScrollViewer), IsRequired = true)]
[TemplatePart("PART_ItemsPresenter", typeof(Avalonia.Controls.Presenters.ItemsPresenter), IsRequired = true)]
[PseudoClasses(":grid", ":reordering")]
public partial class VirtualList : SelectingItemsControl
{
    /// <summary>Defines the <see cref="SelectionMode"/> property.</summary>
    public static readonly new StyledProperty<SelectionMode> SelectionModeProperty = SelectingItemsControl.SelectionModeProperty;

    /// <summary>Defines the <see cref="SelectedItems"/> property.</summary>
    public static readonly new DirectProperty<SelectingItemsControl, IList?> SelectedItemsProperty = SelectingItemsControl.SelectedItemsProperty;

    /// <summary>Defines the <see cref="Layout"/> property.</summary>
    public static readonly StyledProperty<VirtualListLayout> LayoutProperty =
        AvaloniaProperty.Register<VirtualList, VirtualListLayout>(nameof(Layout));

    /// <summary>Defines the <see cref="ItemsPerRow"/> property.</summary>
    public static readonly StyledProperty<int> ItemsPerRowProperty =
        VirtualizingUniformGridPanel.ItemsPerRowProperty.AddOwner<VirtualList>();

    /// <summary>Defines the <see cref="ItemWidth"/> property.</summary>
    public static readonly StyledProperty<double> ItemWidthProperty =
        VirtualizingUniformGridPanel.ItemWidthProperty.AddOwner<VirtualList>();

    /// <summary>Defines the <see cref="ItemHeight"/> property.</summary>
    public static readonly StyledProperty<double> ItemHeightProperty =
        VirtualizingUniformGridPanel.ItemHeightProperty.AddOwner<VirtualList>();

    /// <summary>Defines the <see cref="Swipeable"/> property.</summary>
    public static readonly StyledProperty<bool> SwipeableProperty =
        AvaloniaProperty.Register<VirtualList, bool>(nameof(Swipeable));

    /// <summary>Defines the <see cref="DividerKeySelector"/> property.</summary>
    public static readonly StyledProperty<Func<object?, string?>?> DividerKeySelectorProperty =
        AvaloniaProperty.Register<VirtualList, Func<object?, string?>?>(nameof(DividerKeySelector));

    /// <summary>Defines the <see cref="DividerMemberBinding"/> property.</summary>
    public static readonly StyledProperty<BindingBase?> DividerMemberBindingProperty =
        AvaloniaProperty.Register<VirtualList, BindingBase?>(nameof(DividerMemberBinding));

    /// <summary>Defines the <see cref="DividerKind"/> property.</summary>
    public static readonly StyledProperty<DividerKind> DividerKindProperty =
        AvaloniaProperty.Register<VirtualList, DividerKind>(nameof(DividerKind));

    private static readonly FuncTemplate<Panel?> s_listPanel = new(() => new VirtualizingStackPanel());

    private BindingEvaluator? _dividerEvaluator;
    private SwipeableItem? _confirming;

    static VirtualList()
    {
        ItemsPanelProperty.OverrideDefaultValue<VirtualList>(s_listPanel);
        LayoutProperty.Changed.AddClassHandler<VirtualList>((l, _) => l.OnLayoutChanged());
        DividerKeySelectorProperty.Changed.AddClassHandler<VirtualList>((l, _) => l.RefreshDividers());
        DividerKindProperty.Changed.AddClassHandler<VirtualList>((l, _) => l.RefreshDividers());
        DividerMemberBindingProperty.Changed.AddClassHandler<VirtualList>((l, e) =>
        {
            l._dividerEvaluator = e.NewValue is BindingBase b ? new BindingEvaluator(b) : null;
            l.RefreshDividers();
        });
        SwipeableProperty.Changed.AddClassHandler<VirtualList>((l, _) => l.Refresh());
        Item.ClickEvent.AddClassHandler<VirtualList>((l, e) => l.OnItemClick(e));
    }

    /// <summary>Initializes a new instance of the <see cref="VirtualList"/> class.</summary>
    public VirtualList()
    {
        InitializePaging();
        InitializeReorder();
    }

    /// <summary>
    /// The selection mode: <see cref="Avalonia.Controls.SelectionMode.Single"/> (default),
    /// <see cref="Avalonia.Controls.SelectionMode.Multiple"/> or <see cref="Avalonia.Controls.SelectionMode.Toggle"/>.
    /// A tap toggles a row in Multiple and Toggle modes.
    /// </summary>
    public new SelectionMode SelectionMode
    {
        get => base.SelectionMode;
        set => base.SelectionMode = value;
    }

    /// <summary>The selected items.</summary>
    public new IList? SelectedItems
    {
        get => base.SelectedItems;
        set => base.SelectedItems = value;
    }

    /// <summary>The selection model.</summary>
    public new ISelectionModel Selection
    {
        get => base.Selection;
        set => base.Selection = value;
    }

    /// <summary>List (default) or Grid.</summary>
    public VirtualListLayout Layout
    {
        get => GetValue(LayoutProperty);
        set => SetValue(LayoutProperty, value);
    }

    /// <summary>The columns of the grid layout. 0 derives them from <see cref="ItemWidth"/>.</summary>
    public int ItemsPerRow
    {
        get => GetValue(ItemsPerRowProperty);
        set => SetValue(ItemsPerRowProperty, value);
    }

    /// <summary>The cell width of the grid layout when <see cref="ItemsPerRow"/> is 0.</summary>
    public double ItemWidth
    {
        get => GetValue(ItemWidthProperty);
        set => SetValue(ItemWidthProperty, value);
    }

    /// <summary>The cell height of the grid layout. NaN makes cells square.</summary>
    public double ItemHeight
    {
        get => GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    /// <summary>True: rows are <see cref="SwipeableItem"/>s that can be swiped to delete.</summary>
    public bool Swipeable
    {
        get => GetValue(SwipeableProperty);
        set => SetValue(SwipeableProperty, value);
    }

    /// <summary>Returns the section key of an item; a change of key starts a section with a divider.</summary>
    public Func<object?, string?>? DividerKeySelector
    {
        get => GetValue(DividerKeySelectorProperty);
        set => SetValue(DividerKeySelectorProperty, value);
    }

    /// <summary>A binding that reads the section key of an item, used when <see cref="DividerKeySelector"/> is not set.</summary>
    [AssignBinding]
    public BindingBase? DividerMemberBinding
    {
        get => GetValue(DividerMemberBindingProperty);
        set => SetValue(DividerMemberBindingProperty, value);
    }

    /// <summary>The divider rows show: <see cref="Controls.DividerKind.Labeled"/> (default) or alphabetical.</summary>
    public DividerKind DividerKind
    {
        get => GetValue(DividerKindProperty);
        set => SetValue(DividerKindProperty, value);
    }

    /// <summary>The scroll viewer of the template, once applied.</summary>
    public ScrollViewer? ScrollViewer { get; private set; }

    /// <summary>True when the item at <paramref name="index"/> is selected.</summary>
    public bool IsSelected(int index) => Selection.IsSelected(index);

    /// <summary>Selects the item at <paramref name="index"/> (adding to the selection in Multiple and Toggle modes).</summary>
    public void Select(int index)
    {
        if (SelectionMode.HasFlag(SelectionMode.Multiple) || SelectionMode.HasFlag(SelectionMode.Toggle))
        {
            Selection.Select(index);
        }
        else
        {
            SelectedIndex = index;
        }
    }

    /// <summary>Deselects the item at <paramref name="index"/>.</summary>
    public void Deselect(int index) => Selection.Deselect(index);

    /// <summary>Re-creates the content of the realized rows from their items (Enyo <c>refresh</c>).</summary>
    public void Refresh()
    {
        var generator = ItemContainerGenerator;
        foreach (var container in GetRealizedContainers())
        {
            var index = IndexFromContainer(container);
            if (index < 0 || index >= ItemCount)
            {
                continue;
            }

            var item = ItemsView[index];
            generator.ClearItemContainer(container);
            generator.PrepareItemContainer(container, item, index);
            generator.ItemContainerPrepared(container, item, index);
        }

        RefreshPages();
    }

    /// <summary>Clears the selection, releases the loaded pages and scrolls back to the top (Enyo <c>reset</c>).</summary>
    public void Reset()
    {
        Selection.Clear();
        if (_confirming is { } c)
        {
            c.ResetSwipe();
            _confirming = null;
        }

        ResetPages();
        if (ScrollViewer is { } sv)
        {
            sv.Offset = default;
        }

        Refresh();
    }

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) =>
        Swipeable ? NeedsContainer<SwipeableItem>(item, out recycleKey) : NeedsContainer<Item>(item, out recycleKey);

    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) =>
        Swipeable ? new SwipeableItem() : new Item();

    /// <inheritdoc/>
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        UpdateDivider(container, index);
        if (Layout == VirtualListLayout.Grid && container is Item cell)
        {
            cell.Classes.Set("grid-cell", true);
        }
    }

    /// <inheritdoc/>
    protected override void ContainerIndexChangedOverride(Control container, int oldIndex, int newIndex)
    {
        base.ContainerIndexChangedOverride(container, oldIndex, newIndex);
        UpdateDivider(container, newIndex);
    }

    /// <inheritdoc/>
    protected override void ClearContainerForItemOverride(Control container)
    {
        base.ClearContainerForItemOverride(container);
        if (container is SwipeableItem swipe)
        {
            swipe.ResetSwipe();
            if (_confirming == swipe)
            {
                _confirming = null;
            }
        }

        if (container is Item row)
        {
            row.ShowDivider = false;
            row.DividerCaption = null;
        }
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (ScrollViewer is not null)
        {
            ScrollViewer.ScrollChanged -= OnScrollChanged;
        }

        ScrollViewer = e.NameScope.Find<ScrollViewer>("PART_ScrollViewer");
        if (ScrollViewer is not null)
        {
            ScrollViewer.ScrollChanged += OnScrollChanged;
        }
    }

    /// <summary>Called by a row that shows its confirm prompt; hides the prompt of the previous row.</summary>
    internal bool OnRowConfirming(SwipeableItem row)
    {
        var autoConfirmed = false;
        if (_confirming is { } previous && previous != row)
        {
            autoConfirmed = previous.AutoHide();
        }

        _confirming = row;
        return autoConfirmed;
    }

    /// <summary>The section key of the item at <paramref name="index"/>, or null when sections are off.</summary>
    protected string? DividerKeyAt(int index)
    {
        if (index < 0 || index >= ItemCount)
        {
            return null;
        }

        var item = ItemsView[index];
        if (DividerKeySelector is { } selector)
        {
            return selector(item);
        }

        return _dividerEvaluator?.Evaluate(item)?.ToString();
    }

    private bool HasSections => DividerKeySelector is not null || _dividerEvaluator is not null;

    private void UpdateDivider(Control container, int index)
    {
        if (container is not Item row)
        {
            return;
        }

        row.DividerKind = DividerKind;
        if (!HasSections || Layout == VirtualListLayout.Grid)
        {
            row.ShowDivider = false;
            return;
        }

        var key = DividerKeyAt(index);
        var show = key is not null && (index == 0 || !string.Equals(key, DividerKeyAt(index - 1), StringComparison.Ordinal));
        row.ShowDivider = show;
        row.DividerCaption = show ? key : null;
    }

    private void RefreshDividers()
    {
        foreach (var container in GetRealizedContainers())
        {
            UpdateDivider(container, IndexFromContainer(container));
        }
    }

    private void OnLayoutChanged()
    {
        PseudoClasses.Set(":grid", Layout == VirtualListLayout.Grid);
        if (Layout == VirtualListLayout.Grid)
        {
            SetCurrentValue(ItemsPanelProperty, new FuncTemplate<Panel?>(() =>
            {
                var panel = new VirtualizingUniformGridPanel();
                panel.Bind(VirtualizingUniformGridPanel.ItemsPerRowProperty, this.GetObservable(ItemsPerRowProperty));
                panel.Bind(VirtualizingUniformGridPanel.ItemWidthProperty, this.GetObservable(ItemWidthProperty));
                panel.Bind(VirtualizingUniformGridPanel.ItemHeightProperty, this.GetObservable(ItemHeightProperty));
                return panel;
            }));
        }
        else
        {
            ClearValue(ItemsPanelProperty);
        }
    }

    private void OnItemClick(RoutedEventArgs e)
    {
        if (e.Source is not Control source || source.FindAncestorOfType<VirtualList>(includeSelf: false) is { } nested && nested != this)
        {
            return;
        }

        var index = IndexFromContainer(source);
        if (index < 0)
        {
            return;
        }

        var toggle = SelectionMode.HasFlag(SelectionMode.Multiple) || SelectionMode.HasFlag(SelectionMode.Toggle);
        UpdateSelection(index, select: true, rangeModifier: false, toggleModifier: toggle);
    }

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e) => UpdatePages();
}
