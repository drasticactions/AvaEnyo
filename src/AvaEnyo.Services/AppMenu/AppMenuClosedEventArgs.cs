using System;
using AvaEnyo.Controls;

namespace AvaEnyo.Services;

/// <summary>Data of <see cref="IAppMenuHost.Closed"/>.</summary>
public sealed class AppMenuClosedEventArgs : EventArgs
{
    /// <summary>Initializes a new instance of the <see cref="AppMenuClosedEventArgs"/> class.</summary>
    public AppMenuClosedEventArgs(PopupCloseReason reason, object? value)
    {
        Reason = reason;
        Value = value;
    }

    /// <summary>Why the menu closed.</summary>
    public PopupCloseReason Reason { get; }

    /// <summary>The value of the item that closed the menu, or null.</summary>
    public object? Value { get; }
}
