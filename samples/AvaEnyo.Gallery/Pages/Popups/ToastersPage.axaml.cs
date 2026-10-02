using Avalonia.Controls;
using AvaEnyo.Animations;

namespace AvaEnyo.Gallery.Pages.Popups;

public partial class ToastersPage : UserControl
{
    public ToastersPage()
    {
        InitializeComponent();
        BottomButton.Click += (_, _) => Show(ToasterEdge.Bottom);
        TopButton.Click += (_, _) => Show(ToasterEdge.Top);
        LeftButton.Click += (_, _) => Show(ToasterEdge.Left);
        RightButton.Click += (_, _) => Show(ToasterEdge.Right);
        DialogButton.Click += (_, _) => DemoDialog.Open();
    }

    private void Show(ToasterEdge edge)
    {
        DemoToaster.Close();
        DemoToaster.FlyInFrom = edge;
        ToasterText.Text = "From the " + edge.ToString().ToLowerInvariant();
        DemoToaster.Open();
    }
}
