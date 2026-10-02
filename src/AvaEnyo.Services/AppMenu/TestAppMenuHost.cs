using System;
using AvaEnyo.Controls;

namespace AvaEnyo.Services;

/// <summary>A test double of <see cref="IAppMenuHost"/> that only records state and calls.</summary>
public sealed class TestAppMenuHost : IAppMenuHost
{
    /// <summary>How many times the menu was opened.</summary>
    public int OpenCount { get; private set; }

    /// <inheritdoc/>
    public bool IsOpen { get; private set; }

    /// <inheritdoc/>
    public event EventHandler<AppMenuOpeningEventArgs>? Opening;

    /// <inheritdoc/>
    public event EventHandler<AppMenuClosedEventArgs>? Closed;

    /// <inheritdoc/>
    public void Open()
    {
        var args = new AppMenuOpeningEventArgs(null);
        Opening?.Invoke(this, args);
        if (!args.Cancel)
        {
            IsOpen = true;
            OpenCount++;
        }
    }

    /// <inheritdoc/>
    public void Close() => CloseWith(PopupCloseReason.Code, null);

    /// <inheritdoc/>
    public void Toggle()
    {
        if (IsOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    /// <summary>Closes the menu as if an item with <paramref name="value"/> was clicked.</summary>
    public void CloseWith(PopupCloseReason reason, object? value)
    {
        if (!IsOpen)
        {
            return;
        }

        IsOpen = false;
        Closed?.Invoke(this, new AppMenuClosedEventArgs(reason, value));
    }
}
