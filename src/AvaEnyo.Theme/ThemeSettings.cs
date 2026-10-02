namespace AvaEnyo;

/// <summary>
/// Input timings and switches shared by the theme and <c>AvaEnyo.Controls</c>. The defaults are Enyo's
/// (<c>enyo.gesture</c>). Tests set them to make gestures deterministic.
/// </summary>
public static class ThemeSettings
{
    /// <summary>Distance a pointer must move before a drag starts: 4 px (Enyo <c>hysteresis</c>).</summary>
    public static double DragThreshold { get; set; } = 4;

    /// <summary>Time a press must last to become a hold: 200 ms (Enyo <c>holdDelay</c>).</summary>
    public static double HoldDelay { get; set; } = 200;

    /// <summary>Interval of the hold pulse after a hold starts: 100 ms (Enyo <c>pulseInterval</c>).</summary>
    public static double PulseInterval { get; set; } = 100;

    /// <summary>
    /// The off-axis ratio below which a drag is lockable to its main axis: 0.414 (tan 22.5°).
    /// </summary>
    public static double LockRatio { get; set; } = 0.414;

    /// <summary>
    /// True (default): a <c>CheckBox</c> toggles when it is pressed, as Enyo toggled on mousedown. False: it toggles on
    /// release, as stock Avalonia does.
    /// </summary>
    public static bool CheckBoxTogglesOnPress { get; set; } = true;

    /// <summary>
    /// False (default): an item raises its click at once. True: restores Enyo's 100 ms click delay, which let the
    /// held highlight show before the click handler ran.
    /// </summary>
    public static bool DelayItemClick { get; set; }

    /// <summary>The Enyo item click delay used when <see cref="DelayItemClick"/> is true: 100 ms.</summary>
    public static double ItemClickDelay { get; set; } = 100;
}
