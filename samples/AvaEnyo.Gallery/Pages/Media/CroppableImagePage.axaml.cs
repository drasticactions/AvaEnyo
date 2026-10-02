using Avalonia.Controls;
using AvaEnyo.Samples;

namespace AvaEnyo.Gallery.Pages.Media;

public partial class CroppableImagePage : UserControl
{
    public CroppableImagePage()
    {
        InitializeComponent();
        Cropper.Source = SampleAssets.Photo(3);
        ZoomSlider.PropertyChanged += (_, e) =>
        {
            if (e.Property == Slider.ValueProperty)
            {
                Cropper.Zoom = Cropper.MinZoom * ZoomSlider.Value;
            }
        };
        CropButton.Click += (_, _) =>
        {
            var bitmap = Cropper.Crop();
            Result.Source = bitmap;
            Size.Text = bitmap is null ? "Nothing to crop" : $"{bitmap.PixelSize.Width} × {bitmap.PixelSize.Height} px";
        };
    }
}
