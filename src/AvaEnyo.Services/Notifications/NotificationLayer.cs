namespace AvaEnyo.Services;

/// <summary>One stacked layer of a <see cref="Notification"/>.</summary>
/// <param name="Title">The layer title.</param>
/// <param name="Text">The layer text.</param>
public sealed record NotificationLayer(string? Title, string? Text);
