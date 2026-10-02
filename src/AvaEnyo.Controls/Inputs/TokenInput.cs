using System;
using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaEnyo.Controls.Primitives;

namespace AvaEnyo.Controls;

/// <summary>
/// A text box that turns entries into tokens (Enyo <c>AtomizingInput</c>, without the contact services): a wrapping
/// row of <see cref="TokenChip"/>s followed by a text box. Typing a separator (<c>,</c> or <c>;</c>), Enter, or
/// leaving the box turns the text into a token through the cancelable <see cref="Atomizing"/> event.
/// </summary>
/// <remarks>
/// <para>Backspace in an empty box selects the last chip; a second Backspace removes it. <see cref="FilterText"/>
/// follows the typed text <see cref="FilterDelay"/> ms after typing stops, for autocomplete; <see cref="Suggestions"/>
/// then show in an Onyx popup list below the input, and choosing one atomizes it.</para>
/// <para>Without focus the input collapses to one row with a "+N" button; the button expands it again.</para>
/// </remarks>
[TemplatePart("PART_Panel", typeof(TokenPanel), IsRequired = true)]
[TemplatePart("PART_TextBox", typeof(TextBox), IsRequired = true)]
[TemplatePart("PART_ShowAll", typeof(Button))]
[TemplatePart("PART_SuggestionsPopup", typeof(Popup))]
[PseudoClasses(":collapsed")]
public class TokenInput : TemplatedControl
{
    /// <summary>Defines the <see cref="Tokens"/> property.</summary>
    public static readonly DirectProperty<TokenInput, AvaloniaList<object>> TokensProperty =
        AvaloniaProperty.RegisterDirect<TokenInput, AvaloniaList<object>>(nameof(Tokens), o => o.Tokens, (o, v) => o.Tokens = v);

    /// <summary>Defines the <see cref="Text"/> property.</summary>
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<TokenInput, string?>(nameof(Text), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Defines the <see cref="PlaceholderText"/> property.</summary>
    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        TextBox.PlaceholderTextProperty.AddOwner<TokenInput>();

    /// <summary>Defines the <see cref="TokenTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> TokenTemplateProperty =
        AvaloniaProperty.Register<TokenInput, IDataTemplate?>(nameof(TokenTemplate));

    /// <summary>Defines the <see cref="FilterText"/> property.</summary>
    public static readonly DirectProperty<TokenInput, string> FilterTextProperty =
        AvaloniaProperty.RegisterDirect<TokenInput, string>(nameof(FilterText), o => o.FilterText);

    /// <summary>Defines the <see cref="FilterDelay"/> property.</summary>
    public static readonly StyledProperty<double> FilterDelayProperty =
        AvaloniaProperty.Register<TokenInput, double>(nameof(FilterDelay), 250);

    /// <summary>Defines the <see cref="Suggestions"/> property.</summary>
    public static readonly StyledProperty<IEnumerable?> SuggestionsProperty =
        AvaloniaProperty.Register<TokenInput, IEnumerable?>(nameof(Suggestions));

    /// <summary>Defines the <see cref="SuggestionsTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> SuggestionsTemplateProperty =
        AvaloniaProperty.Register<TokenInput, IDataTemplate?>(nameof(SuggestionsTemplate));

    /// <summary>Defines the <see cref="IsCollapsed"/> property.</summary>
    public static readonly DirectProperty<TokenInput, bool> IsCollapsedProperty =
        AvaloniaProperty.RegisterDirect<TokenInput, bool>(nameof(IsCollapsed), o => o.IsCollapsed);

    /// <summary>Defines the <see cref="Atomizing"/> event.</summary>
    public static readonly RoutedEvent<AtomizingEventArgs> AtomizingEvent =
        RoutedEvent.Register<TokenInput, AtomizingEventArgs>(nameof(Atomizing), RoutingStrategies.Bubble);

    /// <summary>Defines the <see cref="FilterTextChanged"/> event.</summary>
    public static readonly RoutedEvent<FilterTextChangedEventArgs> FilterTextChangedEvent =
        RoutedEvent.Register<TokenInput, FilterTextChangedEventArgs>(nameof(FilterTextChanged), RoutingStrategies.Bubble);

    private AvaloniaList<object> _tokens = new();
    private string _filterText = string.Empty;
    private bool _isCollapsed;
    private bool _expanded;
    private bool _openingSuggestions;
    private DispatcherTimer? _filterTimer;
    private TokenPanel? _panel;
    private TextBox? _textBox;
    private Button? _showAll;
    private Popup? _popup;
    private ListBox? _list;

    static TokenInput()
    {
        TextProperty.Changed.AddClassHandler<TokenInput>((t, e) => t.OnTextChanged(e.GetNewValue<string?>()));
        SuggestionsProperty.Changed.AddClassHandler<TokenInput>((t, _) => t.UpdateSuggestions());
        TokenTemplateProperty.Changed.AddClassHandler<TokenInput>((t, _) => t.RebuildChips());
        IsKeyboardFocusWithinProperty.Changed.AddClassHandler<TokenInput>((t, _) => t.OnFocusWithinChanged());
        TokenChip.RemoveEvent.AddClassHandler<TokenInput>((t, e) => t.OnChipRemove(e));
        Button.ClickEvent.AddClassHandler<TokenInput>((t, e) => t.OnClick(e));
    }

    /// <summary>Initializes a new instance of the <see cref="TokenInput"/> class.</summary>
    public TokenInput()
    {
        _tokens.CollectionChanged += OnTokensChanged;
        UpdateCollapsed();
    }

    /// <summary>The tokens, in order. A token is the text it came from unless an <see cref="Atomizing"/> handler chose another object.</summary>
    public AvaloniaList<object> Tokens
    {
        get => _tokens;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _tokens.CollectionChanged -= OnTokensChanged;
            SetAndRaise(TokensProperty, ref _tokens, value);
            _tokens.CollectionChanged += OnTokensChanged;
            RebuildChips();
        }
    }

