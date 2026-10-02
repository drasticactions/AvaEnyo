using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AvaEnyo.Animations;

namespace AvaEnyo.Controls;

/// <summary>
/// The Enyo drawer (<c>enyo.Drawer</c>): a header and content that opens over 250 ms and closes over 100 ms with a
/// cubic ease-out height animation, ending at its natural height or collapsed. Clicking the header toggles it.
/// </summary>
[TemplatePart("PART_Header", typeof(Control))]
[TemplatePart("PART_ContentHost", typeof(Control), IsRequired = true)]
[PseudoClasses(":open")]
public class Drawer : HeaderedContentControl
{
    /// <summary>Defines the <see cref="IsOpen"/> property.</summary>
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<Drawer, bool>(nameof(IsOpen), true, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Defines the <see cref="CanChangeOpen"/> property.</summary>
    public static readonly StyledProperty<bool> CanChangeOpenProperty =
        AvaloniaProperty.Register<Drawer, bool>(nameof(CanChangeOpen), true);

    /// <summary>Defines the <see cref="Animate"/> property.</summary>
    public static readonly StyledProperty<bool> AnimateProperty =
        AvaloniaProperty.Register<Drawer, bool>(nameof(Animate), true);

    /// <summary>Defines the <see cref="IsOpenChanged"/> event.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> IsOpenChangedEvent =
        RoutedEvent.Register<Drawer, RoutedEventArgs>(nameof(IsOpenChanged), RoutingStrategies.Bubble);

    /// <summary>Defines the <see cref="OpenAnimationCompleted"/> event.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> OpenAnimationCompletedEvent =
        RoutedEvent.Register<Drawer, RoutedEventArgs>(nameof(OpenAnimationCompleted), RoutingStrategies.Bubble);

    private Control? _header;
    private Control? _host;
    private bool _reverting;
    private int _generation;

    static Drawer()
    {
        IsOpenProperty.Changed.AddClassHandler<Drawer>((d, e) => d.OnIsOpenChanged(e));
    }

    /// <summary>Initializes a new instance of the <see cref="Drawer"/> class.</summary>
    public Drawer()
    {
        PseudoClasses.Set(":open", true);
    }

    /// <summary>True (default) when the content shows.</summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>True (default): <see cref="IsOpen"/> may change. False locks the drawer.</summary>
    public bool CanChangeOpen
    {
        get => GetValue(CanChangeOpenProperty);
        set => SetValue(CanChangeOpenProperty, value);
    }

    /// <summary>True (default): opening and closing animate.</summary>
    public bool Animate
    {
        get => GetValue(AnimateProperty);
        set => SetValue(AnimateProperty, value);
    }

    /// <summary>Raised when <see cref="IsOpen"/> changes.</summary>
    public event EventHandler<RoutedEventArgs>? IsOpenChanged
    {
        add => AddHandler(IsOpenChangedEvent, value);
        remove => RemoveHandler(IsOpenChangedEvent, value);
    }

    /// <summary>Raised when an open or close animation ends.</summary>
    public event EventHandler<RoutedEventArgs>? OpenAnimationCompleted
    {
        add => AddHandler(OpenAnimationCompletedEvent, value);
        remove => RemoveHandler(OpenAnimationCompletedEvent, value);
    }

    /// <summary>Opens the drawer.</summary>
    public void Open() => IsOpen = true;

    /// <summary>Closes the drawer.</summary>
    public void Close() => IsOpen = false;

    /// <summary>Toggles the drawer.</summary>
    public void ToggleOpen() => IsOpen = !IsOpen;

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_header is not null)
        {
            _header.Tapped -= OnHeaderTapped;
        }

        _header = e.NameScope.Find<Control>("PART_Header");
        _host = e.NameScope.Find<Control>("PART_ContentHost");
        if (_header is not null)
        {
            _header.Tapped += OnHeaderTapped;
        }

        if (_host is not null)
        {
            _host.IsVisible = IsOpen;
        }
    }

    private void OnHeaderTapped(object? sender, TappedEventArgs e)
    {
        e.Handled = true;
        ToggleOpen();
    }

    private async void OnIsOpenChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_reverting)
        {
            return;
        }

        var open = e.GetNewValue<bool>();
        if (!CanChangeOpen)
        {
            _reverting = true;
            try
            {
                SetCurrentValue(IsOpenProperty, !open);
            }
            finally
            {
                _reverting = false;
            }

            return;
        }

        PseudoClasses.Set(":open", open);
        RaiseEvent(new RoutedEventArgs(IsOpenChangedEvent, this));
        if (_host is not { } host)
        {
            return;
        }

        var generation = ++_generation;
        if (Animate && host.IsAttachedToVisualTree())
        {
            var done = open ? await EnyoAnimations.ExpandHeight(host) : await EnyoAnimations.CollapseHeight(host);
            if (!done || generation != _generation)
            {
                return;
            }
        }
        else
        {
            host.ClearValue(HeightProperty);
            host.IsVisible = open;
        }

        RaiseEvent(new RoutedEventArgs(OpenAnimationCompletedEvent, this));
    }
}
