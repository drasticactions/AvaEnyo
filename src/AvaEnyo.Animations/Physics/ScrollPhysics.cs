using System;

namespace AvaEnyo.Animations;

/// <summary>
/// One axis of the Enyo 1.0 scroll simulation (<c>enyo.ScrollStrategy</c>): a Verlet integrator with friction,
/// a damped spring past the edges, and resistance when a drag pulls past an edge.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Position"/> is a scroll offset: 0 shows the start of the content and <see cref="Max"/> shows the end.
/// Enyo stored the negated value; the arithmetic is the same.
/// </para>
/// <para>
/// Enyo stepped a fixed 20 ms per frame, so the motion ran slower on a slow device and faster on a fast one.
/// <see cref="Step"/> consumes real elapsed time in fixed 10 ms steps and keeps the remainder, so the motion is the
/// same at any frame rate and matches a 50 fps TouchPad.
/// </para>
/// </remarks>
public class ScrollPhysics
{
    /// <summary>The simulation step: 10 ms (<c>ScrollStrategy.frame</c>).</summary>
    public const double StepMilliseconds = 10;

    /// <summary>Default velocity kept per step: 0.97 (<c>kFrictionDamping</c>). Lower stops sooner.</summary>
    public static double DefaultFriction = 0.97;

    /// <summary>Default speed below which motion stops: 0.01 px per step (<c>kFrictionEpsilon</c>).</summary>
    public static double DefaultFrictionEpsilon = 0.01;

    /// <summary>Default overscroll kept per step while the spring pulls back: 0.93 (<c>kSpringDamping</c>).</summary>
    public static double DefaultSpring = 0.93;

    /// <summary>Default fraction of a drag applied past an edge: 0.5 (<c>kDragDamping</c>).</summary>
    public static double DefaultDragDamping = 0.5;

    /// <summary>Default extra friction while the spring pulls back: 0.9 (<c>kSnapFriction</c>).</summary>
    public static double DefaultSnapFriction = 0.9;

    /// <summary>Default scale from a flick velocity in px/s to a step delta: 0.01 (<c>kFlickScalar</c>).</summary>
    public static double DefaultFlickScalar = 0.01;

    /// <summary>Default distance to an edge at which the spring snaps onto it: 0.5 px.</summary>
    public static double DefaultBoundarySnap = 0.5;

    private double _remainder;
    private double _dragStart;

    /// <summary>Initializes a new instance with the default constants.</summary>
    public ScrollPhysics()
    {
        Friction = DefaultFriction;
        FrictionEpsilon = DefaultFrictionEpsilon;
        Spring = DefaultSpring;
        DragDamping = DefaultDragDamping;
        SnapFriction = DefaultSnapFriction;
        FlickScalar = DefaultFlickScalar;
        BoundarySnap = DefaultBoundarySnap;
    }

    /// <summary>The current position (scroll offset). Outside <see cref="Min"/>..<see cref="Max"/> while overscrolled.</summary>
    public double Position { get; set; }

    /// <summary>The position one step ago. <c>Position − PreviousPosition</c> is the velocity per step.</summary>
    public double PreviousPosition { get; set; }

    /// <summary>The lowest resting position, usually 0.</summary>
    public double Min { get; set; }

    /// <summary>The highest resting position: the extent minus the viewport, never below <see cref="Min"/>.</summary>
    public double Max { get; set; }

    /// <summary>True between <see cref="BeginDrag"/> and <see cref="EndDrag"/>.</summary>
    public bool IsDragging { get; private set; }

    /// <summary>Velocity kept per step.</summary>
    public double Friction { get; set; }

    /// <summary>Speed (px per step) below which motion stops.</summary>
    public double FrictionEpsilon { get; set; }

    /// <summary>Overscroll kept per step while the spring pulls back.</summary>
    public double Spring { get; set; }

    /// <summary>Fraction of a drag applied past an edge.</summary>
    public double DragDamping { get; set; }

    /// <summary>Extra friction while the spring pulls back.</summary>
    public double SnapFriction { get; set; }

    /// <summary>Scale from a velocity in px/s to a step delta.</summary>
    public double FlickScalar { get; set; }

    /// <summary>Distance to an edge at which the spring snaps onto it.</summary>
    public double BoundarySnap { get; set; }

    /// <summary>The velocity in px per 10 ms step.</summary>
    public double Velocity => Position - PreviousPosition;

    /// <summary>True when the position is outside <see cref="Min"/>..<see cref="Max"/>.</summary>
    public bool IsOverscrolled => Position < Min || Position > Max;

    /// <summary>True when the simulation still moves: it has speed or is overscrolled, and no drag holds it.</summary>
    public bool IsMoving => !IsDragging && (Math.Abs(Velocity) > 0 || IsOverscrolled);

    /// <summary>Puts the simulation at rest at <paramref name="position"/>.</summary>
    public void SetPosition(double position)
    {
        Position = position;
        PreviousPosition = position;
        _remainder = 0;
    }

