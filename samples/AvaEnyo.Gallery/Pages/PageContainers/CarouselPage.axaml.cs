using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using AvaEnyo.Samples;

namespace AvaEnyo.Gallery.Pages.PageContainers;

public partial class CarouselPage : UserControl
{
    private int _center;

    public CarouselPage()
    {
        InitializeComponent();

        // The endless carousel shows integers; its template turns one into a photo with a caption.
        Endless.ContentTemplate = new FuncDataTemplate<int>((i, _) => Photo(SampleAssets.Thumbnail(Mod(i), 640), $"View {i}"));
        Endless.GetLeft += (_, e) =>
        {
            if (e.IsSnap)
            {
                _center--;
            }

            e.View = _center - 1;
        };
        Endless.GetRight += (_, e) =>
        {
            if (e.IsSnap)
            {
                _center++;
            }

            e.View = _center + 1;
        };
        Endless.SnapFinished += (_, _) => EndlessText.Text = $"Center view: {_center}";
        Endless.SetCenterView(_center);
        EndlessText.Text = "Center view: 0";
        PrevButton.Click += (_, _) => Endless.Previous();
        NextButton.Click += (_, _) => Endless.Next();

        var photos = Enumerable.Range(0, SampleAssets.PhotoCount).Select(i => SampleAssets.Thumbnail(i, 640)).ToList();
        Virtual.ContentTemplate = new FuncDataTemplate<Bitmap>((b, _) => Photo(b, null));
        Virtual.ItemsSource = photos;
        Virtual.PropertyChanged += (_, e) =>
        {
            if (e.Property == AvaEnyo.Controls.VirtualCarousel.SelectedIndexProperty)
            {
                VirtualText.Text = $"Photo {Virtual.SelectedIndex + 1} of {photos.Count}";
            }
        };
        VirtualText.Text = $"Photo 1 of {photos.Count}";

        Strip.ItemTemplate = new FuncDataTemplate<Bitmap>((b, _) => new Border
        {
            Margin = new Thickness(4),
            CornerRadius = new CornerRadius(6),
            ClipToBounds = true,
            Child = new Image { Source = b, Stretch = Stretch.UniformToFill },
        });
        Strip.ItemsSource = photos.Take(12).ToList();
    }

    private static int Mod(int i) => ((i % SampleAssets.PhotoCount) + SampleAssets.PhotoCount) % SampleAssets.PhotoCount;

    private static Control Photo(Bitmap? bitmap, string? caption)
    {
        var panel = new Panel { Background = Brushes.Black };
        panel.Children.Add(new Image { Source = bitmap, Stretch = Stretch.UniformToFill });
        if (caption is not null)
        {
            panel.Children.Add(new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(0x99, 0, 0, 0)),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom,
                Padding = new Thickness(10, 6),
                Child = new TextBlock { Text = caption, Foreground = Brushes.White },
            });
        }

        return panel;
    }
}
