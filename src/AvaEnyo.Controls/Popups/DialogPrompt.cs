using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace AvaEnyo.Controls;

/// <summary>
/// A question in a bottom <see cref="Dialog"/> (<c>enyo.DialogPrompt</c>): a title, a message, an accept button and a
/// cancel button, over a scrim. <see cref="ShowAsync"/> completes with true when accepted and false otherwise. A null
/// <see cref="CancelButtonContent"/> hides the cancel button.
/// </summary>
public class DialogPrompt : Dialog
{
    /// <summary>Defines the <see cref="Title"/> property.</summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<DialogPrompt, string?>(nameof(Title));

    /// <summary>Defines the <see cref="Message"/> property.</summary>
    public static readonly StyledProperty<string?> MessageProperty =
        AvaloniaProperty.Register<DialogPrompt, string?>(nameof(Message));

    /// <summary>Defines the <see cref="AcceptButtonContent"/> property.</summary>
    public static readonly StyledProperty<object?> AcceptButtonContentProperty =
        AvaloniaProperty.Register<DialogPrompt, object?>(nameof(AcceptButtonContent), "OK");

    /// <summary>Defines the <see cref="CancelButtonContent"/> property.</summary>
    public static readonly StyledProperty<object?> CancelButtonContentProperty =
        AvaloniaProperty.Register<DialogPrompt, object?>(nameof(CancelButtonContent), "Cancel");

    /// <summary>Defines the <see cref="Accepted"/> event.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> AcceptedEvent =
        RoutedEvent.Register<DialogPrompt, RoutedEventArgs>(nameof(Accepted), RoutingStrategies.Bubble);

    /// <summary>Defines the <see cref="Cancelled"/> event.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> CancelledEvent =
        RoutedEvent.Register<DialogPrompt, RoutedEventArgs>(nameof(Cancelled), RoutingStrategies.Bubble);

    private TaskCompletionSource<bool>? _pending;
    private bool _accepted;

    static DialogPrompt()
    {
        ShowScrimProperty.OverrideDefaultValue<DialogPrompt>(true);
    }

    /// <summary>Initializes a new instance of the <see cref="DialogPrompt"/> class.</summary>
    public DialogPrompt()
    {
        Content = BuildContent();
        Closed += (_, _) =>
        {
            var pending = _pending;
            _pending = null;
            pending?.TrySetResult(_accepted);
        };
    }

    /// <summary>The title, shown above a rule. Null hides it.</summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>The message.</summary>
    public string? Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    /// <summary>The accept button's content. Default "OK".</summary>
    public object? AcceptButtonContent
    {
        get => GetValue(AcceptButtonContentProperty);
        set => SetValue(AcceptButtonContentProperty, value);
    }

    /// <summary>The cancel button's content. Default "Cancel"; null hides the button.</summary>
    public object? CancelButtonContent
    {
        get => GetValue(CancelButtonContentProperty);
        set => SetValue(CancelButtonContentProperty, value);
    }

    /// <summary>Raised when the accept button is pressed.</summary>
    public event EventHandler<RoutedEventArgs>? Accepted
    {
        add => AddHandler(AcceptedEvent, value);
        remove => RemoveHandler(AcceptedEvent, value);
    }

    /// <summary>Raised when the cancel button is pressed.</summary>
    public event EventHandler<RoutedEventArgs>? Cancelled
    {
        add => AddHandler(CancelledEvent, value);
        remove => RemoveHandler(CancelledEvent, value);
    }

    /// <summary>Opens the prompt. Completes with true when accepted, false when cancelled or dismissed.</summary>
    public Task<bool> ShowAsync()
    {
        _pending?.TrySetResult(false);
        var pending = new TaskCompletionSource<bool>();
        _pending = pending;
        _accepted = false;
        Open();
        if (!IsOpen && _pending == pending)
        {
            _pending = null;
            pending.TrySetResult(false);
        }

        return pending.Task;
    }

    /// <summary>Accepts, as the accept button does.</summary>
    public void Accept()
    {
        _accepted = true;
        RaiseEvent(new RoutedEventArgs(AcceptedEvent, this));
        Close(true);
    }

    /// <summary>Cancels, as the cancel button does.</summary>
    public void Cancel()
    {
        _accepted = false;
        RaiseEvent(new RoutedEventArgs(CancelledEvent, this));
        Close(false);
    }

    private Control BuildContent()
    {
        var title = new TextBlock { Classes = { "enyo-dialog-prompt-title" }, TextWrapping = Avalonia.Media.TextWrapping.Wrap };
        title.Bind(TextBlock.TextProperty, this.GetObservable(TitleProperty));
        title.Bind(IsVisibleProperty, this.GetObservable(TitleProperty, t => !string.IsNullOrEmpty(t)));
        var rule = new Border { Classes = { "enyo-dialog-prompt-rule" }, Height = 1 };
        rule.Bind(IsVisibleProperty, this.GetObservable(TitleProperty, t => !string.IsNullOrEmpty(t)));
        var message = new TextBlock { Classes = { "enyo-dialog-prompt-message" }, TextWrapping = Avalonia.Media.TextWrapping.Wrap };
        message.Bind(TextBlock.TextProperty, this.GetObservable(MessageProperty));
        var accept = new Button { Name = "PART_AcceptButton", HorizontalAlignment = HorizontalAlignment.Stretch };
        accept.Bind(ContentControl.ContentProperty, this.GetObservable(AcceptButtonContentProperty));
        accept.Click += (_, e) =>
        {
            e.Handled = true;
            Accept();
        };
        var cancel = new Button { Name = "PART_CancelButton", HorizontalAlignment = HorizontalAlignment.Stretch };
        cancel.Bind(ContentControl.ContentProperty, this.GetObservable(CancelButtonContentProperty));
        cancel.Bind(IsVisibleProperty, this.GetObservable(CancelButtonContentProperty, c => c is not null));
        cancel.Click += (_, e) =>
        {
            e.Handled = true;
            Cancel();
        };

        return new StackPanel
        {
            Classes = { "enyo-dialog-prompt" },
            Children =
            {
                title,
                rule,
                new StackPanel { Classes = { "enyo-dialog-prompt-content" }, Children = { message, accept, cancel } },
            },
        };
    }
}
