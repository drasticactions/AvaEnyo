using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Threading;
using AvaEnyo.Animations;
using AvaEnyo.Controls;
using AvaEnyo.Mail.ViewModels;
using AvaEnyo.Mail.Views;
using AvaEnyo.Samples;
using AvaEnyo.Services;

namespace AvaEnyo.Mail;

/// <summary>
/// The Mail app, after Enyo's sliding layout samples: folders, messages and the reader in a SlidingPane, compose in
/// a sheet (a toaster from the bottom on a narrow screen), Preferences in a Pane view, the app menu, and new-mail
/// dashboard notifications. Escape or back walks compose → reader → messages → folders.
/// </summary>
public partial class MainView : UserControl
{
    private readonly MailViewModel _vm = new(SampleServiceBus.Create());
    private readonly DispatcherTimer _newMail = new() { Interval = TimeSpan.FromSeconds(30) };
    private PopupBase? _composeHost;
    private ComposeView? _compose;
    private INotificationService? _notifications;
    private ApplicationEvents? _events;
    private int _nextFake;

    public MainView()
    {
        InitializeComponent();
        DataContext = _vm;

        MessageList.DividerKeySelector = item => (item as MailMessage)?.DayCaption;
        MessageList.SelectionChanged += OnMessageSelectionChanged;
        MessageList.ContainerPrepared += (_, e) =>
        {
            if (e.Container is SwipeableItem row)
            {
                row.ConfirmRequired = _vm.Preferences.ConfirmDelete;
            }
        };
        MessageList.AddHandler(SwipeableItem.ConfirmedEvent, (_, e) =>
        {
            if ((e.Source as Control)?.DataContext is MailMessage m)
            {
                _vm.Delete(m);
            }
        });
        AccountsList.AddHandler(Item.ClickEvent, (_, e) =>
        {
            if ((e.Source as Control)?.DataContext is FolderRow folder)
            {
                _vm.SelectedFolder = folder;
                Sliding.SelectView(MessagesView);
            }
        });

        FoldersComposeButton.Click += (_, _) => OpenCompose();
        MessagesComposeButton.Click += (_, _) => OpenCompose();
        PreferencesButton.Click += (_, _) => ShowPreferences();
        PreferencesPage.Header.BackRequested += (_, e) => e.Handled = RootPane.Back();
        DeleteSelectedButton.Click += (_, _) => DeleteSelected();
        ReplyButton.Click += (_, _) => Reply(forward: false);
        ForwardButton.Click += (_, _) => Reply(forward: true);
        DeleteButton.Click += (_, _) =>
        {
            if (_vm.SelectedMessage is { } m)
            {
                _vm.Delete(m);
                Sliding.SelectView(MessagesView);
            }
        };

        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MailViewModel.SelectedMessage))
            {
                SenderAvatar.Source = _vm.SelectedMessage is { } m ? SampleAssets.Avatar(m.From.AvatarIndex) : null;
            }
            else if (e.PropertyName == nameof(MailViewModel.IsMultiSelect))
            {
                MessageList.SelectionMode = _vm.IsMultiSelect ? SelectionMode.Multiple | SelectionMode.Toggle : SelectionMode.Single;
                MessageList.Swipeable = !_vm.IsMultiSelect;
            }
        };
        _vm.Preferences.PropertyChanged += (_, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(PreferencesViewModel.DarkTheme):
                    Application.Current!.RequestedThemeVariant = _vm.Preferences.DarkTheme ? ThemeVariant.Dark : ThemeVariant.Light;
                    break;
                case nameof(PreferencesViewModel.NewMailNotifications):
                    _newMail.IsEnabled = _vm.Preferences.NewMailNotifications;
                    break;
                case nameof(PreferencesViewModel.ShowPreview) or nameof(PreferencesViewModel.ConfirmDelete):
                    MessageList.Refresh();
                    break;
            }
        };
        _newMail.Tick += (_, _) => ShowNewMail();
    }

    /// <summary>The view model, for the heads.</summary>
    public MailViewModel ViewModel => _vm;

    /// <summary>Opens a screen by name: folders, messages, reader, compose or preferences (the <c>--view</c> switch).</summary>
    public void ShowView(string name)
    {
        switch (name.ToLowerInvariant())
        {
            case "folders":
                Sliding.SelectView(FoldersView, immediate: true);
                break;
            case "messages":
                Sliding.SelectView(MessagesView, immediate: true);
                break;
            case "reader":
                if (_vm.Messages.Count > 0)
                {
                    MessageList.SelectedIndex = 0;
                }

                Sliding.SelectView(ReaderView, immediate: true);
                break;
            case "compose":
                OpenCompose(new Draft([SampleData.Contacts[2]], [], "Lunch on Thursday?", "Hi Carol,\n\nAre you free for lunch on Thursday?\n"));
                break;
            case "preferences":
                RootPane.SelectView(PreferencesPage, immediate: true);
                break;
        }
    }

    protected override async void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (TopLevel.GetTopLevel(this) is not { } top || _events is not null)
        {
            return;
        }

        _events = ApplicationEvents.Attach(top);
        _events.Deactivated += (_, _) => SaveDraft();
        AppMenuService.Attach(top, BuildAppMenu());
        var notifications = new InAppNotificationService(top);
        notifications.Activated += (_, n) =>
        {
            if (n.Notification.Tag is MailMessage m)
            {
                OpenMessage(m);
            }
        };
        _notifications = notifications;
        _newMail.IsEnabled = _vm.Preferences.NewMailNotifications;
        await _vm.LoadAsync();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _newMail.IsEnabled = false;
        if (TopLevel.GetTopLevel(this) is { } top)
        {
            AppMenuService.Detach(top);
        }

        _events?.Detach();
        _events = null;
        base.OnDetachedFromVisualTree(e);
    }

    private AppMenu BuildAppMenu()
    {
        var newMessage = new AppMenuItem { Header = "New message" };
        newMessage.Click += (_, _) => OpenCompose();
        var preferences = new AppMenuItem { Header = "Preferences" };
        preferences.Click += (_, _) => ShowPreferences();
        var menu = new AppMenu();
        menu.Items.Add(newMessage);
        menu.Items.Add(new EditMenu());
        menu.Items.Add(preferences);
        menu.Items.Add(new HelpMenu { Target = new Uri("https://github.com/drasticactions/avaenyo") });
        return menu;
    }

    private void ShowPreferences() => RootPane.SelectView(PreferencesPage);

    private void OnMessageSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_vm.IsMultiSelect || MessageList.SelectedItem is not MailMessage m)
        {
            return;
        }

        OpenMessage(m);
    }

    private void OpenMessage(MailMessage m)
    {
        RootPane.SelectView(RootPane.Views[0]);
        _vm.SelectedMessage = m;
        ReaderView.IsShowing = true;
        Sliding.SelectView(ReaderView);
    }

    private void DeleteSelected()
    {
        foreach (var m in MessageList.SelectedItems?.OfType<MailMessage>().ToList() ?? [])
        {
            _vm.Delete(m);
        }

        _vm.IsMultiSelect = false;
    }

    private void Reply(bool forward)
    {
        if (_vm.SelectedMessage is not { } m)
        {
            return;
        }

        var quote = $"\n\nOn {m.Received:f}, {m.From.Name} wrote:\n> " + m.Body.Replace("\n", "\n> ");
        OpenCompose(forward
            ? new Draft([], [], "Fwd: " + m.Subject, quote)
            : new Draft([m.From], [], m.Subject.StartsWith("Re:", StringComparison.Ordinal) ? m.Subject : "Re: " + m.Subject, quote));
    }

    private void OpenCompose(Draft? draft = null)
    {
        if (_composeHost?.IsOpen == true)
        {
            return;
        }

        var start = draft ?? _vm.SavedDraft;
        _vm.SavedDraft = null;
        _compose = new ComposeView(_vm, start);
        _compose.Sent += (_, _) =>
        {
            _composeHost?.Close("sent");
            _notifications?.Show(new Notification("sent") { Title = "Message sent", Text = _compose?.Snapshot().Subject ?? string.Empty });
        };
        _compose.Cancelled += (_, _) => _composeHost?.Close("cancel");

        // On a narrow screen compose rises from the bottom in the toaster dialog frame; otherwise it is a sheet.
        PopupBase host = Bounds.Width <= Sliding.MultiViewMinWidth
            ? new Dialog { IsModal = true, Content = _compose }
            : new ComposeSheet { Caption = "New message", Content = _compose };
        host.Closed += (_, e) =>
        {
            // Escape, back or a tap outside keeps what was typed; Send and Cancel do not.
            if (e.Reason != PopupCloseReason.Handle && e.Reason != PopupCloseReason.Code)
            {
                SaveDraft();
            }

            PopupHost.Children.Remove(host);
            _composeHost = null;
            _compose = null;
        };
        _composeHost?.Close();
        PopupHost.Children.Add(host);
        _composeHost = host;
        host.OpenAtCenter();
    }

    /// <summary>Keeps the open draft in memory, as the webOS app did when it went to the background.</summary>
    private void SaveDraft()
    {
        if (_compose?.Snapshot() is { IsEmpty: false } draft)
        {
            _vm.SavedDraft = draft;
        }
    }

    private void ShowNewMail()
    {
        if (_notifications is null || _vm.Messages.Count == 0)
        {
            return;
        }

        var m = _vm.Messages[_nextFake++ % _vm.Messages.Count];
        var n = new Notification("new-mail") { Title = m.From.Name, Text = m.Subject, Icon = SampleAssets.Avatar(m.From.AvatarIndex), Tag = m };
        _notifications.Show(n);
    }
}
