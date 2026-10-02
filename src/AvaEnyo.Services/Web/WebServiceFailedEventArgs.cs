using System;

namespace AvaEnyo.Services;

/// <summary>Data of <see cref="WebService.Failed"/>: a failed status or an exception (timeout, network).</summary>
public sealed class WebServiceFailedEventArgs : EventArgs
{
    /// <summary>Initializes a new instance of the <see cref="WebServiceFailedEventArgs"/> class.</summary>
    public WebServiceFailedEventArgs(WebServiceResult? result, Exception? exception)
    {
        Result = result;
        Exception = exception;
    }

    /// <summary>The response, when the server answered with a failure status.</summary>
    public WebServiceResult? Result { get; }

    /// <summary>The exception, when the request did not complete.</summary>
    public Exception? Exception { get; }
}
