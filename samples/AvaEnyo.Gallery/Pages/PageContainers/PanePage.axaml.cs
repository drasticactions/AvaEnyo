using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using AvaEnyo.Animations;

namespace AvaEnyo.Gallery.Pages.PageContainers;

public partial class PanePage : UserControl
{
    private int _created;

    public PanePage()
    {
        InitializeComponent();
        TransitionGroup.ValueChanged += (_, _) => DemoPane.Transition = TransitionGroup.SelectedIndex switch
        {
            1 => EnyoPaneTransitions.FlyIn,
            2 => EnyoPaneTransitions.None,
            _ => EnyoPaneTransitions.Fade,
        };
        BackButton.Click += (_, _) => DemoPane.Back();
        NextButton.Click += (_, _) => DemoPane.Next();
        LazyButton.Click += (_, _) => DemoPane.SelectViewByName("lazy");
        DynamicButton.Click += (_, _) => DemoPane.SelectViewByName("dynamic" + (++_created));
        DemoPane.CreateView += (_, e) => e.View = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(0x40, 0x8a, 0x9c)),
            Child = new TextBlock
            {
                Text = "Created for \"" + e.Name + "\"",
                Foreground = Brushes.White,
                FontSize = 28,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };
        DemoPane.SelectionChanged += (_, _) => UpdateStatus();
        UpdateStatus();
    }

    private void UpdateStatus() =>
        StatusText.Text = $"Selected: {DemoPane.SelectedView?.Name ?? "-"} · views: {DemoPane.Views.Count} · can go back: {DemoPane.CanGoBack}";
}
