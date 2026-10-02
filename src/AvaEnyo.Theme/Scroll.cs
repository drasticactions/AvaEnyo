using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using AvaEnyo.Primitives;

namespace AvaEnyo;

/// <summary>
/// Attached properties of the Enyo scroller, set on a <see cref="ScrollViewer"/> (or an ancestor, for
/// <c>Fades</c>).
/// </summary>
public static class Scroll
{
    /// <summary>Defines the <c>Fades</c> attached property. Inherited; default true.</summary>
    public static readonly AttachedProperty<bool> FadesProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>("Fades", typeof(Scroll), defaultValue: true, inherits: true);

    /// <summary>Defines the <c>Physics</c> attached property.</summary>
    public static readonly AttachedProperty<ScrollPhysicsMode> PhysicsProperty =
        AvaloniaProperty.RegisterAttached<Control, ScrollPhysicsMode>("Physics", typeof(Scroll), ScrollPhysicsMode.Enyo, inherits: true);

    /// <summary>Defines the <c>Lock</c> attached property.</summary>
    public static readonly AttachedProperty<bool> LockProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>("Lock", typeof(Scroll), defaultValue: true);

    /// <summary>Defines the <c>AnimateBringIntoView</c> attached property.</summary>
    public static readonly AttachedProperty<bool> AnimateBringIntoViewProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>("AnimateBringIntoView", typeof(Scroll));

    /// <summary>Gets <c>Fades</c>.</summary>
    public static bool GetFades(Control element) => element.GetValue(FadesProperty);

    /// <summary>
    /// Sets <c>Fades</c>: true (default) shows Enyo's 54 px fade at each edge the content can scroll toward. False is
    /// Enyo's plain <c>BasicScroller</c>.
    /// </summary>
    public static void SetFades(Control element, bool value) => element.SetValue(FadesProperty, value);

    /// <summary>Gets <c>Physics</c>.</summary>
    public static ScrollPhysicsMode GetPhysics(Control element) => element.GetValue(PhysicsProperty);

    /// <summary>Sets <c>Physics</c>: <see cref="ScrollPhysicsMode.Native"/> turns the Enyo drag and fling off.</summary>
    public static void SetPhysics(Control element, ScrollPhysicsMode value) => element.SetValue(PhysicsProperty, value);

    /// <summary>Gets <c>Lock</c>.</summary>
    public static bool GetLock(Control element) => element.GetValue(LockProperty);

    /// <summary>
    /// Sets <c>Lock</c>: when the viewer scrolls on both axes, true (default) locks a drag that is within 22.5° of an
    /// axis to that axis.
    /// </summary>
    public static void SetLock(Control element, bool value) => element.SetValue(LockProperty, value);

    /// <summary>Gets <c>AnimateBringIntoView</c>.</summary>
    public static bool GetAnimateBringIntoView(Control element) => element.GetValue(AnimateBringIntoViewProperty);

    /// <summary>
    /// Sets <c>AnimateBringIntoView</c>: true makes bring-into-view requests glide with the Enyo
    /// <c>scrollTo</c> motion. False (default) jumps, which keeps list virtualization and keyboard focus exact.
    /// </summary>
    public static void SetAnimateBringIntoView(Control element, bool value) => element.SetValue(AnimateBringIntoViewProperty, value);

    /// <summary>
    /// Glides a scroll viewer to <paramref name="offset"/> with Enyo's <c>scrollTo</c> motion (a velocity whose
    /// friction decay ends on the target). Jumps when the viewer does not use Enyo physics.
    /// </summary>
    public static Task ScrollToAsync(this ScrollViewer viewer, Vector offset)
    {
        if (viewer.GetVisualDescendants() is { } d)
        {
            foreach (var v in d)
            {
                if (v is EnyoScrollContentPresenter p && p.TemplatedParent == viewer)
                {
                    return p.ScrollToAsync(offset);
                }
            }
        }

        viewer.Offset = offset;
        return Task.CompletedTask;
    }
}
