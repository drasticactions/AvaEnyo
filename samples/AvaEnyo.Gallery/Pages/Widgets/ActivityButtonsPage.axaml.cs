using System;
using Avalonia.Controls;
using Avalonia.Threading;

namespace AvaEnyo.Gallery.Pages.Widgets;

public partial class ActivityButtonsPage : UserControl
{
    private readonly DispatcherTimer _timer;

    public ActivityButtonsPage()
    {
        InitializeComponent();
        Activity.Click += (_, _) =>
        {
            Activity.IsActive = !Activity.IsActive;
            Activity.Content = Activity.IsActive ? "Working" : "Tap to start";
        };

        // A fake download: the bar fills, then starts over.
        _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(200), DispatcherPriority.Background, (_, _) =>
        {
            Download.Value = Download.Value >= Download.Maximum ? 0 : Download.Value + 2;
        });
        Download.Cancel += (_, _) =>
        {
            _timer.Stop();
            DownloadState.Text = "Canceled at " + Download.Value + " %.";
        };
        AttachedToVisualTree += (_, _) => _timer.Start();
        DetachedFromVisualTree += (_, _) => _timer.Stop();
    }
}
