using System.ComponentModel;
using AvaEnyo.Controls;

namespace AvaEnyo.Services;

/// <summary>Data of <see cref="IAppMenuHost.Opening"/>. Set <c>Cancel</c> to keep the menu closed.</summary>
public sealed class AppMenuOpeningEventArgs : CancelEventArgs
{
    /// <summary>Initializes a new instance of the <see cref="AppMenuOpeningEventArgs"/> class.</summary>
    public AppMenuOpeningEventArgs(AppMenu? menu)
    {
        Menu = menu;
    }

    /// <summary>The menu about to open, to add items to. Null for a test host.</summary>
    public AppMenu? Menu { get; }
}
