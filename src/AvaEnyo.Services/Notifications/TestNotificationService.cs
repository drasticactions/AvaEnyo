using System;
using System.Collections.Generic;

namespace AvaEnyo.Services;

/// <summary>A test double of <see cref="INotificationService"/> that keeps notifications in a list.</summary>
public sealed class TestNotificationService : INotificationService
{
    private readonly List<Notification> _notifications = new();

    /// <inheritdoc/>
    public IReadOnlyList<Notification> Notifications => _notifications;

    /// <summary>How many times <see cref="Update"/> was called.</summary>
    public int UpdateCount { get; private set; }

    /// <inheritdoc/>
    public event EventHandler<NotificationEventArgs>? Dismissed;

    /// <inheritdoc/>
    public event EventHandler<NotificationEventArgs>? Activated;

    /// <inheritdoc/>
    public void Show(Notification notification)
    {
        if (!_notifications.Contains(notification))
        {
            _notifications.RemoveAll(n => n.Id == notification.Id);
            _notifications.Add(notification);
        }
    }

    /// <inheritdoc/>
    public void Update(Notification notification) => UpdateCount++;

    /// <inheritdoc/>
    public void Dismiss(string id)
    {
        var n = _notifications.Find(x => x.Id == id);
        if (n is not null)
        {
            _notifications.Remove(n);
            Dismissed?.Invoke(this, new NotificationEventArgs(n));
        }
    }

    /// <summary>Acts as if the user tapped <paramref name="notification"/>.</summary>
    public void Activate(Notification notification) => Activated?.Invoke(this, new NotificationEventArgs(notification));
}
