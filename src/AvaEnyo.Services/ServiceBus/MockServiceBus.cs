using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace AvaEnyo.Services;

/// <summary>
/// A test double of <see cref="IServiceBus"/> (Enyo's <c>MockPalmService</c>): routes URIs to handlers, fixed JSON
/// replies or JSON files, and logs every call.
/// </summary>
public sealed class MockServiceBus : InProcessServiceBus
{
    private readonly List<(string Uri, JsonNode? Parameters)> _log = new();

    /// <summary>The calls made, in order.</summary>
    public IReadOnlyList<(string Uri, JsonNode? Parameters)> Calls => _log;

    /// <summary>Answers <paramref name="uri"/> with the JSON text <paramref name="json"/>.</summary>
    public void RegisterJson(string uri, string json) => Register(uri, _ => JsonNode.Parse(json));

    /// <summary>Answers <paramref name="uri"/> with the JSON in the file at <paramref name="path"/>, read on each call.</summary>
    public void RegisterFile(string uri, string path) =>
        Register(uri, async (_, ct) => JsonNode.Parse(await File.ReadAllTextAsync(path, ct).ConfigureAwait(false)));

    /// <summary>Answers <paramref name="uri"/> with a delay first, to show progress in a demo.</summary>
    public void RegisterDelayed(string uri, TimeSpan delay, Func<JsonNode?, JsonNode?> handler) =>
        Register(uri, async (p, ct) =>
        {
            await Task.Delay(delay, ct).ConfigureAwait(false);
            return handler(p);
        });

    /// <inheritdoc/>
    public override Task<JsonNode?> CallAsync(string uri, JsonNode? parameters, CancellationToken cancellationToken = default)
    {
        _log.Add((uri, parameters));
        return base.CallAsync(uri, parameters, cancellationToken);
    }

    /// <inheritdoc/>
    protected override IAsyncEnumerable<JsonNode> Open(string uri, JsonNode? parameters, CancellationToken cancellationToken)
    {
        _log.Add((uri, parameters));
        return base.Open(uri, parameters, cancellationToken);
    }
}
