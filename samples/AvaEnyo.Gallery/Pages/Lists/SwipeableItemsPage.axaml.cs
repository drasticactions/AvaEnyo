using System.Collections.ObjectModel;
using Avalonia.Controls;
using AvaEnyo.Controls;
using AvaEnyo.Samples;

namespace AvaEnyo.Gallery.Pages.Lists;

public partial class SwipeableItemsPage : UserControl
{
    private readonly ObservableCollection<MailMessage> _messages = new();

    public SwipeableItemsPage()
    {
        InitializeComponent();
        Reset();
        List.ItemsSource = _messages;
        List.DividerKeySelector = o => (o as MailMessage)?.DayCaption;
        ResetButton.Click += (_, _) => Reset();
        ConfirmBox.IsCheckedChanged += (_, _) => Apply();
        List.ContainerPrepared += (_, e) => Apply(e.Container);
        List.AddHandler(SwipeableItem.ConfirmedEvent, (_, e) =>
        {
            if (e.Source is SwipeableItem { DataContext: MailMessage m })
            {
                _messages.Remove(m);
                Log.Text = $"Deleted \"{m.Subject}\"";
            }
        });
        List.AddHandler(SwipeableItem.CancelledEvent, (_, _) => Log.Text = "Cancelled");
    }

    private void Reset()
    {
        _messages.Clear();
        foreach (var m in SampleData.MessagesIn("inbox"))
        {
            _messages.Add(m);
        }
    }

    private void Apply()
    {
        foreach (var c in List.GetRealizedContainers())
        {
            Apply(c);
        }
    }

    private void Apply(Control c)
    {
        if (c is SwipeableItem row)
        {
            row.ConfirmRequired = ConfirmBox.IsChecked == true;
        }
    }
}
