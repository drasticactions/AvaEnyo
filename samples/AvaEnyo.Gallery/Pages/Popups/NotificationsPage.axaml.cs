using System.Linq;
using Avalonia;
using Avalonia.Controls;
using AvaEnyo.Samples;
using AvaEnyo.Services;

namespace AvaEnyo.Gallery.Pages.Popups;

public partial class NotificationsPage : UserControl
{
    private InAppNotificationService? _service;
    private Notification? _last;
    private int _count;

    public NotificationsPage()
    {
        InitializeComponent();
        ShowButton.Click += (_, _) =>
        {
            var contact = SampleData.Contacts[_count % SampleData.Contacts.Count];
            _last = new Notification("n" + (++_count))
            {
                Title = contact.Name,
                Text = "New message " + _count,
                Icon = SampleAssets.Avatar(contact.AvatarIndex),
            };
            Service()?.Show(_last);
        };
        LayerButton.Click += (_, _) =>
        {
            if (_last is not null)
            {
                _last.AddLayer(_last.Title, "Another message (" + (_last.Layers.Count + 1) + ")");
                Service()?.Update(_last);
            }
        };
        UpdateButton.Click += (_, _) =>
        {
            if (_last is not null)
            {
                _last.Text = "Updated at " + System.DateTime.Now.ToString("T", System.Globalization.CultureInfo.CurrentCulture);
                Service()?.Update(_last);
            }
        };
        DismissButton.Click += (_, _) =>
        {
            if (_last is not null)
            {
                Service()?.Dismiss(_last.Id);
            }
        };
    }

    /// <summary>
    /// Creates the service on first use. The dashboard presenter needs AvaEnyoServicesTheme; an app adds it in
    /// App.axaml, and this page adds it at run time if the app has not.
    /// </summary>
    private InAppNotificationService? Service()
    {
        if (_service is not null)
        {
            return _service;
        }

        if (Application.Current is { } app && !app.Styles.OfType<AvaEnyoServicesTheme>().Any())
        {
            app.Styles.Add(new AvaEnyoServicesTheme());
        }

        if (TopLevel.GetTopLevel(this) is not { } top)
        {
            return null;
        }

        _service = new InAppNotificationService(top);
        _service.Activated += (_, e) => EventText.Text = $"Activated: {e.Notification.DisplayTitle}";
        _service.Dismissed += (_, e) => EventText.Text = $"Dismissed: {e.Notification.DisplayTitle}";
        return _service;
    }
}
