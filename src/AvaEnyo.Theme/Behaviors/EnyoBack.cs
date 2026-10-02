using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AvaEnyo;

/// <summary>
/// The Enyo <c>back</c> event as an Avalonia routed event. Escape (when no focused control handled it) and
/// <see cref="TopLevel.BackRequested"/> (the system back button, Alt+Left, the mouse back button) start the back chain:
/// open popups close first, newest first; then <see cref="BackRequestedEvent"/> bubbles from the focused element, so a
/// Pane, SlidingPane or NavigationPage on the way can go back; then it reaches the application.
/// </summary>
public static class EnyoBack
{
    private static readonly List<Func<TopLevel, BackSource, bool>> s_interceptors = new();

    /// <summary>Defines the <c>BackRequested</c> routed event (bubbles).</summary>
    public static readonly RoutedEvent<RoutedEventArgs> BackRequestedEvent =
        RoutedEvent.Register<RoutedEventArgs>("BackRequested", RoutingStrategies.Bubble, typeof(EnyoBack));

    /// <summary>Defines the <c>IsBackTarget</c> attached property.</summary>
    public static readonly AttachedProperty<bool> IsBackTargetProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>("IsBackTarget", typeof(EnyoBack));

    /// <summary>Gets <c>IsBackTarget</c>.</summary>
    public static bool GetIsBackTarget(Control element) => element.GetValue(IsBackTargetProperty);

    /// <summary>
    /// Sets <c>IsBackTarget</c>: marks a control that handles back (a pane, a navigation host). When nothing has
    /// focus, the back event starts from the deepest visible back target, so it still reaches the innermost pane.
    /// </summary>
    public static void SetIsBackTarget(Control element, bool value) => element.SetValue(IsBackTargetProperty, value);

    /// <summary>Adds a <c>BackRequested</c> handler to an element.</summary>
    public static void AddBackRequestedHandler(Interactive element, EventHandler<RoutedEventArgs> handler) =>
        element.AddHandler(BackRequestedEvent, handler);

    /// <summary>Removes a <c>BackRequested</c> handler from an element.</summary>
    public static void RemoveBackRequestedHandler(Interactive element, EventHandler<RoutedEventArgs> handler) =>
        element.RemoveHandler(BackRequestedEvent, handler);

    /// <summary>
    /// Runs the back chain for <paramref name="topLevel"/>, as Escape does. Returns true when something handled it.
    /// </summary>
    public static bool RequestBack(TopLevel topLevel) => RequestBack(topLevel, BackSource.Code);

    /// <summary>Runs the back chain for <paramref name="topLevel"/>, telling interceptors what started it.</summary>
    public static bool RequestBack(TopLevel topLevel, BackSource source)
    {
        for (var i = s_interceptors.Count - 1; i >= 0; i--)
        {
            if (s_interceptors[i](topLevel, source))
            {
                return true;
            }
        }

        var origin = topLevel.FocusManager?.GetFocusedElement() as Interactive;
        if (origin is Visual v && TopLevel.GetTopLevel(v) != topLevel)
        {
            origin = null;
        }

        return RaiseFrom(origin ?? DeepestBackTarget(topLevel) ?? topLevel);
    }

    /// <summary>The visible back target with the most ancestors, or null.</summary>
    internal static Interactive? DeepestBackTarget(TopLevel topLevel)
    {
        Control? best = null;
        var bestDepth = -1;
        Walk(topLevel, 0);
        return best;

        void Walk(Visual visual, int depth)
        {
            foreach (var child in visual.GetVisualChildren())
            {
                if (!child.IsVisible)
                {
                    continue;
                }

                if (child is Control c && GetIsBackTarget(c) && depth >= bestDepth)
                {
                    best = c;
                    bestDepth = depth;
                }

                Walk(child, depth + 1);
            }
        }
    }

    /// <summary>Raises <see cref="BackRequestedEvent"/> from <paramref name="source"/>. Returns true when handled.</summary>
    public static bool RaiseFrom(Interactive source)
    {
        var args = new RoutedEventArgs(BackRequestedEvent, source);
        source.RaiseEvent(args);
        return args.Handled;
    }

    /// <summary>
    /// Registers a handler that runs before the routed event, newest first. Popups use it to close on back. Return
    /// true to stop the chain.
    /// </summary>
    internal static IDisposable AddInterceptor(Func<TopLevel, BackSource, bool> interceptor)
    {
        s_interceptors.Add(interceptor);
        return new Unregister(interceptor);
    }

    internal static void EnsureRegistered()
    {
        InputElement.KeyDownEvent.AddClassHandler<TopLevel>(OnTopLevelKeyDown);
        TopLevel.BackRequestedEvent.AddClassHandler<TopLevel>(OnTopLevelBackRequested);
    }

    private static void OnTopLevelKeyDown(TopLevel top, KeyEventArgs e)
    {
        if (e.Handled || e.Key != Key.Escape || e.KeyModifiers != KeyModifiers.None)
        {
            return;
        }

        e.Handled = RequestBack(top, BackSource.Escape);
    }

    private static void OnTopLevelBackRequested(TopLevel top, RoutedEventArgs e)
    {
        if (e.Handled || e.Source != top)
        {
            return;
        }

        e.Handled = RequestBack(top, BackSource.System);
    }

    private sealed class Unregister(Func<TopLevel, BackSource, bool> interceptor) : IDisposable
    {
        public void Dispose() => s_interceptors.Remove(interceptor);
    }
}
