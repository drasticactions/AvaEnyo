using Avalonia.Controls;

namespace AvaEnyo.Controls;

/// <summary>
/// The 46×54 drag nub drawn from <c>grabbutton.png</c> (<c>enyo.GrabButton</c>). It is a sliding handle: dragging it
/// moves its <see cref="SlidingView"/>, tapping it toggles the view.
/// </summary>
public class GrabButton : Button
{
    static GrabButton()
    {
        Sliding.IsHandleProperty.OverrideDefaultValue<GrabButton>(true);
    }
}
