using Avalonia;
using Avalonia.Controls;

namespace AvaEnyo.Controls;

/// <summary>
/// A view of a <see cref="Pane"/> whose content can be created late (Enyo <c>lazy: true</c>). With
/// <see cref="IsLazy"/>, the <see cref="ContentControl.ContentTemplate"/> is built the first time the view is
/// selected; otherwise it is built when the view is created.
/// </summary>
public class PaneView : ContentControl
{
    /// <summary>Defines the <see cref="IsLazy"/> property.</summary>
    public static readonly StyledProperty<bool> IsLazyProperty =
        AvaloniaProperty.Register<PaneView, bool>(nameof(IsLazy));

    /// <summary>True to build the content only when the view is first selected.</summary>
    public bool IsLazy
    {
        get => GetValue(IsLazyProperty);
        set => SetValue(IsLazyProperty, value);
    }

    /// <summary>True once the content exists.</summary>
    public bool IsRealized => Content is not null;

    /// <summary>Builds the content from the template if it does not exist yet.</summary>
    public void Realize()
    {
        if (Content is null && ContentTemplate is { } template)
        {
            Content = template.Build(DataContext);
        }
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (!IsLazy)
        {
            Realize();
        }
    }
}
