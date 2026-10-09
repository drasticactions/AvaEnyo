using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Reactive;
using Avalonia.VisualTree;

namespace AvaEnyo;

/// <summary>
/// Enyo window chrome: the TouchPad status bar drawn as the window's title bar. The app name sits at the left (with a
/// ▾ that opens the app menu when the window has one), the window title in the middle, the caption buttons at the
/// right.
/// </summary>
/// <remarks>
/// <para>Set <c>UseEnyoDecorations</c> on a <see cref="Window"/> to draw the bar on Windows, where the system still
/// draws the border, shadow and resize edges, and on Linux. The window's content then starts below the bar. macOS keeps
/// its native title bar.</para>
/// <para>Linux draws the same bar without the property whenever the compositor asks the app to draw its own
/// decorations.</para>
/// </remarks>
public static class EnyoWindow
{
    private static readonly ConditionalWeakTable<WindowDrawnDecorations, List<IDisposable>> s_mirrors = new();

    /// <summary>Defines the <c>UseEnyoDecorations</c> attached property.</summary>
    public static readonly AttachedProperty<bool> UseEnyoDecorationsProperty =
        AvaloniaProperty.RegisterAttached<Window, bool>("UseEnyoDecorations", typeof(EnyoWindow));

    /// <summary>Defines the <c>AppName</c> attached property.</summary>
    public static readonly AttachedProperty<string?> AppNameProperty =
        AvaloniaProperty.RegisterAttached<StyledElement, string?>("AppName", typeof(EnyoWindow));

    /// <summary>Defines the <c>HasAppMenu</c> attached property.</summary>
    public static readonly AttachedProperty<bool> HasAppMenuProperty =
        AvaloniaProperty.RegisterAttached<StyledElement, bool>("HasAppMenu", typeof(EnyoWindow));

    /// <summary>Defines the <c>IsWindowActive</c> attached property.</summary>
    public static readonly AttachedProperty<bool> IsWindowActiveProperty =
        AvaloniaProperty.RegisterAttached<WindowDrawnDecorations, bool>("IsWindowActive", typeof(EnyoWindow), defaultValue: true);

    /// <summary>Defines the <c>MirrorsWindow</c> attached property.</summary>
    public static readonly AttachedProperty<bool> MirrorsWindowProperty =
        AvaloniaProperty.RegisterAttached<WindowDrawnDecorations, bool>("MirrorsWindow", typeof(EnyoWindow));

    /// <summary>Defines the <c>OpensAppMenu</c> attached property.</summary>
    public static readonly AttachedProperty<bool> OpensAppMenuProperty =
        AvaloniaProperty.RegisterAttached<Button, bool>("OpensAppMenu", typeof(EnyoWindow));

    /// <summary>Defines the <c>AppMenuRequested</c> routed event, raised on the window (bubbles).</summary>
    public static readonly RoutedEvent<RoutedEventArgs> AppMenuRequestedEvent =
        RoutedEvent.Register<RoutedEventArgs>("AppMenuRequested", RoutingStrategies.Bubble, typeof(EnyoWindow));

    static EnyoWindow()
    {
        UseEnyoDecorationsProperty.Changed.AddClassHandler<Window>((w, e) => OnUseEnyoDecorationsChanged(w, e.GetNewValue<bool>()));
        MirrorsWindowProperty.Changed.AddClassHandler<WindowDrawnDecorations>((d, e) => OnMirrorsWindowChanged(d, e.GetNewValue<bool>()));
        Button.ClickEvent.AddClassHandler<Button>(OnButtonClick);
    }

    /// <summary>Gets <c>UseEnyoDecorations</c>.</summary>
    public static bool GetUseEnyoDecorations(Window element) => element.GetValue(UseEnyoDecorationsProperty);

    /// <summary>
    /// Sets <c>UseEnyoDecorations</c>: true extends the window into its title bar and draws the Enyo status bar there.
    /// Does nothing on macOS.
    /// </summary>
    public static void SetUseEnyoDecorations(Window element, bool value) => element.SetValue(UseEnyoDecorationsProperty, value);

    /// <summary>Gets <c>AppName</c>.</summary>
    public static string? GetAppName(StyledElement element) => element.GetValue(AppNameProperty);

    /// <summary>Sets <c>AppName</c>: the name at the left of the bar. Default: the application's name.</summary>
    public static void SetAppName(StyledElement element, string? value) => element.SetValue(AppNameProperty, value);

    /// <summary>Gets <c>HasAppMenu</c>.</summary>
    public static bool GetHasAppMenu(StyledElement element) => element.GetValue(HasAppMenuProperty);

    /// <summary>
    /// Sets <c>HasAppMenu</c>: true makes the app name a button that raises <see cref="AppMenuRequestedEvent"/>.
    /// <c>AppMenuService</c> sets it when it connects a menu to the window.
    /// </summary>
    public static void SetHasAppMenu(StyledElement element, bool value) => element.SetValue(HasAppMenuProperty, value);

