using System;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AvaEnyo.Controls;

namespace AvaEnyo.Services;

/// <summary>
/// Connects an <see cref="AppMenu"/> to its window, as webOS opened the menu from the status bar. The menu opens
/// on Ctrl+` (Enyo's desktop shortcut), on F10 and on the context-menu key, and on a tap of any control marked
/// <c>enyo:AppMenuService.IsOpenRegion="True"</c> (an app's title, standing in for the status bar). It does not open
/// while a modal popup is open. Set <c>Automatic</c> to false to drive the menu yourself.
/// </summary>
/// <remarks>
/// In XAML: <c>&lt;Window enyo:AppMenuService.Menu="{StaticResource AppMenu}"&gt;</c>, or set it on the window in
/// code with <see cref="Attach"/>.
/// </remarks>
public static class AppMenuService
{
    private static readonly ConditionalWeakTable<TopLevel, AppMenuHost> s_hosts = new();

    /// <summary>Defines the <c>Menu</c> attached property.</summary>
    public static readonly AttachedProperty<AppMenu?> MenuProperty =
        AvaloniaProperty.RegisterAttached<TopLevel, AppMenu?>("Menu", typeof(AppMenuService));

    /// <summary>Defines the <c>IsOpenRegion</c> attached property.</summary>
    public static readonly AttachedProperty<bool> IsOpenRegionProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>("IsOpenRegion", typeof(AppMenuService));

    /// <summary>Defines the <c>Automatic</c> attached property.</summary>
    public static readonly AttachedProperty<bool> AutomaticProperty =
        AvaloniaProperty.RegisterAttached<TopLevel, bool>("Automatic", typeof(AppMenuService), true);

    static AppMenuService()
    {
        MenuProperty.Changed.AddClassHandler<TopLevel>((top, e) =>
        {
            Detach(top);
            if (e.GetNewValue<AppMenu?>() is { } menu)
            {
                Attach(top, menu);
            }
        });
        InputElement.TappedEvent.AddClassHandler<Control>(OnTapped, RoutingStrategies.Bubble);
    }

    /// <summary>Gets <c>Menu</c>.</summary>
    public static AppMenu? GetMenu(TopLevel element) => element.GetValue(MenuProperty);

    /// <summary>Sets <c>Menu</c>: the app menu of the window.</summary>
    public static void SetMenu(TopLevel element, AppMenu? value) => element.SetValue(MenuProperty, value);

    /// <summary>Gets <c>IsOpenRegion</c>.</summary>
    public static bool GetIsOpenRegion(Control element) => element.GetValue(IsOpenRegionProperty);

    /// <summary>Sets <c>IsOpenRegion</c>: a tap on the control toggles the window's app menu.</summary>
    public static void SetIsOpenRegion(Control element, bool value) => element.SetValue(IsOpenRegionProperty, value);

    /// <summary>Gets <c>Automatic</c>.</summary>
    public static bool GetAutomatic(TopLevel element) => element.GetValue(AutomaticProperty);

    /// <summary>
    /// Sets <c>Automatic</c>: true (default) opens the menu from keys and regions; false is Enyo's manual mode, where
    /// the app calls <see cref="IAppMenuHost.Open"/> itself.
    /// </summary>
    public static void SetAutomatic(TopLevel element, bool value) => element.SetValue(AutomaticProperty, value);

    /// <summary>Connects <paramref name="menu"/> to <paramref name="topLevel"/> and returns its host.</summary>
    public static IAppMenuHost Attach(TopLevel topLevel, AppMenu menu)
    {
        ArgumentNullException.ThrowIfNull(topLevel);
        ArgumentNullException.ThrowIfNull(menu);
        if (s_hosts.TryGetValue(topLevel, out var existing))
        {
            if (existing.Menu == menu)
            {
                return existing;
            }

            Detach(topLevel);
        }

        var host = new AppMenuHost(topLevel, menu);
        s_hosts.AddOrUpdate(topLevel, host);
        topLevel.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Bubble);
        return host;
    }

    /// <summary>Disconnects the app menu of <paramref name="topLevel"/>.</summary>
    public static void Detach(TopLevel topLevel)
    {
        if (s_hosts.TryGetValue(topLevel, out var host))
        {
            host.Detach();
            s_hosts.Remove(topLevel);
            topLevel.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown);
        }
    }

    /// <summary>The host of <paramref name="topLevel"/>, or null.</summary>
    public static IAppMenuHost? GetHost(TopLevel topLevel) => s_hosts.TryGetValue(topLevel, out var host) ? host : null;

    /// <summary>
    /// On macOS, builds a native application menu from the items of <paramref name="menu"/> and sets it on
    /// <paramref name="topLevel"/>. Does nothing on other systems.
    /// </summary>
    public static void ExportToNativeMenu(TopLevel topLevel, AppMenu menu)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }

        var native = new NativeMenu();
        foreach (var item in menu.Items)
        {
            if (item is AppMenuItem appItem)
            {
                native.Items.Add(ToNative(appItem));
            }
        }

        NativeMenu.SetMenu(topLevel, native);
    }

    /// <summary>True when <paramref name="e"/> is one of the keys that open the app menu.</summary>
    public static bool IsOpenKey(KeyEventArgs e) =>
        (e.Key == Key.OemTilde && e.KeyModifiers == KeyModifiers.Control)
        || (e.Key == Key.F10 && e.KeyModifiers == KeyModifiers.None)
        || (e.Key == Key.Apps && e.KeyModifiers == KeyModifiers.None);

    private static NativeMenuItem ToNative(AppMenuItem item)
    {
        var native = new NativeMenuItem(item.Header?.ToString() ?? string.Empty) { IsEnabled = item.IsEnabled };
        if (item.ItemCount > 0)
        {
            native.Menu = new NativeMenu();
            foreach (var child in item.Items)
            {
                if (child is AppMenuItem c)
                {
                    native.Menu.Items.Add(ToNative(c));
                }
            }
        }
        else
        {
            native.Click += (_, _) => item.RaiseClick();
        }

        return native;
    }

    private static void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Handled || sender is not TopLevel top || !GetAutomatic(top) || !IsOpenKey(e))
        {
            return;
        }

        if (s_hosts.TryGetValue(top, out var host) && host.Menu.Automatic)
        {
            host.Toggle();
            e.Handled = true;
        }
    }

    private static void OnTapped(Control control, TappedEventArgs e)
    {
        if (e.Handled || !GetIsOpenRegion(control) || TopLevel.GetTopLevel(control) is not { } top || !GetAutomatic(top))
        {
            return;
        }

        if (s_hosts.TryGetValue(top, out var host) && host.Menu.Automatic)
        {
            host.Toggle();
            e.Handled = true;
        }
    }
}
