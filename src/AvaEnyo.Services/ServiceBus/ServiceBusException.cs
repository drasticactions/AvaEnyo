using System;

namespace AvaEnyo.Services;

/// <summary>A service bus call failed, for example because no service answers the URI.</summary>
public sealed class ServiceBusException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="ServiceBusException"/> class.</summary>
    public ServiceBusException()
    {
    }

    /// <summary>Initializes a new instance with a message.</summary>
    public ServiceBusException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance with a message and the cause.</summary>
    public ServiceBusException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
