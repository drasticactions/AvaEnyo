using Avalonia.Controls;

namespace AvaEnyo.Gallery.Pages.StockControls;

public partial class NotificationsPage : UserControl
{
    public NotificationsPage()
    {
        InitializeComponent();
        InfoButton.Click += (_, _) => Manager()?.Show(new Avalonia.Controls.Notifications.Notification("New mail", "3 new messages from Grace Hopper"));
        ErrorButton.Click += (_, _) => Manager()?.Show(new Avalonia.Controls.Notifications.Notification("Sync failed", "Could not reach the server", Avalonia.Controls.Notifications.NotificationType.Error));
    }

    private Avalonia.Controls.Notifications.WindowNotificationManager? _manager;

    private Avalonia.Controls.Notifications.WindowNotificationManager? Manager()
    {
        if (_manager is null && TopLevel.GetTopLevel(this) is { } top)
        {
            _manager = new Avalonia.Controls.Notifications.WindowNotificationManager(top) { Position = Avalonia.Controls.Notifications.NotificationPosition.BottomRight, MaxItems = 3 };
        }

        return _manager;
    }
}
