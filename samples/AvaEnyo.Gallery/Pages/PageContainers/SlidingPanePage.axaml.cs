using System.Linq;
using Avalonia.Controls;

namespace AvaEnyo.Gallery.Pages.PageContainers;

public partial class SlidingPanePage : UserControl
{
    public SlidingPanePage()
    {
        InitializeComponent();
        ShowButton.Click += (_, _) =>
        {
            var last = Pane.Views.Last();
            last.IsShowing = true;
            Pane.SelectView(last);
        };
    }
}