    /// <summary>The text not yet turned into a token.</summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>The hint shown in the empty box.</summary>
    public string? PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    /// <summary>The template of a chip's content. Null shows the token's text.</summary>
    public IDataTemplate? TokenTemplate
    {
        get => GetValue(TokenTemplateProperty);
        set => SetValue(TokenTemplateProperty, value);
    }

    /// <summary>The typed text, updated <see cref="FilterDelay"/> ms after typing stops.</summary>
    public string FilterText
    {
        get => _filterText;
        private set => SetAndRaise(FilterTextProperty, ref _filterText, value);
    }

    /// <summary>The time in ms the text must be still before <see cref="FilterText"/> updates. Default 250.</summary>
    public double FilterDelay
    {
        get => GetValue(FilterDelayProperty);
        set => SetValue(FilterDelayProperty, value);
    }

    /// <summary>Suggestions for the typed text, shown in a popup list below the input.</summary>
    public IEnumerable? Suggestions
    {
        get => GetValue(SuggestionsProperty);
        set => SetValue(SuggestionsProperty, value);
    }

    /// <summary>The template of a suggestion row.</summary>
    public IDataTemplate? SuggestionsTemplate
    {
        get => GetValue(SuggestionsTemplateProperty);
        set => SetValue(SuggestionsTemplateProperty, value);
    }

    /// <summary>True while the input shows only its first row.</summary>
    public bool IsCollapsed
    {
        get => _isCollapsed;
        private set => SetAndRaise(IsCollapsedProperty, ref _isCollapsed, value);
    }

    /// <summary>Raised before text becomes a token; cancelable.</summary>
    public event EventHandler<AtomizingEventArgs>? Atomizing
    {
        add => AddHandler(AtomizingEvent, value);
        remove => RemoveHandler(AtomizingEvent, value);
    }

    /// <summary>Raised when <see cref="FilterText"/> changes.</summary>
    public event EventHandler<FilterTextChangedEventArgs>? FilterTextChanged
    {
        add => AddHandler(FilterTextChangedEvent, value);
        remove => RemoveHandler(FilterTextChangedEvent, value);
    }

