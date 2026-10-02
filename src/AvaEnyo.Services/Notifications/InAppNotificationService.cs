using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AvaEnyo.Services;

/// <summary>
/// The default <see cref="INotificationService"/>: draws dashboards at the bottom edge of a window, newest at the
/// bottom, at most <see cref="MaxVisible"/> (3) at a time. Add <c>AvaEnyoServicesTheme</c> to the app styles for the
/// dashboard look.
/// </summary>
public sealed class InAppNotificationService : INotificationService
{
    private readonly List<Notification> _notifications = new();
    private readonly TopLevel _topLevel;
    private StackPanel? _stack;

    /// <summary>Initializes a new instance for <paramref name="topLevel"/>.</summary>
    public InAppNotificationService(TopLevel topLevel)
    {
        _topLevel = topLevel ?? throw new ArgumentNullException(nameof(topLevel));
    }

    /// <summary>The most dashboards shown at once: 3.</summary>
    public int MaxVisible { get; set; } = 3;

    /// <summary>An optional forwarder to an operating system notifier.</summary>
    public INotificationBackend? Backend { get; set; }

    /// <inheritdoc/>
    public IReadOnlyList<Notification> Notifications => _notifications;

    /// <summary>The dashboards on screen, top to bottom.</summary>
    public IReadOnlyList<DashboardPresenter> VisiblePresenters =>
        _stack?.Children.OfType<DashboardPresenter>().ToList() ?? (IReadOnlyList<DashboardPresenter>)Array.Empty<DashboardPresenter>();

    /// <inheritdoc/>
    public event EventHandler<NotificationEventArgs>? Dismissed;

    /// <inheritdoc/>
    public event EventHandler<NotificationEventArgs>? Activated;

    /// <inheritdoc/>
    public void Show(Notification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);
        var existing = _notifications.FindIndex(n => n.Id == notification.Id);
        if (existing >= 0)
        {
            _notifications[existing] = notification;
            Backend?.Update(notification);
        }
        else
        {
            _notifications.Add(notification);
            Backend?.Show(notification);
        }

        Rebuild(slideIn: existing < 0 ? notification : null);
    }

    /// <inheritdoc/>
    public void Update(Notification notification)
    {
        Backend?.Update(notification);
        Rebuild(null);
    }

    /// <inheritdoc/>
    public void Dismiss(string id)
    {
        var n = _notifications.Find(x => x.Id == id);
        if (n is null)
        {
            return;
        }

        _notifications.Remove(n);
        Backend?.Dismiss(n);
        Rebuild(null);
        Dismissed?.Invoke(this, new NotificationEventArgs(n));
    }

    private void Rebuild(Notification? slideIn)
    {
        if (EnsureStack() is not { } stack)
        {
            return;
        }

        stack.Children.Clear();
        var visible = _notifications.Skip(Math.Max(0, _notifications.Count - MaxVisible)).ToList();
        DashboardPresenter? fresh = null;
        foreach (var n in visible)
        {
            var p = new DashboardPresenter { Notification = n };
            var captured = n;
            p.Dismissed += (_, _) => Dismiss(captured.Id);
            p.Activated += (_, _) => Activated?.Invoke(this, new NotificationEventArgs(captured));
            stack.Children.Add(p);
            if (n == slideIn)
            {
                fresh = p;
            }
        }

        stack.IsVisible = visible.Count > 0;
        if (fresh is not null)
        {
            stack.UpdateLayout();
            fresh.SlideIn();
        }
    }

    private StackPanel? EnsureStack()
    {
        if (_stack is not null)
        {
            return _stack;
        }

        if (OverlayLayer.GetOverlayLayer(_topLevel) is not { } layer)
        {
            return null;
        }

        _stack = new StackPanel { Spacing = 4, Margin = new Thickness(8, 0, 8, 8), ZIndex = 1000 };
        Canvas.SetLeft(_stack, 0);
        Canvas.SetBottom(_stack, 0);
        void Sync() => _stack!.Width = Math.Max(0, layer.Bounds.Width - 16);
        layer.PropertyChanged += (_, e) =>
        {
            if (e.Property == Visual.BoundsProperty)
            {
                Sync();
            }
        };
        Sync();
        layer.Children.Add(_stack);
        return _stack;
    }
}
