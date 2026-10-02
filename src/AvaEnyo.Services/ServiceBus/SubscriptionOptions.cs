using System;

namespace AvaEnyo.Services;

/// <summary>How a service bus subscription behaves when it ends (Enyo <c>resubscribe</c>).</summary>
public sealed class SubscriptionOptions
{
    /// <summary>True: a subscription that ends or fails starts again after <see cref="ResubscribeDelay"/>.</summary>
    public bool Resubscribe { get; init; }

    /// <summary>The wait before resubscribing: 10 s by default, as in Enyo.</summary>
    public TimeSpan ResubscribeDelay { get; init; } = TimeSpan.FromSeconds(10);
}
