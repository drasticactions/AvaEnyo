namespace AvaEnyo.Controls;

/// <summary>Why a popup closed.</summary>
public enum PopupCloseReason
{
    /// <summary>A click outside the popup (Enyo <c>dismissWithClick</c>).</summary>
    Click,

    /// <summary>The Escape key.</summary>
    Escape,

    /// <summary>The system back request.</summary>
    Back,

    /// <summary>A control inside with <c>enyo:Popup.CloseValue</c> was clicked; see <see cref="PopupClosedEventArgs.Value"/>.</summary>
    Handle,

    /// <summary>The popup's placeholder left the visual tree or was hidden.</summary>
    Hidden,

    /// <summary>The window lost activation (Enyo <c>autoHide</c>).</summary>
    AutoClose,

    /// <summary>Code called <see cref="PopupBase.Close(object?)"/>.</summary>
    Code,
}
