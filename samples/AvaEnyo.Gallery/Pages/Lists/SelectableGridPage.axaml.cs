using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using AvaEnyo.Controls;
using AvaEnyo.Samples;

namespace AvaEnyo.Gallery.Pages.Lists;

public partial class SelectableGridPage : UserControl
{
    public SelectableGridPage()
    {
        InitializeComponent();
        Grid.ItemsSource = Enumerable.Range(0, SampleAssets.PhotoCount).ToList();
        Grid.ItemTemplate = new FuncDataTemplate<int>((i, _) => new Image { Source = SampleAssets.Thumbnail(i, 200), Stretch = Stretch.UniformToFill });
        Grid.SelectionChanged += (_, _) => Update();
        SelectToggle.PropertyChanged += (_, e) =>
        {
            if (e.Property == ToolButton.IsCheckedProperty)
            {
                Grid.SelectionMode = SelectToggle.IsChecked ? SelectionMode.Toggle | SelectionMode.Multiple : SelectionMode.Single;
                if (!SelectToggle.IsChecked)
                {
                    Grid.SelectedItems?.Clear();
                }

                Update();
            }
        };
        Update();
    }

    private void Update() => Count.Text = SelectToggle.IsChecked ? $"{Grid.SelectedItems?.Count ?? 0} selected" : "Tap Select to pick photos";
}