    /// <summary>Stops all motion where it is (Enyo's tap-to-stop). An overscroll stays and springs back on the next step.</summary>
    public void Stop()
    {
        PreviousPosition = Position;
        _remainder = 0;
    }

    /// <summary>Starts a drag at the current position. Any motion stops.</summary>
    public void BeginDrag()
    {
        IsDragging = true;
        _dragStart = Position;
        PreviousPosition = Position;
        _remainder = 0;
    }

    /// <summary>
    /// Moves a drag. <paramref name="delta"/> is the total movement since <see cref="BeginDrag"/>, in scroll-offset
    /// direction (a finger moving down scrolls toward the start, so pass the negated pointer delta). Inside the bounds
    /// the content follows 1:1. Past an edge the excess is multiplied by <see cref="DragDamping"/>.
    /// </summary>
    public void Drag(double delta)
    {
        if (!IsDragging)
        {
            return;
        }

        var p = BoundaryDamping(_dragStart + delta, Min, Max, DragDamping);
        PreviousPosition = p;
        Position = p;
    }

    /// <summary>
    /// Ends a drag and starts the fling. <paramref name="velocity"/> is the release velocity in px/ms, in
    /// scroll-offset direction. It becomes a step delta through <see cref="FlickScalar"/>: px/s × 0.01.
    /// </summary>
    public void EndDrag(double velocity)
    {
        IsDragging = false;
        PreviousPosition = Position - (velocity * 1000 * FlickScalar);
        _remainder = 0;
    }

    /// <summary>
    /// Starts a motion that comes to rest on <paramref name="target"/>. Like Enyo, it sets an initial velocity whose
    /// geometric decay under <see cref="Friction"/> sums to the distance.
    /// </summary>
    public void ScrollTo(double target)
    {
        IsDragging = false;
        Position = PreviousPosition + ((target - PreviousPosition) * (1 - Friction));
        _remainder = 0;
    }

    /// <summary>
    /// Advances the simulation by real <paramref name="elapsed"/> time in fixed 10 ms steps. The remainder is kept for
    /// the next call. Returns true while the motion continues.
    /// </summary>
    public bool Step(TimeSpan elapsed)
    {
        if (IsDragging)
        {
            return true;
        }

        _remainder += Math.Max(0, elapsed.TotalMilliseconds);
        while (_remainder >= StepMilliseconds)
        {
            _remainder -= StepMilliseconds;
            StepOnce();
        }

        if (!IsMoving)
        {
            _remainder = 0;
            return false;
        }

        return true;
    }

    /// <summary>
    /// Runs the simulation until it rests, at most <paramref name="maxSteps"/> steps, then returns the rest position.
    /// Used when animations are off (time scale 0).
    /// </summary>
    public double Settle(int maxSteps = 100_000)
    {
        IsDragging = false;
        for (var i = 0; i < maxSteps && IsMoving; i++)
        {
            StepOnce();
        }

        if (IsOverscrolled)
        {
            SetPosition(Math.Clamp(Position, Min, Math.Max(Min, Max)));
        }

        return Position;
    }

    /// <summary>One 10 ms step: constrain (unless dragging), Verlet, friction.</summary>
    protected virtual void StepOnce()
    {
        if (!IsDragging)
        {
            Constrain();
        }

        var p = Position;
        Position += p - PreviousPosition;
        PreviousPosition = p;

        var dp = Position - PreviousPosition;
        var c = Math.Abs(dp) > FrictionEpsilon ? Friction : 0;
        Position = PreviousPosition + (c * dp);
    }

    /// <summary>The spring: pulls an overscroll back toward the edge and adds snap friction to its velocity.</summary>
    protected void Constrain()
    {
        var p = BoundaryDamping(Position, Min, Max, Spring);
        if (p != Position)
        {
            PreviousPosition = p - ((Position - PreviousPosition) * SnapFriction);
            Position = p;
        }
    }

    /// <summary>
    /// Enyo's one-sided damping: past <paramref name="origin"/> on the side given by <paramref name="sign"/>, the
    /// excess is multiplied by <paramref name="coefficient"/>. Within <see cref="BoundarySnap"/> of the origin the value
    /// snaps onto it.
    /// </summary>
    protected double Damping(double value, double origin, double coefficient, int sign)
    {
        var dv = value - origin;
        if (Math.Abs(dv) < BoundarySnap)
        {
            return origin;
        }

        return value * sign > origin * sign ? (coefficient * dv) + origin : value;
    }

    /// <summary>Damping at both edges: below <paramref name="min"/> and above <paramref name="max"/>.</summary>
    protected double BoundaryDamping(double value, double min, double max, double coefficient)
    {
        // Offsets grow toward the end, so the start edge damps values below it and the end edge values above it.
        return Damping(Damping(value, min, coefficient, -1), Math.Max(min, max), coefficient, 1);
    }
}