    /// <summary>Turns the current text into a token. Returns true when a token was added.</summary>
    public bool Atomize() => Atomize(Text ?? string.Empty, clearText: true);

    /// <summary>Shows every row and moves focus to the text box.</summary>
    public void ShowAll()
    {
        _expanded = true;
        UpdateCollapsed();
        _textBox?.Focus();
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_textBox is not null)
        {
            _textBox.RemoveHandler(KeyDownEvent, OnTextBoxKeyDown);
            _textBox.LostFocus -= OnTextBoxLostFocus;
        }

        if (_list is not null)
        {
            _list.SelectionChanged -= OnSuggestionChosen;
        }

        _panel = e.NameScope.Find<TokenPanel>("PART_Panel");
        _textBox = e.NameScope.Find<TextBox>("PART_TextBox");
        _showAll = e.NameScope.Find<Button>("PART_ShowAll");
        _popup = e.NameScope.Find<Popup>("PART_SuggestionsPopup");
        if (_textBox is not null)
        {
            _textBox.AddHandler(KeyDownEvent, OnTextBoxKeyDown, RoutingStrategies.Tunnel);
            _textBox.LostFocus += OnTextBoxLostFocus;
        }

        if (_popup is not null)
        {
            _list = new ListBox { Focusable = false, MaxHeight = 300 };
            _list.Bind(ItemsControl.ItemsSourceProperty, this.GetObservable(SuggestionsProperty));
            _list.Bind(ItemsControl.ItemTemplateProperty, this.GetObservable(SuggestionsTemplateProperty));
            _list.SelectionChanged += OnSuggestionChosen;
            _popup.Content = _list;
        }

