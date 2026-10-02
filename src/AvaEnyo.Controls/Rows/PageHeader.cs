using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace AvaEnyo.Controls;

/// <summary>
/// The header of a page: a <see cref="Header"/> with the title centered and an optional back button at the left.
/// The back button raises <see cref="BackRequested"/>, and also the <c>BackRequested</c> routed event of the back
/// chain, so a Pane or SlidingPane above it goes back.
/// </summary>
[TemplatePart("PART_BackButton", typeof(Button))]
public class PageHeader : Header
{
    /// <summary>Defines the <see cref="ShowBackButton"/> property.</summary>
    public static readonly StyledProperty<bool> ShowBackButtonProperty =
        AvaloniaProperty.Register<PageHeader, bool>(nameof(ShowBackButton));

    /// <summary>Defines the <see cref="BackRequested"/> event.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> BackRequestedEvent =
        RoutedEvent.Register<PageHeader, RoutedEventArgs>(nameof(BackRequested), RoutingStrategies.Bubble);

    private Button? _back;

    /// <summary>True to show the back button.</summary>
    public bool ShowBackButton
    {
        get => GetValue(ShowBackButtonProperty);
        set => SetValue(ShowBackButtonProperty, value);
    }

    /// <summary>Raised when the back button is pressed. Handle it to stop the back chain event.</summary>
    public event EventHandler<RoutedEventArgs>? BackRequested
    {
        add => AddHandler(BackRequestedEvent, value);
        remove => RemoveHandler(BackRequestedEvent, value);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_back is not null)
        {
            _back.Click -= OnBackClick;
        }

        _back = e.NameScope.Find<Button>("PART_BackButton");
        if (_back is not null)
        {
            _back.Click += OnBackClick;
        }
    }

    private void OnBackClick(object? sender, RoutedEventArgs e)
    {
        var args = new RoutedEventArgs(BackRequestedEvent, this);
        RaiseEvent(args);
        if (!args.Handled)
        {
            EnyoBack.RaiseFrom(this);
        }
    }
}
