namespace AvaEnyo.Controls;

/// <summary>Where <see cref="PopupBase.OpenAroundControl"/> places a popup relative to its control.</summary>
public enum PopupAlignment
{
    /// <summary>Below the control, left edges aligned; above it when there is no room below.</summary>
    Below,

    /// <summary>Above the control, left edges aligned; below it when there is no room above.</summary>
    Above,

    /// <summary>Centered over the control.</summary>
    Center,

    /// <summary>Right of the control, top edges aligned.</summary>
    Right,

    /// <summary>Left of the control, top edges aligned.</summary>
    Left,
}
