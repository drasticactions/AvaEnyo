using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;

namespace AvaEnyo.Theme.Tests;

public static partial class ThemedControls
{
    static partial void AddContainers()
    {
        Add("ScrollViewer", () => new ScrollViewer { Content = new Border { Width = 1000, Height = 1000 }, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto },
            ["PART_ContentPresenter", "PART_HorizontalScrollBar", "PART_VerticalScrollBar", "PART_FadeTop", "PART_FadeBottom", "PART_FadeLeft", "PART_FadeRight"], transparent: true);
        Add("MenuScrollViewer", () => new ScrollViewer { Content = new Border { Height = 1000 }, Theme = (ControlTheme)Avalonia.Application.Current!.FindResource("EnyoMenuScrollViewer")! },
            ["PART_ContentPresenter", "PART_FadeTop", "PART_FadeBottom"], transparent: true);
        Add("ScrollBar", () => new ScrollBar { Maximum = 100, ViewportSize = 10 },
            ["PART_LineUpButton", "PART_LineDownButton", "PART_PageUpButton", "PART_PageDownButton", "PART_Track", "PART_Thumb"], transparent: true);
    }
}
