using Avalonia.Controls;
using AvaEnyo.Samples;

namespace AvaEnyo.Gallery.Pages.Forms;

public partial class FormsPage : UserControl
{
    public FormsPage()
    {
        InitializeComponent();
        Avatar.Source = SampleAssets.Avatar(0);
    }
}
