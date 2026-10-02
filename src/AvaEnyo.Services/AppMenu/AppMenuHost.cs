using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using AvaEnyo.Controls;

namespace AvaEnyo.Services;

/// <summary>
/// The default <see cref="IAppMenuHost"/>: drives one <see cref="AppMenu"/> in one window. Create it with
/// <see cref="AppMenuService.Attach"/>.
/// </summary>
public sealed class AppMenuHost : IAppMenuHost
{
    internal AppMenuHost(TopLevel topLevel, AppMenu menu)
    {
        TopLevel = topLevel;
        Menu = menu;
        if (menu.Parent is null && OverlayLayer.GetOverlayLayer(topLevel) is { } layer)
        {
            // A menu made in code has no place in the window; the overlay layer holds its zero-size placeholder.
            layer.Children.Add(menu);
        }

        menu.BeforeOpen += OnBeforeOpen;
        menu.Closed += OnClosed;
    }

    /// <summary>The window the menu belongs to.</summary>
    public TopLevel TopLevel { get; }

    /// <summary>The menu.</summary>
    public AppMenu Menu { get; }

    /// <inheritdoc/>
    public bool IsOpen => Menu.IsOpen;

    /// <inheritdoc/>
    public event EventHandler<AppMenuOpeningEventArgs>? Opening;

    /// <inheritdoc/>
    public event EventHandler<AppMenuClosedEventArgs>? Closed;

    /// <inheritdoc/>
    public void Open() => Menu.Open();

    /// <inheritdoc/>
    public void Close() => Menu.Close();

    /// <inheritdoc/>
    public void Toggle() => Menu.Toggle();

    internal void Detach()
    {
        Menu.BeforeOpen -= OnBeforeOpen;
        Menu.Closed -= OnClosed;
    }

    private void OnBeforeOpen(object? sender, PopupOpeningEventArgs e)
    {
        var args = new AppMenuOpeningEventArgs(Menu);
        Opening?.Invoke(this, args);
        if (args.Cancel)
        {
            e.Cancel = true;
        }
    }

    private void OnClosed(object? sender, PopupClosedEventArgs e) => Closed?.Invoke(this, new AppMenuClosedEventArgs(e.Reason, e.Value));
}
