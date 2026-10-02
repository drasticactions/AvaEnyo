using System.Collections.Generic;
using AvaEnyo.Gallery.Pages.Motion;

namespace AvaEnyo.Gallery;

public static partial class GalleryPages
{
    static partial void AddMotion(List<GalleryPage> pages)
    {
        const string g = "Motion";
        pages.Add(new(g, "Easing Curves", "The enyo.easing curves, plotted", () => new EasingCurvesPage(), "src/Motion/EasingCurvesPage.axaml"));
        pages.Add(new(g, "Durations", "Every Enyo duration, played", () => new DurationsPage(), "src/Motion/DurationsPage.axaml"));
        pages.Add(new(g, "Scroll Physics", "The scroll constants as sliders, with a list to fling", () => new ScrollPhysicsPage(), "src/Motion/ScrollPhysicsPage.axaml", Scrolls: false,
            Notes: "Enyo's ScrollStrategy: friction 0.97 per 10 ms step, spring 0.93, half-strength drag past the ends, snap friction 0.9. AvaEnyo steps real time in fixed 10 ms increments, so the motion matches a 50 fps TouchPad at any frame rate."));
        pages.Add(new(g, "Gestures", "Hold, hold pulse, drag start and flick events", () => new GesturesPage(), "src/Motion/GesturesPage.axaml", Scrolls: false,
            Notes: "enyo.gesture: hold after 200 ms, pulses every 100 ms, drag after 4 px (which cancels the hold)."));
    }
}
