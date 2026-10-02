using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace AvaEnyo.Services;

/// <summary>A test double of <see cref="IWebService"/> that returns a fixed response and records the bodies sent.</summary>
public sealed class TestWebService : IWebService
{
    private readonly List<string?> _bodies = new();

    /// <summary>The status to answer with: 200 by default.</summary>
    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

    /// <summary>The body to answer with.</summary>
    public string ResponseText { get; set; } = string.Empty;

    /// <summary>The bodies sent, in order.</summary>
    public IReadOnlyList<string?> Bodies => _bodies;

    /// <inheritdoc/>
    public Task<WebServiceResult> SendAsync(string? body = null, CancellationToken cancellationToken = default)
    {
        _bodies.Add(body);
        JsonNode? json = null;
        try
        {
            json = ResponseText.Length > 0 ? JsonNode.Parse(ResponseText) : null;
        }
        catch (JsonException)
        {
        }

        return Task.FromResult(new WebServiceResult(StatusCode, ResponseText, json, null));
    }

    /// <inheritdoc/>
    public async Task<T?> SendAsync<T>(JsonTypeInfo<T> typeInfo, string? body = null, CancellationToken cancellationToken = default)
    {
        var result = await SendAsync(body, cancellationToken).ConfigureAwait(false);
        return result.IsSuccess && result.Text.Length > 0 ? JsonSerializer.Deserialize(result.Text, typeInfo) : default;
    }
}
