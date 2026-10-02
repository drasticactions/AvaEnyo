using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace AvaEnyo.Services;

/// <summary>
/// The default <see cref="IWebService"/> over a shared <see cref="HttpClient"/>. Configure it in code or declare it
/// in XAML resources and handle <see cref="Succeeded"/> and <see cref="Failed"/>, as Enyo apps declared a
/// <c>WebService</c> component with <c>onSuccess</c> and <c>onFailure</c>.
/// </summary>
public class WebService : IWebService
{
    private static readonly Lazy<HttpClient> s_shared = new(() => new HttpClient { Timeout = System.Threading.Timeout.InfiniteTimeSpan });
    private readonly HttpClient _client;

    /// <summary>Initializes a new instance that uses the shared client.</summary>
    public WebService()
        : this(s_shared.Value)
    {
    }

    /// <summary>Initializes a new instance that uses <paramref name="client"/>, for tests or custom handlers.</summary>
    public WebService(HttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <summary>The request URL.</summary>
    public Uri? Url { get; set; }

    /// <summary>The HTTP method: GET by default.</summary>
    public string Method { get; set; } = "GET";

    /// <summary>Extra request headers.</summary>
    public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>The content type of a request body: <c>application/json</c> by default.</summary>
    public string ContentType { get; set; } = "application/json";

    /// <summary>How the response body is read.</summary>
    public WebServiceHandleAs HandleAs { get; set; }

    /// <summary>The time a request may take before it fails: 30 s by default.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Raised when a request returned a 2xx status.</summary>
    public event EventHandler<WebServiceResult>? Succeeded;

    /// <summary>Raised when a request failed: a non-2xx status, a timeout or a network error.</summary>
    public event EventHandler<WebServiceFailedEventArgs>? Failed;

    /// <inheritdoc/>
    public async Task<WebServiceResult> SendAsync(string? body = null, CancellationToken cancellationToken = default)
    {
        if (Url is null)
        {
            throw new InvalidOperationException("WebService.Url is not set.");
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(Timeout);
        try
        {
            using var request = new HttpRequestMessage(new HttpMethod(Method), Url);
            foreach (var (name, value) in Headers)
            {
                request.Headers.TryAddWithoutValidation(name, value);
            }

            if (body is not null)
            {
                request.Content = new StringContent(body, Encoding.UTF8, ContentType);
            }

            using var response = await _client.SendAsync(request, timeout.Token).ConfigureAwait(false);
            var text = await response.Content.ReadAsStringAsync(timeout.Token).ConfigureAwait(false);
            var result = new WebServiceResult(response.StatusCode, text, ParseJson(text), ParseXml(text));
            if (result.IsSuccess)
            {
                Succeeded?.Invoke(this, result);
            }
            else
            {
                Failed?.Invoke(this, new WebServiceFailedEventArgs(result, null));
            }

            return result;
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            var timedOut = new TimeoutException($"The request to {Url} took longer than {Timeout}.", ex);
            Failed?.Invoke(this, new WebServiceFailedEventArgs(null, timedOut));
            throw timedOut;
        }
        catch (HttpRequestException ex)
        {
            Failed?.Invoke(this, new WebServiceFailedEventArgs(null, ex));
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<T?> SendAsync<T>(JsonTypeInfo<T> typeInfo, string? body = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(typeInfo);
        var result = await SendAsync(body, cancellationToken).ConfigureAwait(false);
        return result.IsSuccess && result.Text.Length > 0 ? JsonSerializer.Deserialize(result.Text, typeInfo) : default;
    }

    private JsonNode? ParseJson(string text)
    {
        if (HandleAs != WebServiceHandleAs.Json || string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        try
        {
            return JsonNode.Parse(text);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private XDocument? ParseXml(string text)
    {
        if (HandleAs != WebServiceHandleAs.Xml || string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        try
        {
            return XDocument.Parse(text);
        }
        catch (XmlException)
        {
            return null;
        }
    }
}
