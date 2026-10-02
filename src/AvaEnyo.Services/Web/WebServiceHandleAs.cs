namespace AvaEnyo.Services;

/// <summary>How a <see cref="WebService"/> reads a response body (Enyo <c>handleAs</c>).</summary>
public enum WebServiceHandleAs
{
    /// <summary>As JSON (the default), in <see cref="WebServiceResult.Json"/>.</summary>
    Json,

    /// <summary>As text only.</summary>
    Text,

    /// <summary>As XML, in <see cref="WebServiceResult.Xml"/>.</summary>
    Xml,
}