    /// <summary>Gets <c>IsWindowActive</c>: false while the decorations' window is inactive.</summary>
    public static bool GetIsWindowActive(WindowDrawnDecorations element) => element.GetValue(IsWindowActiveProperty);

    /// <summary>Gets <c>MirrorsWindow</c>.</summary>
    public static bool GetMirrorsWindow(WindowDrawnDecorations element) => element.GetValue(MirrorsWindowProperty);

    /// <summary>
    /// Sets <c>MirrorsWindow</c>: true copies the window's <c>AppName</c>, <c>HasAppMenu</c> and active state onto
    /// the decorations, which are not under the window and so cannot bind to it. Set by the decorations theme.
    /// </summary>
    public static void SetMirrorsWindow(WindowDrawnDecorations element, bool value) => element.SetValue(MirrorsWindowProperty, value);

    /// <summary>Gets <c>OpensAppMenu</c>.</summary>
    public static bool GetOpensAppMenu(Button element) => element.GetValue(OpensAppMenuProperty);

    /// <summary>Sets <c>OpensAppMenu</c>: a click on the button raises <see cref="AppMenuRequestedEvent"/> on its window.</summary>
    public static void SetOpensAppMenu(Button element, bool value) => element.SetValue(OpensAppMenuProperty, value);

    /// <summary>Adds an <c>AppMenuRequested</c> handler to an element.</summary>
    public static void AddAppMenuRequestedHandler(Interactive element, EventHandler<RoutedEventArgs> handler) =>
        element.AddHandler(AppMenuRequestedEvent, handler);

    /// <summary>Removes an <c>AppMenuRequested</c> handler from an element.</summary>
    public static void RemoveAppMenuRequestedHandler(Interactive element, EventHandler<RoutedEventArgs> handler) =>
        element.RemoveHandler(AppMenuRequestedEvent, handler);

    /// <summary>The window of <paramref name="visual"/>, including a visual in the window's drawn decorations.</summary>
    internal static Window? FindWindow(Visual? visual)
    {
        if (visual is null)
        {
            return null;
        }

        if (TopLevel.GetTopLevel(visual) is Window window)
        {
            return window;
        }

        // Drawn decorations sit beside the window, under the host that holds both.
        var root = visual;
        while (root.GetVisualParent() is { } parent)
        {
            root = parent;
        }

        return root.GetVisualChildren().OfType<Window>().FirstOrDefault();
    }

    private static void OnUseEnyoDecorationsChanged(Window window, bool use)
    {
        if (!OperatingSystem.IsMacOS())
        {
            window.SetCurrentValue(Window.ExtendClientAreaToDecorationsHintProperty, use);
        }
    }

    private static void OnMirrorsWindowChanged(WindowDrawnDecorations decorations, bool mirrors)
    {
        decorations.AttachedToLogicalTree -= OnDecorationsAttached;
        decorations.DetachedFromLogicalTree -= OnDecorationsDetached;
        StopMirroring(decorations);
        if (mirrors)
        {
            decorations.AttachedToLogicalTree += OnDecorationsAttached;
            decorations.DetachedFromLogicalTree += OnDecorationsDetached;
            StartMirroring(decorations);
        }
    }

    private static void OnDecorationsAttached(object? sender, LogicalTreeAttachmentEventArgs e) => StartMirroring((WindowDrawnDecorations)sender!);

    private static void OnDecorationsDetached(object? sender, LogicalTreeAttachmentEventArgs e) => StopMirroring((WindowDrawnDecorations)sender!);

    private static void StartMirroring(WindowDrawnDecorations decorations)
    {
        StopMirroring(decorations);
        if (FindWindow(decorations.Parent as Visual) is not { } window)
        {
            return;
        }

        var subscriptions = new List<IDisposable>
        {
            window.GetObservable(AppNameProperty).Subscribe(new AnonymousObserver<string?>(name =>
                decorations.SetCurrentValue(AppNameProperty, name ?? Application.Current?.Name))),
            window.GetObservable(HasAppMenuProperty).Subscribe(new AnonymousObserver<bool>(has =>
                decorations.SetCurrentValue(HasAppMenuProperty, has))),
            window.GetObservable(WindowBase.IsActiveProperty).Subscribe(new AnonymousObserver<bool>(active =>
                decorations.SetCurrentValue(IsWindowActiveProperty, active))),
        };
        s_mirrors.AddOrUpdate(decorations, subscriptions);
    }

    private static void StopMirroring(WindowDrawnDecorations decorations)
    {
        if (s_mirrors.TryGetValue(decorations, out var subscriptions))
        {
            foreach (var s in subscriptions)
            {
                s.Dispose();
            }

            s_mirrors.Remove(decorations);
        }
    }

    private static void OnButtonClick(Button button, RoutedEventArgs e)
    {
        if (!e.Handled && GetOpensAppMenu(button) && FindWindow(button) is { } window)
        {
            window.RaiseEvent(new RoutedEventArgs(AppMenuRequestedEvent, button));
            e.Handled = true;
        }
    }
}
