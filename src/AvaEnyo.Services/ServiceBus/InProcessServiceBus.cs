using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace AvaEnyo.Services;

/// <summary>
/// The default <see cref="IServiceBus"/>: services are handlers registered in the app, routed by URI. A URI is
/// matched exactly, then by its longest registered prefix ending in <c>/</c>.
/// </summary>
public class InProcessServiceBus : IServiceBus
{
    private readonly Dictionary<string, Func<JsonNode?, CancellationToken, Task<JsonNode?>>> _calls = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Func<JsonNode?, CancellationToken, IAsyncEnumerable<JsonNode>>> _subscriptions = new(StringComparer.Ordinal);

    /// <summary>Registers a call handler for <paramref name="uri"/>.</summary>
    public void Register(string uri, Func<JsonNode?, CancellationToken, Task<JsonNode?>> handler) => _calls[uri] = handler;

    /// <summary>Registers a synchronous call handler for <paramref name="uri"/>.</summary>
    public void Register(string uri, Func<JsonNode?, JsonNode?> handler) => _calls[uri] = (p, _) => Task.FromResult(handler(p));

    /// <summary>Registers a subscription source for <paramref name="uri"/>.</summary>
    public void RegisterSubscription(string uri, Func<JsonNode?, CancellationToken, IAsyncEnumerable<JsonNode>> source) => _subscriptions[uri] = source;

    /// <summary>Removes every handler of <paramref name="uri"/>.</summary>
    public void Unregister(string uri)
    {
        _calls.Remove(uri);
        _subscriptions.Remove(uri);
    }

    /// <inheritdoc/>
    public virtual Task<JsonNode?> CallAsync(string uri, JsonNode? parameters, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (Find(_calls, uri) is { } handler)
        {
            return handler(parameters, cancellationToken);
        }

        if (Find(_subscriptions, uri) is not null)
        {
            return FirstAsync(uri, parameters, cancellationToken);
        }

        throw new ServiceBusException($"No service answers '{uri}'.");
    }

    /// <inheritdoc/>
    public virtual async IAsyncEnumerable<JsonNode> SubscribeAsync(string uri, JsonNode? parameters, [EnumeratorCancellation] CancellationToken cancellationToken = default, SubscriptionOptions? options = null)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await using (var e = Open(uri, parameters, cancellationToken).GetAsyncEnumerator(cancellationToken))
            {
                while (true)
                {
                    JsonNode? item;
                    try
                    {
                        if (!await e.MoveNextAsync().ConfigureAwait(false))
                        {
                            break;
                        }

                        item = e.Current;
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        yield break;
                    }
                    catch (Exception) when (options?.Resubscribe == true)
                    {
                        break;
                    }

                    yield return item;
                }
            }

            if (options?.Resubscribe != true)
            {
                yield break;
            }

            try
            {
                await Task.Delay(options.ResubscribeDelay, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                yield break;
            }
        }
    }

    /// <summary>The source of one subscription round. The default uses the registered subscription, else one call.</summary>
    protected virtual IAsyncEnumerable<JsonNode> Open(string uri, JsonNode? parameters, CancellationToken cancellationToken)
    {
        if (Find(_subscriptions, uri) is { } source)
        {
            return source(parameters, cancellationToken);
        }

        if (Find(_calls, uri) is { } handler)
        {
            return Once(handler, parameters, cancellationToken);
        }

        throw new ServiceBusException($"No service answers '{uri}'.");
    }

    private static async IAsyncEnumerable<JsonNode> Once(Func<JsonNode?, CancellationToken, Task<JsonNode?>> handler, JsonNode? parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (await handler(parameters, cancellationToken).ConfigureAwait(false) is { } reply)
        {
            yield return reply;
        }
    }

    private async Task<JsonNode?> FirstAsync(string uri, JsonNode? parameters, CancellationToken cancellationToken)
    {
        await foreach (var item in Open(uri, parameters, cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            return item;
        }

        return null;
    }

    private static T? Find<T>(Dictionary<string, T> map, string uri)
        where T : class
    {
        if (map.TryGetValue(uri, out var exact))
        {
            return exact;
        }

        T? best = null;
        var length = -1;
        foreach (var (key, value) in map)
        {
            if (key.EndsWith('/') && uri.StartsWith(key, StringComparison.Ordinal) && key.Length > length)
            {
                best = value;
                length = key.Length;
            }
        }

        return best;
    }
}
