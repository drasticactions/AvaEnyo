using System;
using System.Collections.Generic;

namespace AvaEnyo.Services;

/// <summary>Shows webOS dashboard notifications.</summary>
public interface INotificationService
{
    /// <summary>The notifications shown, oldest first.</summary>
    IReadOnlyList<Notification> Notifications { get; }

    /// <summary>Raised when a notification is dismissed (by the user or by <see cref="Dismiss"/>).</summary>
    event EventHandler<NotificationEventArgs>? Dismissed;

    /// <summary>Raised when the user taps a notification.</summary>
    event EventHandler<NotificationEventArgs>? Activated;

    /// <summary>Shows a notification; showing one with an existing <see cref="Notification.Id"/> updates it.</summary>
    void Show(Notification notification);

    /// <summary>Redraws a shown notification after its properties changed.</summary>
    void Update(Notification notification);

    /// <summary>Removes the notification with <paramref name="id"/>.</summary>
    void Dismiss(string id);
}
