using System;
using System.Net;
using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace AvaEnyo.Services;

/// <summary>The response of a <see cref="WebService"/> request.</summary>
public sealed class WebServiceResult : EventArgs
{
    /// <summary>Initializes a new instance of the <see cref="WebServiceResult"/> class.</summary>
    public WebServiceResult(HttpStatusCode statusCode, string text, JsonNode? json, XDocument? xml)
    {
        StatusCode = statusCode;
        Text = text;
        Json = json;
        Xml = xml;
    }

    /// <summary>The HTTP status.</summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>True for a 2xx status.</summary>
    public bool IsSuccess => (int)StatusCode is >= 200 and < 300;

    /// <summary>The body as text.</summary>
    public string Text { get; }

    /// <summary>The body as JSON, when read as JSON and parsable.</summary>
    public JsonNode? Json { get; }

    /// <summary>The body as XML, when read as XML and parsable.</summary>
    public XDocument? Xml { get; }
}
