namespace AvaEnyo.Controls.Tests;

public static partial class ThemedControls
{
    static partial void AddExtras()
    {
        Add("TokenInput", new("TokenInput", () => new TokenInput { Tokens = { "a", "b" }, Text = "c" }, ["PART_Panel", "PART_TextBox", "PART_ShowAll", "PART_SuggestionsPopup", "PART_Frame"], false));
        Add("TokenChip", new("TokenInput", () => new TokenChip { Content = "x", IsSelected = true }, ["PART_Border", "PART_ContentPresenter"], false));
        Add("NumericKeypad", new("NumericKeypad", () => new NumericKeypad(), ["PART_Grid"], true));
        Add("NumericKeypadPin", new("NumericKeypad", () => new NumericKeypad { Mode = NumericKeypadMode.Pin }, ["PART_Grid"], true));
        Add("NumericKeypadButton", new("NumericKeypad", () => new NumericKeypadButton { Content = "2", Letters = "ABC" }, ["PART_ContentPresenter", "PART_Letters", "PART_Icon"], false));
    }
}
