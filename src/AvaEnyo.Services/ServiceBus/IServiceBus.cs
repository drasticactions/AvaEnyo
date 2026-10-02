using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace AvaEnyo.Services;

/// <summary>
/// Calls services by URI with JSON parameters, in the shape of webOS's <c>palm://</c> service bus
/// (<c>enyo.PalmService</c>).
/// </summary>
public interface IServiceBus
{
    /// <summary>Calls <paramref name="uri"/> once and returns its JSON reply.</summary>
    Task<JsonNode?> CallAsync(string uri, JsonNode? parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribes to <paramref name="uri"/> (Enyo <c>subscribe: true</c>) and yields each reply until
    /// <paramref name="cancellationToken"/> is canceled. With <see cref="SubscriptionOptions.Resubscribe"/>, a
    /// subscription that ends or fails starts again after <see cref="SubscriptionOptions.ResubscribeDelay"/>.
    /// </summary>
    IAsyncEnumerable<JsonNode> SubscribeAsync(string uri, JsonNode? parameters, CancellationToken cancellationToken = default, SubscriptionOptions? options = null);
}
