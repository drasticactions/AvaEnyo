using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AvaEnyo.Controls.Primitives;

namespace AvaEnyo.Controls;

/// <summary>
/// A 3×4 number pad drawn after the webOS dial pad (telephony <c>Dialpad</c>), without the telephony.
/// <see cref="NumericKeypadMode.Phone"/> has 1–9, <c>*</c>, 0 and <c>#</c> with letters under the digits; holding 0
/// presses <c>+</c>. <see cref="NumericKeypadMode.Pin"/> has 1–9, an empty key, 0 and backspace. Every key raises
/// <see cref="KeyPressed"/>; typed digits do too while the pad has focus.
/// </summary>
[TemplatePart("PART_Grid", typeof(Panel), IsRequired = true)]
public class NumericKeypad : TemplatedControl
{
    /// <summary>The key string of the backspace key.</summary>
    public const string BackspaceKey = "Backspace";

    /// <summary>Defines the <see cref="Mode"/> property.</summary>
    public static readonly StyledProperty<NumericKeypadMode> ModeProperty =
        AvaloniaProperty.Register<NumericKeypad, NumericKeypadMode>(nameof(Mode));

    /// <summary>Defines the <see cref="ShowLetters"/> property.</summary>
    public static readonly StyledProperty<bool> ShowLettersProperty =
        AvaloniaProperty.Register<NumericKeypad, bool>(nameof(ShowLetters), true);

    /// <summary>Defines the <see cref="KeyPressed"/> event.</summary>
    public static readonly RoutedEvent<KeypadKeyEventArgs> KeyPressedEvent =
        RoutedEvent.Register<NumericKeypad, KeypadKeyEventArgs>(nameof(KeyPressed), RoutingStrategies.Bubble);

    private static readonly string[] s_letters = [string.Empty, "ABC", "DEF", "GHI", "JKL", "MNO", "PQRS", "TUV", "WXYZ"];

    private Panel? _grid;

    static NumericKeypad()
    {
        FocusableProperty.OverrideDefaultValue<NumericKeypad>(true);
        ModeProperty.Changed.AddClassHandler<NumericKeypad>((k, _) => k.BuildKeys());
        ShowLettersProperty.Changed.AddClassHandler<NumericKeypad>((k, _) => k.BuildKeys());
        Button.ClickEvent.AddClassHandler<NumericKeypad>((k, e) => k.OnKeyClick(e));
        EnyoGestures.HoldEvent.AddClassHandler<NumericKeypad>((k, e) => k.OnHold(e));
        PointerPressedEvent.AddClassHandler<NumericKeypad>((k, e) => k.OnPressed(e), RoutingStrategies.Tunnel, handledEventsToo: true);
        TextInputEvent.AddClassHandler<NumericKeypad>((k, e) => k.OnText(e));
    }

    /// <summary>The key layout.</summary>
    public NumericKeypadMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    /// <summary>True (default): Phone mode shows the letters under the digits.</summary>
    public bool ShowLetters
    {
        get => GetValue(ShowLettersProperty);
        set => SetValue(ShowLettersProperty, value);
    }

    /// <summary>Raised for every pressed key.</summary>
    public event EventHandler<KeypadKeyEventArgs>? KeyPressed
    {
        add => AddHandler(KeyPressedEvent, value);
        remove => RemoveHandler(KeyPressedEvent, value);
    }

    /// <summary>Raises <see cref="KeyPressed"/> for <paramref name="key"/>, as a tap on that key would.</summary>
    public void Press(string key) => RaiseEvent(new KeypadKeyEventArgs(KeyPressedEvent, this, key));

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _grid = e.NameScope.Find<Panel>("PART_Grid");
        BuildKeys();
    }

    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!e.Handled && e.Key == Key.Back && Mode == NumericKeypadMode.Pin)
        {
            e.Handled = true;
            Press(BackspaceKey);
        }
    }

    private void BuildKeys()
    {
        if (_grid is null)
        {
            return;
        }

        _grid.Children.Clear();
        var letters = ShowLetters && Mode == NumericKeypadMode.Phone;
        for (var d = 1; d <= 9; d++)
        {
            _grid.Children.Add(MakeKey(d.ToString(System.Globalization.CultureInfo.InvariantCulture), s_letters[d - 1], letters));
        }

        if (Mode == NumericKeypadMode.Phone)
        {
            _grid.Children.Add(MakeKey("*", null, letters));
            _grid.Children.Add(MakeKey("0", "+", letters));
            _grid.Children.Add(MakeKey("#", null, letters));
        }
        else
        {
            _grid.Children.Add(new Panel());
            _grid.Children.Add(MakeKey("0", null, false));
            var back = MakeKey(BackspaceKey, null, false);
            back.Icon = this.TryFindResource(EnyoGlyph.Backspace, out var glyph) ? glyph : EnyoGlyph.Backspace;
            back.Content = null;
            Avalonia.Automation.AutomationProperties.SetName(back, "Backspace");
            _grid.Children.Add(back);
        }
    }

    private static NumericKeypadButton MakeKey(string key, string? letters, bool showLetters) => new()
    {
        Key = key,
        Content = key,
        Letters = letters,
        ShowLetters = showLetters,
    };

    private static NumericKeypadButton? KeyOf(object? source) =>
        (source as Visual)?.FindAncestorOfType<NumericKeypadButton>(includeSelf: true);

    private void OnPressed(PointerPressedEventArgs e)
    {
        if (KeyOf(e.Source) is { } key)
        {
            key.HoldFired = false;
        }
    }

    private void OnHold(HoldEventArgs e)
    {
        if (Mode == NumericKeypadMode.Phone && KeyOf(e.Source) is { Key: "0" } key && !key.HoldFired)
        {
            key.HoldFired = true;
            Press("+");
        }
    }

    private void OnKeyClick(RoutedEventArgs e)
    {
        if (KeyOf(e.Source) is not { } key || e.Source != key)
        {
            return;
        }

        e.Handled = true;
        if (key.HoldFired)
        {
            // The hold already pressed "+"; the release is not also a 0.
            key.HoldFired = false;
            return;
        }

        Press(key.Key);
    }

    private void OnText(TextInputEventArgs e)
    {
        if (e.Handled || string.IsNullOrEmpty(e.Text))
        {
            return;
        }

        foreach (var c in e.Text)
        {
            if (char.IsAsciiDigit(c) || (Mode == NumericKeypadMode.Phone && (c == '*' || c == '#' || c == '+')))
            {
                Press(c.ToString());
                e.Handled = true;
            }
        }
    }
}
