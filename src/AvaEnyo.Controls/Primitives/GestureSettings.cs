namespace AvaEnyo.Controls.Primitives;

/// <summary>Switches that restore exact Enyo input timing where AvaEnyo chose a different default.</summary>
public static class GestureSettings
{
    /// <summary>
    /// False (default): an <see cref="Item"/> raises its click at once. True restores Enyo's 100 ms click delay.
    /// </summary>
    public static bool DelayItemClick
    {
        get => ThemeSettings.DelayItemClick;
        set => ThemeSettings.DelayItemClick = value;
    }

    /// <summary>True (default): a CheckBox toggles on press, as in Enyo. False: on release.</summary>
    public static bool CheckBoxTogglesOnPress
    {
        get => ThemeSettings.CheckBoxTogglesOnPress;
        set => ThemeSettings.CheckBoxTogglesOnPress = value;
    }
}
