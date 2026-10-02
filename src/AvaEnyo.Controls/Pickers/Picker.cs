using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace AvaEnyo.Controls;

/// <summary>
/// The Enyo picker (<c>enyo.Picker</c>): a pill that shows the selected item. A tap opens the list of items in a
/// popup at the pill, placed so the selected row covers the pill where there is room; choosing a row closes it and
/// raises <see cref="ValueChanged"/>.
/// </summary>
/// <remarks>
/// <see cref="Value"/> is the selected item. In a <see cref="PickerGroup"/> the group dims the window and keeps all
/// its pills visible above the scrim, so a tap on a sibling pill switches to that picker's list.
/// </remarks>
[TemplatePart("PART_Button", typeof(PickerButton), IsRequired = true)]
[TemplatePart("PART_Popup", typeof(Popup), IsRequired = true)]
[TemplatePart("PART_ListHost", typeof(ScrollViewer))]
[PseudoClasses(":open")]
public class Picker : SelectingItemsControl
{
    /// <summary>Defines the <see cref="Value"/> property.</summary>
    public static readonly StyledProperty<object?> ValueProperty =
        AvaloniaProperty.Register<Picker, object?>(nameof(Value), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Defines the <see cref="TextAlign"/> property.</summary>
    public static readonly StyledProperty<TextAlignment> TextAlignProperty =
        AvaloniaProperty.Register<Picker, TextAlignment>(nameof(TextAlign), TextAlignment.Center);

    /// <summary>Defines the <see cref="IsScrimmed"/> property.</summary>
    public static readonly StyledProperty<bool> IsScrimmedProperty =
        AvaloniaProperty.Register<Picker, bool>(nameof(IsScrimmed), true);

    /// <summary>Defines the <see cref="IsDropDownOpen"/> property.</summary>
    public static readonly DirectProperty<Picker, bool> IsDropDownOpenProperty =
        AvaloniaProperty.RegisterDirect<Picker, bool>(nameof(IsDropDownOpen), o => o.IsDropDownOpen);

    /// <summary>Defines the <see cref="ValueChanged"/> event.</summary>
    public static readonly RoutedEvent<PickerValueChangedEventArgs> ValueChangedEvent =
        RoutedEvent.Register<Picker, PickerValueChangedEventArgs>(nameof(ValueChanged), RoutingStrategies.Bubble);

    private readonly PickerList _list;
    private PickerButton? _button;
    private Popup? _popup;
    private bool _syncing;
    private bool _isDropDownOpen;
    private int _openGeneration;

    static Picker()
    {
        SelectedItemProperty.Changed.AddClassHandler<Picker>((p, e) => p.OnSelectedItemChanged(e));
        ValueProperty.Changed.AddClassHandler<Picker>((p, e) => p.OnValueChanged(e));
        SelectedIndexProperty.Changed.AddClassHandler<Picker>((p, _) => p.SyncList());
    }

    /// <summary>Initializes a new instance of the <see cref="Picker"/> class.</summary>
    public Picker()
    {
        _list = new PickerList();
        _list.Picked += OnPicked;
    }

    /// <summary>The selected item (Enyo <c>value</c>). Setting a value that is not an item is ignored.</summary>
    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>The alignment of the rows' text. Default centered.</summary>
    public TextAlignment TextAlign
    {
        get => GetValue(TextAlignProperty);
        set => SetValue(TextAlignProperty, value);
    }

    /// <summary>True (default): the list opens over a dim scrim.</summary>
    public bool IsScrimmed
    {
        get => GetValue(IsScrimmedProperty);
        set => SetValue(IsScrimmedProperty, value);
    }

    /// <summary>True while the list is open.</summary>
    public bool IsDropDownOpen
    {
        get => _isDropDownOpen;
        private set => SetAndRaise(IsDropDownOpenProperty, ref _isDropDownOpen, value);
    }

    /// <summary>Raised when the value changes.</summary>
    public event EventHandler<PickerValueChangedEventArgs>? ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }

    /// <summary>The pill.</summary>
    internal PickerButton? Pill => _button;

    /// <summary>The list in the popup.</summary>
    internal PickerList List => _list;

    /// <summary>The popup of the list.</summary>
    internal Popup? ListPopup => _popup;

    /// <summary>The picker group this picker belongs to, if any.</summary>
    internal PickerGroup? Group { get; set; }

