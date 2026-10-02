using System;
using System.ComponentModel;

namespace AvaEnyo.Samples;

/// <summary>A fake mail message. Read and flag state notify, so lists update when a message is opened.</summary>
public sealed class MailMessage : INotifyPropertyChanged
{
    private bool _isUnread;
    private bool _isFlagged;

    public event PropertyChangedEventHandler? PropertyChanged;

    public required int Id { get; init; }

    public required string FolderId { get; init; }

    public required Contact From { get; init; }

    public required string Subject { get; init; }

    public required string Body { get; init; }

    public required DateTime Received { get; init; }

    public bool IsUnread
    {
        get => _isUnread;
        set
        {
            if (_isUnread != value)
            {
                _isUnread = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsUnread)));
            }
        }
    }

    public bool IsFlagged
    {
        get => _isFlagged;
        set
        {
            if (_isFlagged != value)
            {
                _isFlagged = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsFlagged)));
            }
        }
    }

    public string Preview => Body.Length > 90 ? Body[..90].Replace('\n', ' ') + "…" : Body.Replace('\n', ' ');

    public string DayCaption => Received.Date == SampleData.Today ? "Today"
        : Received.Date == SampleData.Today.AddDays(-1) ? "Yesterday"
        : Received.ToString("dddd, MMMM d", System.Globalization.CultureInfo.CurrentCulture);

    public string TimeText => Received.ToString("t", System.Globalization.CultureInfo.CurrentCulture);
}
