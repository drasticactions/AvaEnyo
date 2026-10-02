using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AvaEnyo.Controls;

/// <summary>
/// A progress bar drawn as a button (<c>enyo.ProgressButton</c>): a dark pressed-button frame filled from the left
/// in the raised button look, with content over it and an optional cancel button at the right. Classes
/// <c>affirmative</c>, <c>negative</c>, <c>light</c>, <c>dark</c> and <c>blue</c> color the filled part.
/// </summary>
[TemplatePart("PART_Bar", typeof(Control), IsRequired = true)]
[TemplatePart("PART_CancelButton", typeof(Button))]
public class ProgressButton : RangeBase
{
    /// <summary>Defines the <see cref="Content"/> property.</summary>
    public static readonly StyledProperty<object?> ContentProperty =
        ContentControl.ContentProperty.AddOwner<ProgressButton>();

    /// <summary>Defines the <see cref="ContentTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        ContentControl.ContentTemplateProperty.AddOwner<ProgressButton>();

    /// <summary>Defines the <see cref="Cancelable"/> property.</summary>
    public static readonly StyledProperty<bool> CancelableProperty =
        AvaloniaProperty.Register<ProgressButton, bool>(nameof(Cancelable), true);

    /// <summary>Defines the <see cref="BarBrush"/> property.</summary>
    public static readonly StyledProperty<IBrush?> BarBrushProperty =
        AvaloniaProperty.Register<ProgressButton, IBrush?>(nameof(BarBrush));

    /// <summary>Defines the <see cref="Ratio"/> property.</summary>
    public static readonly DirectProperty<ProgressButton, double> RatioProperty =
        AvaloniaProperty.RegisterDirect<ProgressButton, double>(nameof(Ratio), o => o.Ratio);

    /// <summary>Defines the <see cref="Cancel"/> event.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> CancelEvent =
        RoutedEvent.Register<ProgressButton, RoutedEventArgs>(nameof(Cancel), RoutingStrategies.Bubble);

    private Button? _cancel;
    private double _ratio;

    static ProgressButton()
    {
        MaximumProperty.OverrideDefaultValue<ProgressButton>(100);
        foreach (var p in new AvaloniaProperty[] { ValueProperty, MinimumProperty, MaximumProperty })
        {
            p.Changed.AddClassHandler<ProgressButton>((b, _) => b.UpdateRatio());
        }
    }

    /// <summary>The content shown over the bar.</summary>
    [Content]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>The template of <see cref="Content"/>.</summary>
    public IDataTemplate? ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    /// <summary>True (default): a cancel button shows at the right and raises <see cref="Cancel"/>.</summary>
    public bool Cancelable
    {
        get => GetValue(CancelableProperty);
        set => SetValue(CancelableProperty, value);
    }

    /// <summary>The background of the filled part.</summary>
    public IBrush? BarBrush
    {
        get => GetValue(BarBrushProperty);
        set => SetValue(BarBrushProperty, value);
    }

    /// <summary>The filled fraction.</summary>
    public double Ratio
    {
        get => _ratio;
        private set => SetAndRaise(RatioProperty, ref _ratio, value);
    }

    /// <summary>Raised when the cancel button is pressed.</summary>
    public event EventHandler<RoutedEventArgs>? Cancel
    {
        add => AddHandler(CancelEvent, value);
        remove => RemoveHandler(CancelEvent, value);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_cancel is not null)
        {
            _cancel.Click -= OnCancelClick;
        }

        _cancel = e.NameScope.Find<Button>("PART_CancelButton");
        if (_cancel is not null)
        {
            _cancel.Click += OnCancelClick;
        }

        UpdateRatio();
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        RaiseEvent(new RoutedEventArgs(CancelEvent, this));
    }

    private void UpdateRatio() =>
        Ratio = Maximum > Minimum ? Math.Clamp((Value - Minimum) / (Maximum - Minimum), 0, 1) : 0;
}
