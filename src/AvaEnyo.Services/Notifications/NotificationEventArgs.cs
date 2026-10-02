using System;

namespace AvaEnyo.Services;

/// <summary>Data of <see cref="INotificationService.Dismissed"/> and <see cref="INotificationService.Activated"/>.</summary>
public sealed class NotificationEventArgs : EventArgs
{
    /// <summary>Initializes a new instance of the <see cref="NotificationEventArgs"/> class.</summary>
    public NotificationEventArgs(Notification notification)
    {
        Notification = notification;
    }

    /// <summary>The notification.</summary>
    public Notification Notification { get; }
}
