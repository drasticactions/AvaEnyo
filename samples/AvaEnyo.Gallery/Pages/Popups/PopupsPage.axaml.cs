using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using AvaEnyo.Controls;

namespace AvaEnyo.Gallery.Pages.Popups;

public partial class PopupsPage : UserControl
{
    private PointerEventArgs? _lastPointer;

    public PopupsPage()
    {
        InitializeComponent();
        DemoPopup.Closed += (_, e) => ReasonText.Text = $"Closed: {e.Reason}" + (e.Value is null ? string.Empty : $" ({e.Value})");
        AddHandler(PointerReleasedEvent, (_, e) => _lastPointer = e, handledEventsToo: true);

        AtPointButton.Click += (_, _) => Prepare().OpenAt(new Point(40, 40));
        AtPointerButton.Click += (_, _) =>
        {
            if (_lastPointer is { } p)
            {
                Prepare().OpenAtPointer(p);
            }
            else
            {
                Prepare().OpenAroundControl(AtPointerButton);
            }
        };
        AtControlButton.Click += (_, _) => Prepare().OpenAtControl(AtControlButton, new Vector(10, 10));
        CenterButton.Click += (_, _) => Prepare().OpenAtCenter();
        NearButton.Click += (_, _) => Prepare().OpenNear(NearButton);
        BelowButton.Click += (_, _) => Prepare().OpenAroundControl(BelowButton, PopupAlignment.Below);
        AboveButton.Click += (_, _) => Prepare().OpenAroundControl(AboveButton, PopupAlignment.Above);
        RightButton.Click += (_, _) => Prepare().OpenAroundControl(RightButton, PopupAlignment.Right);
        LeftButton.Click += (_, _) => Prepare().OpenAroundControl(LeftButton, PopupAlignment.Left);
        AroundCenterButton.Click += (_, _) => Prepare().OpenAroundControl(AroundCenterButton, PopupAlignment.Center);
    }

    private AvaEnyo.Controls.Popup Prepare()
    {
        DemoPopup.IsModal = ModalSwitch.IsChecked == true;
        DemoPopup.ShowScrim = ScrimSwitch.IsChecked == true;
        return DemoPopup;
    }
}
