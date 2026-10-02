using System;

namespace AvaEnyo.Services;

/// <summary>Data of <see cref="IApplicationEvents.Relaunched"/> and <see cref="IApplicationEvents.WindowParamsChanged"/>.</summary>
public sealed class RelaunchEventArgs : EventArgs
{
    /// <summary>Initializes a new instance of the <see cref="RelaunchEventArgs"/> class.</summary>
    public RelaunchEventArgs(object? parameters)
    {
        Parameters = parameters;
    }

    /// <summary>The new launch parameters, for example command-line arguments.</summary>
    public object? Parameters { get; }
}
