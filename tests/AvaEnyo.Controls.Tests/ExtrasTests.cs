using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AvaEnyo.Controls.Primitives;
using Xunit;

namespace AvaEnyo.Controls.Tests;

public class ExtrasTests
{
    private static (TokenInput Input, TextBox Box, Window Window, Button Other) MakeInput(double width = 400)
    {
        var input = new TokenInput { Width = width };
        var other = new Button { Content = "Other" };
        var w = TestHost.Show(new StackPanel { Children = { input, other } });
        var box = input.GetVisualDescendants().OfType<TextBox>().Single(t => t.Name == "PART_TextBox");
        return (input, box, w, other);
    }

    private static void Key(TextBox box, Key key) =>
        box.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = key, Source = box });

    private static List<TokenChip> Chips(TokenInput input) => input.GetVisualDescendants().OfType<TokenChip>().ToList();

    [AvaloniaFact]
    public void Separators_Atomize_Each_Entry()
    {
        var (input, box, w, _) = MakeInput();
        box.Focus();
        box.Text = "a, b;";
        TestHost.Settle(w);
        Assert.Equal(new object[] { "a", "b" }, input.Tokens.ToArray());
        Assert.True(string.IsNullOrEmpty(input.Text));
        Assert.Equal(2, Chips(input).Count);
        w.Close();
    }

    [AvaloniaFact]
    public void Canceled_Atomizing_Keeps_The_Text()
    {
        var (input, box, w, _) = MakeInput();
        input.Atomizing += (_, e) => e.Cancel = e.Text == "bad";
        box.Focus();
        box.Text = "bad";
        Key(box, Avalonia.Input.Key.Enter);
        Assert.Empty(input.Tokens);
        Assert.Equal("bad", input.Text);
        w.Close();
    }

    [AvaloniaFact]
    public void Atomizing_Can_Replace_The_Token()
    {
        var (input, box, w, _) = MakeInput();
        input.Atomizing += (_, e) => e.Token = e.Text.ToUpperInvariant();
        box.Focus();
        box.Text = "ada";
        Key(box, Avalonia.Input.Key.Enter);
        Assert.Equal("ADA", input.Tokens.Single());
        w.Close();
    }

    [AvaloniaFact]
    public void Enter_Atomizes()
    {
        var (input, box, w, _) = MakeInput();
        box.Focus();
        box.Text = "enyo";
        Key(box, Avalonia.Input.Key.Enter);
        Assert.Equal("enyo", input.Tokens.Single());
        w.Close();
    }

    [AvaloniaFact]
    public void Losing_Focus_Atomizes()
    {
        var (input, box, w, other) = MakeInput();
        box.Focus();
        box.Text = "webos";
        other.Focus();
        TestHost.Settle(w);
        Assert.Equal("webos", input.Tokens.Single());
        w.Close();
    }

    [AvaloniaFact]
    public void Backspace_Selects_Then_Removes_The_Last_Chip()
    {
        var (input, box, w, _) = MakeInput();
        input.Tokens.Add("a");
        input.Tokens.Add("b");
        box.Focus();
        TestHost.Settle(w);
        Key(box, Avalonia.Input.Key.Back);
        Assert.Equal(2, input.Tokens.Count);
        Assert.True(Chips(input).Last().IsSelected);
        Key(box, Avalonia.Input.Key.Back);
        Assert.Equal(new object[] { "a" }, input.Tokens.ToArray());
        w.Close();
    }

    [AvaloniaFact]
    public void FilterText_Follows_After_The_Delay()
    {
        var (input, box, w, _) = MakeInput();
        input.FilterDelay = 20;
        string? filter = null;
        input.FilterTextChanged += (_, e) => filter = e.Text;
        box.Focus();
        box.Text = "ad";
        Assert.Null(filter);
        TestHost.Pump(1000, () => filter is not null);
        Assert.Equal("ad", filter);
        Assert.Equal("ad", input.FilterText);
        w.Close();
    }

    [AvaloniaFact]
    public void Choosing_A_Suggestion_Atomizes_It()
    {
        var (input, box, w, _) = MakeInput();
        input.FilterDelay = 0;
        input.Suggestions = new[] { "Ada Lovelace", "Alan Kay" };
        box.Focus();
        box.Text = "a";
        TestHost.Settle(w);
        var popup = input.GetVisualDescendants().OfType<Popup>().Single();
        Assert.True(popup.IsOpen);
        Assert.True(box.IsFocused);
        var list = (ListBox)popup.Content!;
        list.SelectedIndex = 1;
        Assert.Equal("Alan Kay", input.Tokens.Single());
        Assert.False(popup.IsOpen);
        Assert.True(string.IsNullOrEmpty(input.Text));
        w.Close();
    }

    [AvaloniaFact]
    public void Without_Focus_It_Collapses_To_One_Row()
    {
        var (input, box, w, other) = MakeInput(220);
        for (var i = 0; i < 8; i++)
        {
            input.Tokens.Add("token " + i);
        }

        other.Focus();
        TestHost.Settle(w);
        Assert.True(input.IsCollapsed);
        var panel = input.GetVisualDescendants().OfType<TokenPanel>().Single();
        Assert.True(panel.HiddenCount > 0);
        var more = input.GetVisualDescendants().OfType<Button>().Single(b => b.Name == "PART_ShowAll");
        Assert.Equal("+" + panel.HiddenCount, more.Content);
        Assert.True(more.Bounds.Width > 0);
        more.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, more));
        TestHost.Settle(w);
        Assert.False(input.IsCollapsed);
        Assert.Equal(0, panel.HiddenCount);
        Assert.True(input.Bounds.Height > 60);
        w.Close();
    }

    [AvaloniaFact]
    public void Chip_Delete_Raises_Remove()
    {
        var chip = new TokenChip { Content = "x" };
        var removed = 0;
        chip.Remove += (_, _) => removed++;
        var w = TestHost.Show(chip);
        chip.Focus();
        chip.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Avalonia.Input.Key.Delete, Source = chip });
        Assert.Equal(1, removed);
        w.Close();
    }

    [AvaloniaFact]
    public void Chip_Remove_Removes_The_Token()
    {
        var (input, _, w, _) = MakeInput();
        input.Tokens.Add("a");
        input.Tokens.Add("b");
        TestHost.Settle(w);
        var first = Chips(input)[0];
        first.RaiseEvent(new RoutedEventArgs(TokenChip.RemoveEvent, first));
        Assert.Equal(new object[] { "b" }, input.Tokens.ToArray());
        w.Close();
    }

    private static List<NumericKeypadButton> Keys(NumericKeypad pad) => pad.GetVisualDescendants().OfType<NumericKeypadButton>().ToList();

    [AvaloniaFact]
    public void Phone_And_Pin_Layouts()
    {
        var pad = new NumericKeypad { Width = 300 };
        var w = TestHost.Show(pad);
        Assert.Equal(new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "*", "0", "#" }, Keys(pad).Select(k => k.Key));
        Assert.Equal("ABC", Keys(pad)[1].Letters);
        Assert.True(Keys(pad)[1].ShowLetters);
        pad.Mode = NumericKeypadMode.Pin;
        TestHost.Settle(w);
        Assert.Equal(new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", NumericKeypad.BackspaceKey }, Keys(pad).Select(k => k.Key));
        Assert.False(Keys(pad)[0].ShowLetters);
        Assert.NotNull(Keys(pad).Last().Icon);
        w.Close();
    }

    [AvaloniaFact]
    public void Keys_Raise_KeyPressed()
    {
        var pad = new NumericKeypad { Width = 300 };
        var pressed = new List<string>();
        pad.KeyPressed += (_, e) => pressed.Add(e.Key);
        var w = TestHost.Show(pad);
        var keys = Keys(pad);
        keys[4].RaiseEvent(new RoutedEventArgs(Button.ClickEvent, keys[4]));
        keys[11].RaiseEvent(new RoutedEventArgs(Button.ClickEvent, keys[11]));
        pad.Focus();
        pad.RaiseEvent(new TextInputEventArgs { RoutedEvent = InputElement.TextInputEvent, Text = "7", Source = pad });
        Assert.Equal(new[] { "5", "#", "7" }, pressed);
        w.Close();
    }

    [AvaloniaFact]
    public void Holding_Zero_Presses_Plus()
    {
        var old = EnyoGestures.HoldDelay;
        EnyoGestures.HoldDelay = 20;
        try
        {
            var pad = new NumericKeypad { Width = 300, Height = 320 };
            var pressed = new List<string>();
            pad.KeyPressed += (_, e) => pressed.Add(e.Key);
            var w = TestHost.Show(new Canvas { Children = { pad } }, 400, 400);
            var zero = Keys(pad).Single(k => k.Key == "0");
            var p = zero.TranslatePoint(new Point(zero.Bounds.Width / 2, zero.Bounds.Height / 2), w)!.Value;
            var touch = new TouchInput(w);
            touch.Down(p);
            TestHost.Pump(1000, () => pressed.Count > 0);
            touch.Up(p);
            TestHost.Settle(w);
            Assert.Equal(new[] { "+" }, pressed);
            w.Close();
        }
        finally
        {
            EnyoGestures.HoldDelay = old;
        }
    }
}
