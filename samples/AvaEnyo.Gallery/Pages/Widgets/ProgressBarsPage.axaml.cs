using Avalonia.Controls;

namespace AvaEnyo.Gallery.Pages.Widgets;

public partial class ProgressBarsPage : UserControl
{
    public ProgressBarsPage()
    {
        InitializeComponent();
        Bump.Click += (_, _) => Animated.Value = Animated.Value >= 100 ? 0 : Animated.Value + 20;
    }
}
