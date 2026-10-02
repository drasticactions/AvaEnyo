using System;

namespace AvaEnyo.Services;

/// <summary>
/// Opens and closes a window's application menu (Enyo <c>enyo.appMenu</c>).
/// </summary>
public interface IAppMenuHost
{
    /// <summary>True while the menu is open.</summary>
    bool IsOpen { get; }

    /// <summary>
    /// Raised before the menu opens. Handlers may add or change items, as Enyo apps did in <c>onBeforeOpen</c>, or
    /// cancel the opening.
    /// </summary>
    event EventHandler<AppMenuOpeningEventArgs>? Opening;

    /// <summary>Raised after the menu closed, with the value of the item that closed it, if any.</summary>
    event EventHandler<AppMenuClosedEventArgs>? Closed;

    /// <summary>Opens the menu. Does nothing while a modal popup is open.</summary>
    void Open();

    /// <summary>Closes the menu.</summary>
    void Close();

    /// <summary>Opens the menu when closed, closes it when open.</summary>
    void Toggle();
}
