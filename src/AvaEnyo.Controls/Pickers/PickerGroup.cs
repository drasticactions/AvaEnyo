using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;

namespace AvaEnyo.Controls;

/// <summary>
/// A label followed by a row of <see cref="Picker"/>s (<c>enyo.PickerGroup</c>). While any of its lists is open,
/// every pill of the group shows the focus look and the group stays visible above the scrim; a tap on a sibling pill
/// closes the open list and opens that picker's list instead.
/// </summary>
/// <remarks>
/// The pills above the scrim are a live copy (<see cref="PickerGroupOverlay"/>), as Enyo copied its pickers' HTML into
/// a popup. A tap that lands outside the open list closes it; when that tap was on a sibling pill, the group opens the
/// sibling.
/// </remarks>
[TemplatePart("PART_Label", typeof(TextBlock))]
[TemplatePart("PART_ItemsPresenter", typeof(ItemsPresenter), IsRequired = true)]
[TemplatePart("PART_Overlay", typeof(PickerGroupOverlay))]
[PseudoClasses(":open")]
public class PickerGroup : ItemsControl
{
    /// <summary>Defines the <see cref="Label"/> property.</summary>
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<PickerGroup, string?>(nameof(Label));

    /// <summary>Defines the <see cref="IsScrimmed"/> property.</summary>
    public static readonly StyledProperty<bool> IsScrimmedProperty =
        AvaloniaProperty.Register<PickerGroup, bool>(nameof(IsScrimmed), true);

    /// <summary>Defines the <see cref="ValueChanged"/> event.</summary>
    public static readonly RoutedEvent<PickerValueChangedEventArgs> ValueChangedEvent =
        RoutedEvent.Register<PickerGroup, PickerValueChangedEventArgs>(nameof(ValueChanged), RoutingStrategies.Bubble);

    private static readonly FuncTemplate<Panel?> s_panel = new(() => new StackPanel { Orientation = Orientation.Horizontal });

    private readonly List<Picker> _pickers = new();
    private PickerGroupOverlay? _overlay;
    private ItemsPresenter? _itemsPresenter;
    private TopLevel? _top;
    private Point? _lastPress;

    static PickerGroup()
    {
        ItemsPanelProperty.OverrideDefaultValue<PickerGroup>(s_panel);
    }

    /// <summary>The label before the pickers, in the Enyo label style (blue, uppercase).</summary>
    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>True (default): a dim scrim covers the window while a list is open.</summary>
    public bool IsScrimmed
    {
        get => GetValue(IsScrimmedProperty);
        set => SetValue(IsScrimmedProperty, value);
    }

    /// <summary>The pickers of the group.</summary>
    public IReadOnlyList<Picker> Pickers => _pickers;

    /// <summary>Raised when the value of the group changes. Date and time pickers raise it for their value.</summary>
    public event EventHandler<PickerValueChangedEventArgs>? ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _itemsPresenter = e.NameScope.Find<ItemsPresenter>("PART_ItemsPresenter");
        _overlay = e.NameScope.Find<PickerGroupOverlay>("PART_Overlay");
    }

    /// <inheritdoc/>
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is Picker picker && !_pickers.Contains(picker))
        {
            picker.Group = this;
            _pickers.Add(picker);
        }
    }

    /// <inheritdoc/>
    protected override void ClearContainerForItemOverride(Control container)
    {
        base.ClearContainerForItemOverride(container);
        if (container is Picker picker)
        {
            picker.Group = null;
            _pickers.Remove(picker);
        }
    }

    /// <summary>Called by a picker of the group before its list opens.</summary>
    internal void OnPickerOpening(Picker picker)
    {
        foreach (var p in _pickers)
        {
            p.SetFocusOpen(true);
        }

        PseudoClasses.Set(":open", true);
        if (_overlay is { IsOpen: false } overlay && _itemsPresenter is not null)
        {
            overlay.Show(_itemsPresenter, IsScrimmed);
            _top = TopLevel.GetTopLevel(this);
            _top?.AddHandler(PointerPressedEvent, OnTopPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
        }
    }

    /// <summary>Called by a picker of the group after its list closed.</summary>
    internal void OnPickerClosed(Picker picker, PopupCloseReason reason)
    {
        var press = _lastPress;
        _lastPress = null;
        if (reason == PopupCloseReason.Click && press is { } point && FindPickerAt(point) is { } sibling && sibling != picker)
        {
            // Enyo's pickerPopupClick: a tap on a sibling pill opens that picker.
            Dispatcher.UIThread.Post(sibling.OpenDropDown, DispatcherPriority.Input);
            return;
        }

        foreach (var p in _pickers)
        {
            if (p.IsDropDownOpen)
            {
                return;
            }
        }

        foreach (var p in _pickers)
        {
            p.SetFocusOpen(false);
        }

        PseudoClasses.Set(":open", false);
        _overlay?.Close();
        _top?.RemoveHandler(PointerPressedEvent, OnTopPressed);
        _top = null;
    }

    /// <summary>Raises <see cref="ValueChanged"/>.</summary>
    protected void RaiseValueChanged(object? oldValue, object? newValue) =>
        RaiseEvent(new PickerValueChangedEventArgs(ValueChangedEvent, this, oldValue, newValue));

    private void OnTopPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_top is not null)
        {
            _lastPress = e.GetPosition(_top);
        }
    }

    private Picker? FindPickerAt(Point point)
    {
        if (_top is null)
        {
            return null;
        }

        foreach (var p in _pickers)
        {
            if (p.Pill is { } pill && pill.TranslatePoint(default, _top) is { } origin
                && new Rect(origin, pill.Bounds.Size).Contains(point))
            {
                return p;
            }
        }

        return null;
    }
}
