using System.Threading.Tasks;
using Avalonia;
using AvaEnyo.Animations;

namespace AvaEnyo.Controls;

/// <summary>
/// The palmstyle modal dialog (<c>enyo.ModalDialog</c>): a 320 px <c>dialog.png</c> panel with a centered
/// <see cref="Caption"/>, over a scrim. Outside clicks do not close it; it fades in and out over 500 ms. A button
/// inside with <c>enyo:Popup.CloseValue</c> closes it, and <see cref="ShowAsync"/> returns that value.
/// </summary>
public class ModalDialog : PopupBase
{
    /// <summary>Defines the <see cref="Caption"/> property.</summary>
    public static readonly StyledProperty<string?> CaptionProperty =
        AvaloniaProperty.Register<ModalDialog, string?>(nameof(Caption));

    private TaskCompletionSource<object?>? _pending;

    static ModalDialog()
    {
        IsModalProperty.OverrideDefaultValue<ModalDialog>(true);
        DismissWithClickProperty.OverrideDefaultValue<ModalDialog>(false);
        ShowScrimProperty.OverrideDefaultValue<ModalDialog>(true);
    }

    /// <summary>Initializes a new instance of the <see cref="ModalDialog"/> class.</summary>
    public ModalDialog()
    {
        Closed += (_, e) =>
        {
            var pending = _pending;
            _pending = null;
            pending?.TrySetResult(e.Value);
        };
    }

    /// <summary>The title shown centered above the content.</summary>
    public string? Caption
    {
        get => GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }

    /// <summary>
    /// Opens the dialog centered and completes when it closes, with the close value (the <c>CloseValue</c> of the
    /// button that closed it, or the value given to <see cref="PopupBase.Close"/>), or null.
    /// </summary>
    public Task<object?> ShowAsync()
    {
        _pending?.TrySetResult(null);
        var pending = new TaskCompletionSource<object?>();
        _pending = pending;
        OpenAtCenter();
        if (!IsOpen && _pending == pending)
        {
            _pending = null;
            pending.TrySetResult(null);
        }

        return pending.Task;
    }

    /// <inheritdoc/>
    protected override PopupPresenter CreatePresenter() => new ModalDialogPresenter();

    /// <inheritdoc/>
    protected override void OnPresenterAttached(PopupPresenter presenter)
    {
        base.OnPresenterAttached(presenter);
        presenter.Bind(ModalDialogPresenter.CaptionProperty, this.GetObservable(CaptionProperty));
    }

    /// <inheritdoc/>
    protected override Task AnimateOpenAsync(PopupPresenter presenter) =>
        EnyoAnimations.FadeIn(presenter, EnyoDurations.ModalDialogFade, EnyoEasing.CssEase);

    /// <inheritdoc/>
    protected override Task AnimateCloseAsync(PopupPresenter presenter) =>
        EnyoAnimations.FadeOut(presenter, EnyoDurations.ModalDialogFade, EnyoEasing.CssEase);
}
