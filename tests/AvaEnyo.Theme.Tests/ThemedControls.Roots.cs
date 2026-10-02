using Avalonia.Controls;

namespace AvaEnyo.Theme.Tests;

public static partial class ThemedControls
{
    static partial void AddRoots()
    {
        Add("TextBlock", () => new TextBlock { Text = "x", Classes = { "enyo-label" } }, transparent: true);
        Add("ThemeVariantScope", () => new ThemeVariantScope { Child = new TextBlock { Text = "x" } }, transparent: true);
    }
}
