using Avalonia.Controls;
using AvaEnyo.Controls;

namespace AvaEnyo.Gallery.Pages.Popups;

public partial class AppMenuPage : UserControl
{
    public AppMenuPage()
    {
        InitializeComponent();
        OpenButton.Click += (_, _) => DemoMenu.Open();
        DemoMenu.Closed += (_, e) =>
        {
            if (e.Reason == PopupCloseReason.Handle)
            {
                ResultText.Text = $"Chosen: {e.Value}";
            }
        };
    }
}
