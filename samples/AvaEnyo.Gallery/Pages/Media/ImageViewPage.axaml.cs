using Avalonia.Controls;
using AvaEnyo.Samples;

namespace AvaEnyo.Gallery.Pages.Media;

public partial class ImageViewPage : UserControl
{
    public ImageViewPage()
    {
        InitializeComponent();
        var photos = new System.Collections.Generic.List<object>();
        for (var i = 0; i < 12; i++)
        {
            photos.Add(SampleAssets.Photo(i));
        }

        Viewer.Images = photos;
        Prev.Click += (_, _) => Viewer.Previous();
        Next.Click += (_, _) => Viewer.Next();
    }
}
