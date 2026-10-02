using System;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace AvaEnyo.Controls;

/// <summary>
/// The Enyo <c>SearchInput</c> as a behavior of a <see cref="TextBox"/> with the <c>search</c> class. Typing raises
/// <c>Committed</c> once the text has been still for <c>InputDelay</c> (250 ms); Enter raises it at once. Clearing
/// the text (the clear button, or deleting it all) raises <c>Cancelled</c>.
/// </summary>
public static class Search
{
    private static readonly ConditionalWeakTable<TextBox, State> s_states = new();

    /// <summary>Defines the <c>InputDelay</c> attached property, in milliseconds.</summary>
    public static readonly AttachedProperty<double> InputDelayProperty =
        AvaloniaProperty.RegisterAttached<TextBox, double>("InputDelay", typeof(Search), 250d);

    /// <summary>Defines the <c>Committed</c> attached routed event.</summary>
    public static readonly RoutedEvent<SearchEventArgs> CommittedEvent =
        RoutedEvent.Register<SearchEventArgs>("Committed", RoutingStrategies.Bubble, typeof(Search));

    /// <summary>Defines the <c>Cancelled</c> attached routed event.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> CancelledEvent =
        RoutedEvent.Register<RoutedEventArgs>("Cancelled", RoutingStrategies.Bubble, typeof(Search));

    static Search()
    {
        TextBox.TextProperty.Changed.AddClassHandler<TextBox>(OnTextChanged);
        InputElement.KeyDownEvent.AddClassHandler<TextBox>(OnKeyDown, RoutingStrategies.Tunnel);
    }

    /// <summary>Gets <c>InputDelay</c>.</summary>
    public static double GetInputDelay(TextBox element) => element.GetValue(InputDelayProperty);

    /// <summary>Sets <c>InputDelay</c>: how long the text must be still before <c>Committed</c>.</summary>
    public static void SetInputDelay(TextBox element, double value) => element.SetValue(InputDelayProperty, value);

    /// <summary>Adds a <c>Committed</c> handler.</summary>
    public static void AddCommittedHandler(Interactive element, EventHandler<SearchEventArgs> handler) => element.AddHandler(CommittedEvent, handler);

    /// <summary>Removes a <c>Committed</c> handler.</summary>
    public static void RemoveCommittedHandler(Interactive element, EventHandler<SearchEventArgs> handler) => element.RemoveHandler(CommittedEvent, handler);

    /// <summary>Adds a <c>Cancelled</c> handler.</summary>
    public static void AddCancelledHandler(Interactive element, EventHandler<RoutedEventArgs> handler) => element.AddHandler(CancelledEvent, handler);

    /// <summary>Removes a <c>Cancelled</c> handler.</summary>
    public static void RemoveCancelledHandler(Interactive element, EventHandler<RoutedEventArgs> handler) => element.RemoveHandler(CancelledEvent, handler);

    /// <summary>Makes sure the class handlers are registered.</summary>
    public static void EnsureRegistered()
    {
    }

    private static bool IsSearch(TextBox box) => box.Classes.Contains("search");

    private static void OnTextChanged(TextBox box, AvaloniaPropertyChangedEventArgs e)
    {
        if (!IsSearch(box))
        {
            return;
        }

        var state = s_states.GetOrCreateValue(box);
        state.Timer?.Stop();
        var text = e.GetNewValue<string?>() ?? string.Empty;
        if (text.Length == 0)
        {
            if (!string.IsNullOrEmpty(e.GetOldValue<string?>()))
            {
                box.RaiseEvent(new RoutedEventArgs(CancelledEvent, box));
            }

            return;
        }

        var delay = GetInputDelay(box);
        if (delay <= 0)
        {
            Commit(box);
            return;
        }

        state.Timer = new DispatcherTimer(TimeSpan.FromMilliseconds(delay), DispatcherPriority.Input, (_, _) =>
        {
            state.Timer?.Stop();
            state.Timer = null;
            Commit(box);
        });
        state.Timer.Start();
    }

    private static void OnKeyDown(TextBox box, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && IsSearch(box) && !box.AcceptsReturn)
        {
            if (s_states.TryGetValue(box, out var state))
            {
                state.Timer?.Stop();
                state.Timer = null;
            }

            Commit(box);
        }
    }

    private static void Commit(TextBox box) => box.RaiseEvent(new SearchEventArgs(CommittedEvent, box, box.Text ?? string.Empty));

    private sealed class State
    {
        public DispatcherTimer? Timer;
    }
}
