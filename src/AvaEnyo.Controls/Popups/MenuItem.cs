using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AvaEnyo.Animations;
using AvaEnyo.Controls.Primitives;

namespace AvaEnyo.Controls;

/// <summary>
/// A row of an Enyo <see cref="Menu"/> (<c>enyo.MenuItem</c>): an optional icon, the caption, and for an item with
/// children a <c>menuitem-arrow</c> chevron. Tapping an item with children opens or closes its drawer of children,
/// indented 24 px per level; tapping a leaf raises <see cref="Click"/>, and the menu closes with the item's
/// <see cref="Value"/>. In XAML use <c>enyo:MenuItem</c>; the short name also exists in Avalonia.
/// </summary>
[TemplatePart("PART_Row", typeof(Border), IsRequired = true)]
[TemplatePart("PART_ItemsPresenter", typeof(ItemsPresenter))]
[PseudoClasses(":pressed", ":open", ":has-items", ":first", ":middle", ":last", ":single")]
public class MenuItem : HeaderedItemsControl
{
    /// <summary>Defines the <see cref="Icon"/> property.</summary>
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<MenuItem, object?>(nameof(Icon));

    /// <summary>Defines the <see cref="Value"/> property.</summary>
    public static readonly StyledProperty<object?> ValueProperty =
        AvaloniaProperty.Register<MenuItem, object?>(nameof(Value));

    /// <summary>Defines the <see cref="IsOpen"/> property.</summary>
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<MenuItem, bool>(nameof(IsOpen));

    /// <summary>Defines the <see cref="Depth"/> property.</summary>
    public static readonly DirectProperty<MenuItem, int> DepthProperty =
        AvaloniaProperty.RegisterDirect<MenuItem, int>(nameof(Depth), o => o.Depth);

    /// <summary>Defines the <see cref="Click"/> event.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
        RoutedEvent.Register<MenuItem, RoutedEventArgs>(nameof(Click), RoutingStrategies.Bubble);

    private static readonly FuncTemplate<Panel?> s_panel = new(() => new OrderedPanel());

    private Border? _row;
    private ItemsPresenter? _drawer;
    private IPointer? _pressed;
    private int _depth;

    static MenuItem()
    {
        ItemsPanelProperty.OverrideDefaultValue<MenuItem>(s_panel);
        IsOpenProperty.Changed.AddClassHandler<MenuItem>((m, e) => m.OnIsOpenChanged(e.GetNewValue<bool>()));
    }

    /// <summary>Initializes a new instance of the <see cref="MenuItem"/> class.</summary>
    public MenuItem()
    {
        Items.CollectionChanged += (_, _) => UpdatePseudoClasses();
    }

    /// <summary>An icon before the caption: an image, a geometry, a geometry resource key or a control.</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>The value the menu reports when this item is chosen. Null means the <see cref="HeaderedItemsControl.Header"/>.</summary>
    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>True while the item's drawer of children is open.</summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>The nesting level: 0 in the menu, 1 for a child, and so on. Each level indents the row 24 px.</summary>
    public int Depth
    {
        get => _depth;
        internal set
        {
            SetAndRaise(DepthProperty, ref _depth, value);
            UpdateIndent();
        }
    }

    /// <summary>The value reported when the item is chosen: <see cref="Value"/>, else the header.</summary>
    public object? EffectiveValue => Value ?? Header;

    /// <summary>Raised when a leaf item is tapped or activated with Enter or Space.</summary>
    public event EventHandler<RoutedEventArgs>? Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    /// <summary>Closes this item's drawer and those of all its children (Enyo <c>closeAll</c>).</summary>
    public void CloseAll()
    {
        SetCurrentValue(IsOpenProperty, false);
        foreach (var item in Items)
        {
            (item as MenuItem)?.CloseAll();
        }
    }

    /// <summary>Taps the item: toggles the drawer of an item with children, otherwise raises <see cref="Click"/>.</summary>
    public void Activate()
    {
        if (!IsEffectivelyEnabled)
        {
            return;
        }

        if (ItemCount > 0)
        {
            SetCurrentValue(IsOpenProperty, !IsOpen);
            return;
        }

        OnClick();
        RaiseEvent(new RoutedEventArgs(ClickEvent, this));
    }

    /// <summary>Called before <see cref="Click"/> is raised.</summary>
    protected virtual void OnClick()
    {
    }

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) =>
        NeedsContainer<MenuItem>(item, out recycleKey);

    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => new MenuItem();

    /// <inheritdoc/>
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is MenuItem child)
        {
            child.Depth = Depth + 1;
            if (!ReferenceEquals(child, item) && !child.IsSet(HeaderProperty))
            {
                child.Header = item;
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_row is not null)
        {
            _row.PointerPressed -= OnRowPressed;
            _row.PointerReleased -= OnRowReleased;
            _row.PointerCaptureLost -= OnRowCaptureLost;
        }

        _row = e.NameScope.Find<Border>("PART_Row");
        _drawer = e.NameScope.Find<ItemsPresenter>("PART_ItemsPresenter");
        if (_row is not null)
        {
            _row.PointerPressed += OnRowPressed;
            _row.PointerReleased += OnRowReleased;
            _row.PointerCaptureLost += OnRowCaptureLost;
        }

        if (_drawer is not null)
        {
            _drawer.IsVisible = IsOpen;
        }

        UpdateIndent();
        UpdatePseudoClasses();
    }

    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!e.Handled && (e.Key == Key.Enter || e.Key == Key.Space))
        {
            e.Handled = true;
            Activate();
        }
    }

    private void OnRowPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsEffectivelyEnabled || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        _pressed = e.Pointer;
        PseudoClasses.Set(":pressed", true);
        e.Handled = true;
    }

    private void OnRowReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.Pointer != _pressed || _row is null)
        {
            return;
        }

        var inside = new Rect(_row.Bounds.Size).Contains(e.GetPosition(_row));
        EndPress();
        if (inside)
        {
            e.Handled = true;
            Activate();
        }
    }

    private void OnRowCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (e.Pointer != _pressed)
        {
            return;
        }

        // A scroller took the pointer for a drag: no tap.
        if (e.Pointer.Captured is Visual v && _row is not null && (v == _row || _row.IsVisualAncestorOf(v)))
        {
            return;
        }

        EndPress();
    }

    private void EndPress()
    {
        _pressed = null;
        PseudoClasses.Set(":pressed", false);
    }

    private void OnIsOpenChanged(bool open)
    {
        PseudoClasses.Set(":open", open);
        if (_drawer is null)
        {
            return;
        }

        if (!this.IsAttachedToVisualTree() || !EnyoAnimations.IsActive)
        {
            _drawer.IsVisible = open;
            return;
        }

        _ = open ? EnyoAnimations.ExpandHeight(_drawer) : EnyoAnimations.CollapseHeight(_drawer);
    }

    private void UpdateIndent()
    {
        if (_row is null)
        {
            return;
        }

        var indent = this.TryFindResource("EnyoMenuItemIndent", out var v) && v is double d ? d : 24;
        var padding = this.TryFindResource("EnyoMenuItemPadding", out var p) && p is Thickness t ? t : new Thickness(10);
        _row.Padding = new Thickness(padding.Left + (Depth * indent), padding.Top, padding.Right, padding.Bottom);
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":has-items", ItemCount > 0 || Items.Count > 0);
        PseudoClasses.Set(":open", IsOpen);
    }
}
