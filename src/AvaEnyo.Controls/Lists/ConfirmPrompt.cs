using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace AvaEnyo.Controls;

/// <summary>
/// The inline confirm prompt of a swiped row (<c>enyo.ConfirmPrompt</c>): a Cancel button and a red Confirm button,
/// centered on the dark <c>confirmprompt-background.png</c> band. <see cref="SwipeableItem"/> shows it; it can also be
/// used on its own.
/// </summary>
[TemplatePart("PART_ConfirmButton", typeof(Button), IsRequired = true)]
[TemplatePart("PART_CancelButton", typeof(Button), IsRequired = true)]
public class ConfirmPrompt : TemplatedControl
{
    /// <summary>Defines the <see cref="ConfirmCaption"/> property.</summary>
    public static readonly StyledProperty<object?> ConfirmCaptionProperty =
        AvaloniaProperty.Register<ConfirmPrompt, object?>(nameof(ConfirmCaption), "Delete");

    /// <summary>Defines the <see cref="CancelCaption"/> property.</summary>
    public static readonly StyledProperty<object?> CancelCaptionProperty =
        AvaloniaProperty.Register<ConfirmPrompt, object?>(nameof(CancelCaption), "Cancel");

    /// <summary>Defines the <see cref="Confirmed"/> event.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> ConfirmedEvent =
        RoutedEvent.Register<ConfirmPrompt, RoutedEventArgs>(nameof(Confirmed), RoutingStrategies.Bubble);

    /// <summary>Defines the <see cref="Cancelled"/> event.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> CancelledEvent =
        RoutedEvent.Register<ConfirmPrompt, RoutedEventArgs>(nameof(Cancelled), RoutingStrategies.Bubble);

    private Button? _confirm;
    private Button? _cancel;

    /// <summary>The content of the Confirm button. Default "Delete".</summary>
    public object? ConfirmCaption
    {
        get => GetValue(ConfirmCaptionProperty);
        set => SetValue(ConfirmCaptionProperty, value);
    }

    /// <summary>The content of the Cancel button. Default "Cancel".</summary>
    public object? CancelCaption
    {
        get => GetValue(CancelCaptionProperty);
        set => SetValue(CancelCaptionProperty, value);
    }

    /// <summary>Raised when Confirm is pressed.</summary>
    public event EventHandler<RoutedEventArgs>? Confirmed
    {
        add => AddHandler(ConfirmedEvent, value);
        remove => RemoveHandler(ConfirmedEvent, value);
    }

    /// <summary>Raised when Cancel is pressed.</summary>
    public event EventHandler<RoutedEventArgs>? Cancelled
    {
        add => AddHandler(CancelledEvent, value);
        remove => RemoveHandler(CancelledEvent, value);
    }

    /// <summary>Raises <see cref="Confirmed"/>, as the Confirm button does.</summary>
    public void Confirm() => RaiseEvent(new RoutedEventArgs(ConfirmedEvent, this));

    /// <summary>Raises <see cref="Cancelled"/>, as the Cancel button does.</summary>
    public void Cancel() => RaiseEvent(new RoutedEventArgs(CancelledEvent, this));

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_confirm is not null)
        {
            _confirm.Click -= OnConfirmClick;
        }

        if (_cancel is not null)
        {
            _cancel.Click -= OnCancelClick;
        }

        _confirm = e.NameScope.Find<Button>("PART_ConfirmButton");
        _cancel = e.NameScope.Find<Button>("PART_CancelButton");
        if (_confirm is not null)
        {
            _confirm.Click += OnConfirmClick;
        }

        if (_cancel is not null)
        {
            _cancel.Click += OnCancelClick;
        }
    }

    private void OnConfirmClick(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        Confirm();
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        Cancel();
    }
}
