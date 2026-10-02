using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using AvaEnyo.Animations;

namespace AvaEnyo.Controls;

/// <summary>
/// The Onyx spinner (<c>enyo.Spinner</c>): a 32 px ring that turns once a second while <see cref="IsActive"/>.
/// The <c>large</c> class makes it 128 px, the <c>light</c> class draws it light for dark backgrounds. Like Enyo's
/// spinner it is hidden until active.
/// </summary>
[TemplatePart("PART_Ring", typeof(Control), IsRequired = true)]
[PseudoClasses(":active")]
public class Spinner : TemplatedControl
{
    /// <summary>Defines the <see cref="IsActive"/> property.</summary>
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<Spinner, bool>(nameof(IsActive));

    /// <summary>Defines the <see cref="StrokeThickness"/> property.</summary>
    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<Spinner, double>(nameof(StrokeThickness), 3);

    private Control? _ring;
    private IDisposable? _spin;

    static Spinner()
    {
        IsActiveProperty.Changed.AddClassHandler<Spinner>((s, _) => s.Update());
    }

    /// <summary>True while the spinner shows and turns.</summary>
    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    /// <summary>The ring's line width.</summary>
    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        Stop();
        _ring = e.NameScope.Find<Control>("PART_Ring");
        Update();
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Update();
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Stop();
        base.OnDetachedFromVisualTree(e);
    }

    private void Update()
    {
        PseudoClasses.Set(":active", IsActive);
        if (IsActive && _ring is not null && Avalonia.VisualTree.VisualExtensions.IsAttachedToVisualTree(this))
        {
            _spin ??= EnyoAnimations.Spin(_ring);
        }
        else
        {
            Stop();
        }
    }

    private void Stop()
    {
        _spin?.Dispose();
        _spin = null;
    }
}
