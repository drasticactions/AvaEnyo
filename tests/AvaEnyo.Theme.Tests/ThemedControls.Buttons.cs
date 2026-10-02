using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AvaEnyo.Theme.Tests;

public static partial class ThemedControls
{
    static partial void AddButtons()
    {
        Add("Button", () => new Button { Content = "x", Classes = { "affirmative" } }, ["PART_Background", "PART_Frame", "PART_ContentPresenter", "PART_FocusVisual"]);
        Add("ToggleButton", () => new ToggleButton { Content = "x" }, ["PART_Background", "PART_Frame", "PART_ContentPresenter"]);
        Add("RepeatButton", () => new RepeatButton { Content = "x" }, ["PART_Background", "PART_Frame", "PART_ContentPresenter"]);
    }
}
