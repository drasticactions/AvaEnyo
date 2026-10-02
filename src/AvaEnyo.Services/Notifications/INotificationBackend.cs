namespace AvaEnyo.Services;

/// <summary>
/// A hook that forwards notifications to an operating system notifier. AvaEnyo ships none; set one on
/// <see cref="InAppNotificationService.Backend"/>.
/// </summary>
public interface INotificationBackend
{
    /// <summary>A notification was shown.</summary>
    void Show(Notification notification);

    /// <summary>A notification changed.</summary>
    void Update(Notification notification);

    /// <summary>A notification was dismissed.</summary>
    void Dismiss(Notification notification);
}
