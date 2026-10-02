namespace AvaEnyo.Animations;

/// <summary>
/// The scroll simulation of Enyo's <c>SnapScroller</c> and <c>BasicCarousel</c>: more friction, so a fling ends
/// quickly, and a target position that a release snaps to with <see cref="ScrollPhysics.ScrollTo"/>.
/// </summary>
public class SnapPhysics : ScrollPhysics
{
    /// <summary>SnapScroller friction: 0.85.</summary>
    public static double SnapScrollerFriction = 0.85;

    /// <summary>Carousel friction: 0.75.</summary>
    public static double CarouselFriction = 0.75;

    /// <summary>Carousel spring: 0.8.</summary>
    public static double CarouselSpring = 0.8;

    /// <summary>Carousel friction epsilon: 0.1.</summary>
    public static double CarouselFrictionEpsilon = 0.1;

    /// <summary>Initializes the physics of a SnapScroller (friction 0.85).</summary>
    public SnapPhysics()
    {
        Friction = SnapScrollerFriction;
    }

    /// <summary>Creates the physics of a Carousel (friction 0.75, spring 0.8, epsilon 0.1).</summary>
    public static SnapPhysics ForCarousel() => new()
    {
        Friction = CarouselFriction,
        Spring = CarouselSpring,
        FrictionEpsilon = CarouselFrictionEpsilon,
    };

    /// <summary>The position the last <see cref="SnapTo"/> aims at, or null.</summary>
    public double? SnapTarget { get; private set; }

    /// <summary>Starts a motion that rests on <paramref name="target"/>, and remembers it.</summary>
    public void SnapTo(double target)
    {
        SnapTarget = target;
        ScrollTo(target);
    }

    /// <inheritdoc/>
    protected override void StepOnce()
    {
        base.StepOnce();

        // The geometric decay lands within the friction epsilon of the target. Put it exactly there, as Enyo's
        // scrollStop did for a remaining distance under 1 px.
        if (SnapTarget is { } t && !IsDragging && System.Math.Abs(Velocity) <= FrictionEpsilon && System.Math.Abs(Position - t) < 1)
        {
            SetPosition(t);
            SnapTarget = null;
        }
    }
}