    /// <summary>Opens the list.</summary>
    public void OpenDropDown()
    {
        if (_popup is null || _button is null || !IsEffectivelyEnabled)
        {
            return;
        }

        Group?.OnPickerOpening(this);
        _popup.ShowScrim = IsScrimmed && Group is null;
        _list.ItemsSource = ItemsView;
        _list.TextAlign = TextAlign;
        _list.ItemTemplate = ItemTemplate;
        _list.SelectedIndex = SelectedIndex;
        var generation = ++_openGeneration;
        _popup.OpenAtControl(_button);
        if (!_popup.IsOpen)
        {
            return;
        }

        if (_popup.Presenter is { } presenter)
        {
            // Enyo set the list's min-width to the pill's width instead of the popup's 300 px.
            presenter.MinWidth = _button.Bounds.Width;
        }

        IsDropDownOpen = true;
        PseudoClasses.Set(":open", true);
        _button.IsFocusOpen = true;
        AlignSelectedRow(generation);
        Dispatcher.UIThread.Post(() => AlignSelectedRow(generation), DispatcherPriority.Render);
    }

    /// <summary>Closes the list.</summary>
    public void CloseDropDown() => _popup?.Close();

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_button is not null)
        {
            _button.Click -= OnButtonClick;
        }

        if (_popup is not null)
        {
            _popup.Closed -= OnPopupClosed;
        }

        _button = e.NameScope.Find<PickerButton>("PART_Button");
        _popup = e.NameScope.Find<Popup>("PART_Popup");
        if (_button is not null)
        {
            _button.Click += OnButtonClick;
        }

        if (_popup is not null)
        {
            _popup.Closed += OnPopupClosed;
            (_list.Parent as ContentControl)?.SetCurrentValue(ContentControl.ContentProperty, null);
            if (e.NameScope.Find<ScrollViewer>("PART_ListHost") is { } host)
            {
                host.Content = _list;
            }
            else
            {
                _popup.Content = _list;
            }
        }
    }

    /// <summary>Sets the focus look of the pill (a group sets it on all its pills).</summary>
    internal void SetFocusOpen(bool value)
    {
        if (_button is not null)
        {
            _button.IsFocusOpen = value;
        }
    }

    /// <summary>The index of the item equal to <paramref name="value"/>, or -1.</summary>
    protected int IndexOfValue(object? value)
    {
        for (var i = 0; i < ItemCount; i++)
        {
            if (Equals(Items[i], value))
            {
                return i;
            }
        }

        return -1;
    }

    private void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        OpenDropDown();
    }

    private void OnPopupClosed(object? sender, PopupClosedEventArgs e)
    {
        _openGeneration++;
        IsDropDownOpen = false;
        PseudoClasses.Set(":open", false);
        SetFocusOpen(false);
        Group?.OnPickerClosed(this, e.Reason);
    }

    private void SyncList()
    {
        // The handler can run from the base constructor, before the list exists.
        if (_list is { } list)
        {
            list.SelectedIndex = SelectedIndex;
        }
    }

    private void OnPicked(object? sender, RoutedEventArgs e)
    {
        SelectedIndex = _list.SelectedIndex;
        CloseDropDown();
    }

    private void AlignSelectedRow(int generation)
    {
        if (generation != _openGeneration || _popup?.Presenter is not { } presenter || _button is null)
        {
            return;
        }

        if (_list.ContainerFromIndex(_list.SelectedIndex) is not { } row)
        {
            return;
        }

        row.BringIntoView();
        presenter.UpdateLayout();
        if (row.TranslatePoint(default, presenter) is { } p)
        {
            // Put the selected row where the pill is, as Enyo's scrollToSelected did; the overlay keeps it on screen.
            var offset = new Vector(0, -p.Y + ((_button.Bounds.Height - row.Bounds.Height) / 2));
            _popup.OpenAtControl(_button, offset);
        }
    }

    private void OnSelectedItemChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_syncing)
        {
            return;
        }

        _syncing = true;
        try
        {
            SetCurrentValue(ValueProperty, e.NewValue);
        }
        finally
        {
            _syncing = false;
        }

        RaiseEvent(new PickerValueChangedEventArgs(ValueChangedEvent, this, e.OldValue, e.NewValue));
    }

    private void OnValueChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_syncing)
        {
            return;
        }

        _syncing = true;
        try
        {
            var index = IndexOfValue(e.NewValue);
            if (index < 0 && e.NewValue is not null)
            {
                // Enyo kept the old value when the new one was not an item.
                SetCurrentValue(ValueProperty, e.OldValue);
            }
            else
            {
                SelectedIndex = index;
            }
        }
        finally
        {
            _syncing = false;
        }
    }
}
