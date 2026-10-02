using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using AvaEnyo.Animations;
using AvaEnyo.Primitives;

namespace AvaEnyo.Services;

/// <summary>
/// One webOS dashboard: a dark bar with the notification's icon, white title, grey text and a badge counting its
/// layers. Swipe it sideways past a third of its width (or fling it) to dismiss it; tap it to activate it.
/// </summary>
[TemplatePart("PART_Bar", typeof(Border), IsRequired = true)]
[PseudoClasses(":layered")]
public class DashboardPresenter : TemplatedControl
{
    /// <summary>Defines the <see cref="Notification"/> property.</summary>
    public static readonly StyledProperty<Notification?> NotificationProperty =
        AvaloniaProperty.Register<DashboardPresenter, Notification?>(nameof(Notification));

    /// <summary>The fraction of the width a swipe must pass to dismiss: 0.3.</summary>
    public static double DismissFraction = 0.3;

    private readonly DragTracker _drag;
    private readonly TranslateTransform _translate = new();
    private Notification? _subscribed;

    static DashboardPresenter()
    {
        NotificationProperty.Changed.AddClassHandler<DashboardPresenter>((p, _) => p.OnNotificationChanged());
    }

    /// <summary>Initializes a new instance of the <see cref="DashboardPresenter"/> class.</summary>
    public DashboardPresenter()
    {
        RenderTransform = _translate;
        _drag = new DragTracker(this) { CanStart = info => info.IsHorizontal };
        _drag.Moved += (_, e) =>
        {
            _translate.X = e.Delta.X;
            Opacity = Math.Max(0.2, 1 - (Math.Abs(e.Delta.X) / Math.Max(1, Bounds.Width)));
        };
        _drag.Completed += (_, e) => OnSwipeEnded(e.Delta.X, e.Velocity.X);
        _drag.Canceled += (_, _) => Restore();
        _drag.Released += (_, _) => Activated?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>The notification shown.</summary>
    public Notification? Notification
    {
        get => GetValue(NotificationProperty);
        set => SetValue(NotificationProperty, value);
    }

    /// <summary>Raised when a swipe dismissed the dashboard.</summary>
    public event EventHandler? Dismissed;

    /// <summary>Raised when the dashboard was tapped.</summary>
    public event EventHandler? Activated;

    private void OnSwipeEnded(double dx, double vx)
    {
        var width = Math.Max(1, Bounds.Width);
        if (Math.Abs(dx) > width * DismissFraction || (Math.Abs(vx) > 0.5 && Math.Sign(vx) == Math.Sign(dx)))
        {
            _translate.X = Math.Sign(dx) * width;
            Dismissed?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Restore();
        }
    }

    private void Restore()
    {
        _translate.X = 0;
        ClearValue(OpacityProperty);
    }

    private void OnNotificationChanged()
    {
        if (_subscribed is not null)
        {
            _subscribed.PropertyChanged -= OnNotificationPropertyChanged;
        }

        _subscribed = Notification;
        if (_subscribed is not null)
        {
            _subscribed.PropertyChanged += OnNotificationPropertyChanged;
        }

        UpdateLayered();
    }

    private void OnNotificationPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) => UpdateLayered();

    private void UpdateLayered() => PseudoClasses.Set(":layered", (Notification?.LayerCount ?? 1) > 1);

    /// <summary>Plays the slide-in from the bottom edge (350 ms, cubic ease-out, as a toaster).</summary>
    internal void SlideIn()
    {
        if (EnyoAnimations.IsActive)
        {
            _ = EnyoAnimations.ToasterIn(this, ToasterEdge.Bottom, Math.Max(40, Bounds.Height));
        }
    }
}
