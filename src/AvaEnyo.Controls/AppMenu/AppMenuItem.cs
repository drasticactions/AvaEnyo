using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AvaEnyo.Controls;

/// <summary>
/// A row of an <see cref="AppMenu"/> (<c>enyo.AppMenuItem</c>): white 18 px text with a 2 px dark divider below
/// (<c>appmenu-divider.png</c>), the blue held highlight while pressed. Clicking an item without children runs its
/// <see cref="Command"/>, raises <see cref="Click"/> and closes the menu with reason
/// <see cref="PopupCloseReason.Handle"/> and <see cref="Value"/> (or the header). An item with children opens and
/// closes them below itself, indented 24 px.
/// </summary>
[TemplatePart("PART_Header", typeof(Control), IsRequired = true)]
[TemplatePart("PART_ItemsPresenter", typeof(Avalonia.Controls.Presenters.ItemsPresenter), IsRequired = true)]
[PseudoClasses(":pressed", ":held", ":expanded", ":has-items")]
public class AppMenuItem : HeaderedItemsControl
{
    /// <summary>Defines the <see cref="Icon"/> property.</summary>
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<AppMenuItem, object?>(nameof(Icon));

    /// <summary>Defines the <see cref="Value"/> property.</summary>
    public static readonly StyledProperty<object?> ValueProperty =
        AvaloniaProperty.Register<AppMenuItem, object?>(nameof(Value));

    /// <summary>Defines the <see cref="Command"/> property.</summary>
    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<AppMenuItem, ICommand?>(nameof(Command));

    /// <summary>Defines the <see cref="CommandParameter"/> property.</summary>
    public static readonly StyledProperty<object?> CommandParameterProperty =
        AvaloniaProperty.Register<AppMenuItem, object?>(nameof(CommandParameter));

    /// <summary>Defines the <see cref="IsExpanded"/> property.</summary>
    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<AppMenuItem, bool>(nameof(IsExpanded));

    /// <summary>Defines the <see cref="Shortcut"/> property.</summary>
    public static readonly StyledProperty<string?> ShortcutProperty =
        AvaloniaProperty.Register<AppMenuItem, string?>(nameof(Shortcut));

    /// <summary>Defines the <see cref="ShowShortcut"/> property.</summary>
    public static readonly StyledProperty<bool> ShowShortcutProperty =
        AvaloniaProperty.Register<AppMenuItem, bool>(nameof(ShowShortcut));

    /// <summary>Defines the <see cref="Click"/> event.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
        RoutedEvent.Register<AppMenuItem, RoutedEventArgs>(nameof(Click), RoutingStrategies.Bubble);

    private IPointer? _pressed;

    static AppMenuItem()
    {
        FocusableProperty.OverrideDefaultValue<AppMenuItem>(true);
        ItemsPanelProperty.OverrideDefaultValue<AppMenuItem>(new Avalonia.Controls.Templates.FuncTemplate<Panel?>(() => new Primitives.OrderedPanel()));
        IsExpandedProperty.Changed.AddClassHandler<AppMenuItem>((i, _) => i.UpdatePseudoClasses());
        ItemCountProperty.Changed.AddClassHandler<AppMenuItem>((i, _) => i.UpdatePseudoClasses());
    }

    /// <summary>Initializes a new instance of the <see cref="AppMenuItem"/> class.</summary>
    public AppMenuItem()
    {
        AddHandler(PointerCaptureLostEvent, OnCaptureLost, RoutingStrategies.Direct | RoutingStrategies.Bubble, handledEventsToo: true);
    }

    /// <summary>An icon before the header: an image, a geometry or a control.</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>The value the menu closes with when this item is clicked. Null means the header.</summary>
    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>The command run on click.</summary>
    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>The parameter of <see cref="Command"/>.</summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <summary>True while the children show.</summary>
    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>A shortcut letter, shown at the right as "Ctrl+A" when <see cref="ShowShortcut"/> is on.</summary>
    public string? Shortcut
    {
        get => GetValue(ShortcutProperty);
        set => SetValue(ShortcutProperty, value);
    }

    /// <summary>True to show <see cref="Shortcut"/>.</summary>
    public bool ShowShortcut
    {
        get => GetValue(ShowShortcutProperty);
        set => SetValue(ShowShortcutProperty, value);
    }

    /// <summary>Raised when the item is clicked.</summary>
    public event EventHandler<RoutedEventArgs>? Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    /// <summary>Clicks the item, as a tap would.</summary>
    public void RaiseClick()
    {
        if (!IsEffectivelyEnabled)
        {
            return;
        }

        if (ItemCount > 0)
        {
            SetCurrentValue(IsExpandedProperty, !IsExpanded);
            return;
        }

        OnClick();
        RaiseEvent(new RoutedEventArgs(ClickEvent, this));
        if (Command is { } command && command.CanExecute(CommandParameter))
        {
            command.Execute(CommandParameter);
        }

        if ((this.FindAncestorOfType<PopupPresenter>() ?? Avalonia.LogicalTree.LogicalExtensions.FindLogicalAncestorOfType<PopupPresenter>(this))?.Owner is { } popup)
        {
            popup.CloseCore(PopupCloseReason.Handle, Value ?? Header);
        }
    }

    /// <summary>Called before <see cref="Click"/> is raised.</summary>
    protected virtual void OnClick()
    {
    }

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) =>
        NeedsContainer<AppMenuItem>(item, out recycleKey);

    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => new AppMenuItem();

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdatePseudoClasses();
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.Handled || !IsEffectivelyEnabled || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed || !IsInHeader(e.Source))
        {
            return;
        }

        e.Handled = true;
        _pressed = e.Pointer;
        e.Pointer.Capture(this);
        SetPressed(true);
    }

    /// <inheritdoc/>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.Pointer != _pressed)
        {
            return;
        }

        _pressed = null;
        SetPressed(false);
        e.Pointer.Capture(null);
        e.Handled = true;
        if (new Rect(Bounds.Size).Contains(e.GetPosition(this)))
        {
            RaiseClick();
        }
    }

    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!e.Handled && (e.Key == Key.Enter || e.Key == Key.Space))
        {
            e.Handled = true;
            RaiseClick();
        }
    }

    private bool IsInHeader(object? source)
    {
        for (var v = source as Visual; v is not null && v != this; v = v.GetVisualParent())
        {
            if (v is AppMenuItem)
            {
                return false;
            }

            if (v is Control { Name: "PART_Header" })
            {
                return true;
            }
        }

        return source == this;
    }

    private void OnCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (e.Pointer == _pressed && e.Pointer.Captured != this)
        {
            _pressed = null;
            SetPressed(false);
        }
    }

    private void SetPressed(bool value)
    {
        PseudoClasses.Set(":pressed", value);
        PseudoClasses.Set(":held", value);
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":expanded", IsExpanded);
        PseudoClasses.Set(":has-items", ItemCount > 0);
    }
}
