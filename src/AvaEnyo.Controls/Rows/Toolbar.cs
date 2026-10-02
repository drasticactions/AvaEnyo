using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Threading;
using AvaEnyo.Animations;
using AvaEnyo.Controls.Primitives;

namespace AvaEnyo.Controls;

/// <summary>
/// The Enyo toolbar (<c>enyo.Toolbar</c>): a dark #343434 band at least 54 px tall whose items are centered on a
/// horizontal flex box. Items that are not controls get a <see cref="ToolButton"/>. The <c>light</c> class gives the
/// #a9a9a9 band. A <see cref="Spacer"/> takes the free space.
/// </summary>
/// <remarks>
/// With <see cref="FadeOnKeyboard"/> the toolbar hides when the on-screen keyboard opens and fades back in over
/// 150 ms after 300 ms, as Enyo did while the keyboard rose.
/// </remarks>
public class Toolbar : ItemsControl
{
    /// <summary>Defines the <see cref="FadeOnKeyboard"/> property.</summary>
    public static readonly StyledProperty<bool> FadeOnKeyboardProperty =
        AvaloniaProperty.Register<Toolbar, bool>(nameof(FadeOnKeyboard));

    private static readonly FuncTemplate<Panel?> s_panel = new(() => new ToolbarPanel());

    private Avalonia.Controls.Platform.IInputPane? _inputPane;
    private DispatcherTimer? _fadeTimer;

    static Toolbar()
    {
        ItemsPanelProperty.OverrideDefaultValue<Toolbar>(s_panel);
    }

    /// <summary>True to hide the toolbar while the on-screen keyboard rises, then fade it in.</summary>
    public bool FadeOnKeyboard
    {
        get => GetValue(FadeOnKeyboardProperty);
        set => SetValue(FadeOnKeyboardProperty, value);
    }

    /// <summary>Hides the toolbar at once and fades it in after 300 ms, over 150 ms.</summary>
    public void FadeIn()
    {
        Opacity = 0;
        _fadeTimer?.Stop();
        _fadeTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(EnyoDurations.ToolbarFadeDelay * Math.Max(0, EnyoAnimations.TimeScale)), DispatcherPriority.Normal, (_, _) =>
        {
            _fadeTimer?.Stop();
            _fadeTimer = null;
            _ = EnyoAnimations.FadeIn(this, EnyoDurations.ToolbarFade, EnyoEasing.Linear);
        });
        if (!EnyoAnimations.IsActive)
        {
            ClearValue(OpacityProperty);
            return;
        }

        _fadeTimer.Start();
    }

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) =>
        NeedsContainer<Control>(item, out recycleKey);

    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => new ToolButton();

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _inputPane = TopLevel.GetTopLevel(this)?.InputPane;
        if (_inputPane is not null)
        {
            _inputPane.StateChanged += OnInputPaneStateChanged;
        }
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_inputPane is not null)
        {
            _inputPane.StateChanged -= OnInputPaneStateChanged;
        }

        _inputPane = null;
        base.OnDetachedFromVisualTree(e);
    }

    private void OnInputPaneStateChanged(object? sender, Avalonia.Controls.Platform.InputPaneStateEventArgs e)
    {
        if (FadeOnKeyboard && e.NewState == Avalonia.Controls.Platform.InputPaneState.Open)
        {
            FadeIn();
        }
    }
}
