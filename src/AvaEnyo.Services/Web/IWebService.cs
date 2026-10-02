using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace AvaEnyo.Services;

/// <summary>Sends one configured HTTP request (Enyo <c>enyo.WebService</c>).</summary>
public interface IWebService
{
    /// <summary>Sends the request with an optional text body and returns the response.</summary>
    Task<WebServiceResult> SendAsync(string? body = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends the request and deserializes a JSON response as <typeparamref name="T"/> with source-generated
    /// metadata, so no reflection serialization is used. Returns default when the status fails.
    /// </summary>
    Task<T?> SendAsync<T>(JsonTypeInfo<T> typeInfo, string? body = null, CancellationToken cancellationToken = default);
}
