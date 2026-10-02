using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AvaEnyo;

/// <summary>
/// Places the drop-down of a <see cref="ComboBox"/> as Enyo's ListSelector placed its popup: the selected row lies
/// over the control. The popup is centered on the control (<c>Placement="Center"</c> in the theme), then shifted by
/// the distance between the selected row and the popup center. The popup host keeps it on screen.
/// </summary>
internal static class ListSelectorPlacement
{
    public static void Register()
    {
        ComboBox.IsDropDownOpenProperty.Changed.AddClassHandler<ComboBox>((box, e) =>
        {
            if (e.NewValue is true)
            {
                Dispatcher.UIThread.Post(() => Align(box), DispatcherPriority.Render);
            }
            else if (FindPopup(box) is { } popup)
            {
                popup.VerticalOffset = 0;
            }
        });
    }

    private static Popup? FindPopup(ComboBox box)
    {
        foreach (var v in box.GetVisualDescendants())
        {
            if (v is Popup { Name: "PART_Popup" } p && p.TemplatedParent == box)
            {
                return p;
            }
        }

        return null;
    }

    private static void Align(ComboBox box)
    {
        if (!box.IsDropDownOpen || FindPopup(box) is not { Child: Control frame } popup || popup.Placement != PlacementMode.Center)
        {
            return;
        }

        if (box.SelectedIndex < 0 || box.ContainerFromIndex(box.SelectedIndex) is not Control row)
        {
            return;
        }

        frame.UpdateLayout();
        if (row.TranslatePoint(new Point(0, row.Bounds.Height / 2), frame) is not { } rowCenter)
        {
            return;
        }

        var frameCenter = frame.Bounds.Height / 2;
        var offset = frameCenter - rowCenter.Y;
        if (Math.Abs(popup.VerticalOffset - offset) > 0.5)
        {
            popup.VerticalOffset = offset;
        }
    }
}
