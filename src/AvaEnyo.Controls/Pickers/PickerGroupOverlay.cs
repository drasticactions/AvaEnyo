using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace AvaEnyo.Controls;

/// <summary>
/// The copy of a <see cref="PickerGroup"/>'s pills that sits above the scrim while one of its lists is open. Enyo's
/// PickerGroup did the same: it put the generated HTML of its pickers in a popup over the scrim. Here the copy is a
/// live <see cref="VisualBrush"/> of the pills, so their focus look shows; it takes no input.
/// </summary>
public sealed class PickerGroupOverlay : PopupBase
{
    /// <summary>Initializes a new instance of the <see cref="PickerGroupOverlay"/> class.</summary>
    public PickerGroupOverlay()
    {
        DismissWithClick = false;
        DismissWithEscape = false;
    }

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(PopupBase);

    /// <summary>Shows a copy of <paramref name="source"/> exactly over it.</summary>
    public void Show(Control source, bool scrim)
    {
        ShowScrim = scrim;
        Content = new Rectangle
        {
            Width = source.Bounds.Width,
            Height = source.Bounds.Height,
            IsHitTestVisible = false,
            Fill = new VisualBrush
            {
                Visual = source,
                Stretch = Stretch.None,
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Top,
            },
        };
        OpenAtControl(source);
    }

    /// <inheritdoc/>
    protected override PopupPresenter CreatePresenter() => new() { IsHitTestVisible = false };
}