        RebuildChips();
        UpdateCollapsed();
    }

    private void OnTokensChanged(object? sender, NotifyCollectionChangedEventArgs e) => RebuildChips();

    private void RebuildChips()
    {
        if (_panel is null)
        {
            return;
        }

        for (var i = _panel.Children.Count - 1; i >= 0; i--)
        {
            if (_panel.Children[i] is TokenChip)
            {
                _panel.Children.RemoveAt(i);
            }
        }

        for (var i = 0; i < _tokens.Count; i++)
        {
            _panel.Children.Insert(i, new TokenChip { Content = _tokens[i], ContentTemplate = TokenTemplate });
        }
    }

    private TokenChip? LastChip()
    {
        if (_panel is null)
        {
            return null;
        }

        for (var i = _panel.Children.Count - 1; i >= 0; i--)
        {
            if (_panel.Children[i] is TokenChip chip)
            {
                return chip;
            }
        }

        return null;
    }

    private void DeselectAll()
    {
        if (_panel is null)
        {
            return;
        }

        foreach (var c in _panel.Children)
        {
            if (c is TokenChip chip)
            {
                chip.IsSelected = false;
            }
        }
    }

    private bool Atomize(string text, bool clearText)
    {
        var trimmed = text.Trim();
        if (trimmed.Length == 0)
        {
            if (clearText && !string.IsNullOrEmpty(Text))
            {
                SetCurrentValue(TextProperty, string.Empty);
            }

            return false;
        }

        var args = new AtomizingEventArgs(AtomizingEvent, this, trimmed);
        RaiseEvent(args);
        if (args.Cancel)
        {
            return false;
        }

        _tokens.Add(args.Token ?? trimmed);
        if (clearText)
        {
            SetCurrentValue(TextProperty, string.Empty);
        }

        return true;
    }

    private void OnTextChanged(string? text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            DeselectAll();
        }

        text ??= string.Empty;
        var last = text.LastIndexOfAny([',', ';']);
        if (last >= 0)
        {
            var remainder = text[(last + 1)..];
            var kept = new System.Text.StringBuilder();
            foreach (var part in text[..last].Split([',', ';']))
            {
                if (!Atomize(part, clearText: false) && part.Trim().Length > 0)
                {
                    kept.Append(part).Append(',');
                }
            }

            // Text that a handler refused stays in the box.
            SetCurrentValue(TextProperty, kept.ToString() + remainder.TrimStart());
            return;
        }

        RestartFilterTimer();
    }

    private void RestartFilterTimer()
    {
        _filterTimer?.Stop();
        var delay = FilterDelay;
        if (delay <= 0)
        {
            UpdateFilter();
            return;
        }

        _filterTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(delay), DispatcherPriority.Input, (_, _) =>
        {
            _filterTimer?.Stop();
            _filterTimer = null;
            UpdateFilter();
        });
        _filterTimer.Start();
    }

    private void UpdateFilter()
    {
        var text = (Text ?? string.Empty).Trim();
        if (text == FilterText)
        {
            return;
        }

        FilterText = text;
        RaiseEvent(new FilterTextChangedEventArgs(FilterTextChangedEvent, this, text));
        UpdateSuggestions();
    }

    private void UpdateSuggestions()
    {
        if (_popup is null)
        {
            return;
        }

        var any = Suggestions?.GetEnumerator().MoveNext() == true;
        if (any && !string.IsNullOrWhiteSpace(Text) && IsKeyboardFocusWithin)
        {
            if (!_popup.IsOpen)
            {
                _openingSuggestions = true;
                try
                {
                    _popup.OpenAroundControl(this, PopupAlignment.Below);

                    // The popup takes focus when it opens; typing goes on in the text box.
                    _textBox?.Focus();
                }
                finally
                {
                    _openingSuggestions = false;
                }
            }
        }
        else if (_popup.IsOpen)
        {
            _popup.Close();
        }
    }

    private void OnSuggestionChosen(object? sender, SelectionChangedEventArgs e)
    {
        if (_list?.SelectedItem is not { } item)
        {
            return;
        }

        _list.SelectedItem = null;
        var args = new AtomizingEventArgs(AtomizingEvent, this, item.ToString() ?? string.Empty) { Token = item };
        RaiseEvent(args);
        if (!args.Cancel)
        {
            _tokens.Add(args.Token ?? item);
            SetCurrentValue(TextProperty, string.Empty);
        }

        _popup?.Close();
        _textBox?.Focus();
    }

    private void OnTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                Atomize();
                e.Handled = true;
                break;
            case Key.Back when string.IsNullOrEmpty(Text):
                if (LastChip() is { } chip)
                {
                    if (chip.IsSelected)
                    {
                        _tokens.RemoveAt(_tokens.Count - 1);
                    }
                    else
                    {
                        chip.IsSelected = true;
                    }

                    e.Handled = true;
                }

                break;
        }
    }

    private void OnTextBoxLostFocus(object? sender, RoutedEventArgs e)
    {
        if (_openingSuggestions)
        {
            return;
        }

        // A click in the suggestion list moves focus into the popup; the choice atomizes there.
        if (_popup?.Presenter is { } presenter && TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() is Visual f
            && (f == presenter || presenter.IsVisualAncestorOf(f)))
        {
            return;
        }

        Atomize();
    }

    private void OnChipRemove(RoutedEventArgs e)
    {
        if (e.Source is TokenChip chip && _panel is not null)
        {
            var index = 0;
            foreach (var c in _panel.Children)
            {
                if (c == chip)
                {
                    _tokens.RemoveAt(index);
                    e.Handled = true;
                    _textBox?.Focus();
                    return;
                }

                if (c is TokenChip)
                {
                    index++;
                }
            }
        }
    }

    private void OnClick(RoutedEventArgs e)
    {
        if (e.Source == _showAll)
        {
            ShowAll();
            e.Handled = true;
        }
        else if (e.Source is TokenChip chip)
        {
            var select = !chip.IsSelected;
            DeselectAll();
            chip.IsSelected = select;
            e.Handled = true;
        }
    }

    private void OnFocusWithinChanged()
    {
        if (!IsKeyboardFocusWithin && !_openingSuggestions)
        {
            _expanded = false;
            DeselectAll();
        }

        UpdateCollapsed();
    }

    private void UpdateCollapsed()
    {
        IsCollapsed = !IsKeyboardFocusWithin && !_expanded;
        PseudoClasses.Set(":collapsed", IsCollapsed);
        if (_panel is not null)
        {
            _panel.IsCollapsed = IsCollapsed;
        }
    }
}
