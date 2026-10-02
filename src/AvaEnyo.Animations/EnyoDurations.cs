using System;

namespace AvaEnyo.Animations;

/// <summary>
/// The animation durations of Enyo 1.0 and the Onyx CSS, in milliseconds. The theme emits the same values as
/// <c>EnyoDuration*</c> resources. The fields are settable so an app can tune them.
/// </summary>
public static class EnyoDurations
{
    /// <summary>SlidingPane selection slide: 700 ms, cubic ease-out.</summary>
    public static double SlidingPane = 700;

    /// <summary>Drawer open: 250 ms, cubic ease-out.</summary>
    public static double DrawerOpen = 250;

    /// <summary>Drawer close: 100 ms, cubic ease-out.</summary>
    public static double DrawerClose = 100;

    /// <summary>Toaster fly-in and fly-out: 350 ms, cubic ease-out.</summary>
    public static double Toaster = 350;

    /// <summary>Animated progress bar value change: 350 ms, cubic ease-out.</summary>
    public static double Progress = 350;

    /// <summary>Slider thumb move after a tap on the track: 350 ms, cubic ease-out.</summary>
    public static double SliderTap = 350;

    /// <summary>Pane cross-fade: 300 ms, cubic ease-out.</summary>
    public static double PaneFade = 300;

    /// <summary>Pane left-right fly-in: 300 ms, cubic ease-out.</summary>
    public static double PaneFlyIn = 300;

    /// <summary>ModalDialog opacity transition: 500 ms.</summary>
    public static double ModalDialogFade = 500;

    /// <summary>Scrim opacity transition: 500 ms.</summary>
    public static double ScrimFade = 500;

    /// <summary>Toolbar fade-in after the on-screen keyboard moves: 150 ms, linear.</summary>
    public static double ToolbarFade = 150;

    /// <summary>The delay before the toolbar fades in again: 300 ms.</summary>
    public static double ToolbarFadeDelay = 300;

    /// <summary>One spinner revolution: 1000 ms, linear, repeated forever.</summary>
    public static double Spinner = 1000;

    /// <summary>Double-tap smart zoom on an image: 350 ms, cubic ease-out.</summary>
    public static double SmartZoom = 350;

    /// <summary>The default <c>enyo.Animator</c> duration: 350 ms, cubic ease-out.</summary>
    public static double Default = 350;

    /// <summary>Converts milliseconds to a <see cref="TimeSpan"/>.</summary>
    public static TimeSpan ToTimeSpan(double milliseconds) => TimeSpan.FromMilliseconds(milliseconds);
}
