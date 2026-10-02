using System;
using Avalonia;
using Avalonia.Controls;

namespace AvaEnyo.Controls.Primitives;

/// <summary>
/// A single row in which a caption shrinks before a filling element does: the layout of Enyo's divider, where the
/// left cap had flex 1000 and the caption flex .1, so the caption truncated and the rule kept at least its minimum.
/// Children take space by their <see cref="RoleProperty"/>.
/// </summary>
public class RowPanel : Panel
{
    /// <summary>Defines the <c>Role</c> attached property.</summary>
    public static readonly AttachedProperty<RowRole> RoleProperty =
        AvaloniaProperty.RegisterAttached<RowPanel, Control, RowRole>("Role");

    static RowPanel()
    {
        AffectsParentMeasure<RowPanel>(RoleProperty);
    }

    /// <summary>Gets the role.</summary>
    public static RowRole GetRole(Control element) => element.GetValue(RoleProperty);

    /// <summary>Sets the role.</summary>
    public static void SetRole(Control element, RowRole value) => element.SetValue(RoleProperty, value);

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        double fixedWidth = 0, height = 0, fillMin = 0;
        Control? shrink = null;
        foreach (var c in Children)
        {
            if (!c.IsVisible)
            {
                continue;
            }

            switch (GetRole(c))
            {
                case RowRole.Shrink:
                    shrink = c;
                    break;
                case RowRole.Fill:
                    c.Measure(new Size(double.PositiveInfinity, availableSize.Height));
                    fillMin += Math.Max(c.MinWidth, 0);
                    height = Math.Max(height, c.DesiredSize.Height);
                    break;
                default:
                    c.Measure(new Size(double.PositiveInfinity, availableSize.Height));
                    fixedWidth += c.DesiredSize.Width;
                    height = Math.Max(height, c.DesiredSize.Height);
                    break;
            }
        }

        var room = double.IsInfinity(availableSize.Width) ? double.PositiveInfinity : Math.Max(0, availableSize.Width - fixedWidth - fillMin);
        if (shrink is not null)
        {
            shrink.Measure(new Size(room, availableSize.Height));
            height = Math.Max(height, shrink.DesiredSize.Height);
            fixedWidth += shrink.DesiredSize.Width;
        }

        return new Size(fixedWidth + fillMin, height);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        double used = 0;
        var fills = 0;
        foreach (var c in Children)
        {
            if (!c.IsVisible)
            {
                continue;
            }

            if (GetRole(c) == RowRole.Fill)
            {
                fills++;
            }
            else
            {
                used += c.DesiredSize.Width;
            }
        }

        var fillWidth = fills > 0 ? Math.Max(0, finalSize.Width - used) / fills : 0;
        double left = 0, right = finalSize.Width;
        foreach (var c in Children)
        {
            if (!c.IsVisible || GetRole(c) != RowRole.End)
            {
                continue;
            }

            right -= c.DesiredSize.Width;
            c.Arrange(new Rect(right, 0, c.DesiredSize.Width, finalSize.Height));
        }

        foreach (var c in Children)
        {
            if (!c.IsVisible || GetRole(c) == RowRole.End)
            {
                continue;
            }

            var w = GetRole(c) == RowRole.Fill ? Math.Max(fillWidth, c.MinWidth) : c.DesiredSize.Width;
            c.Arrange(new Rect(left, 0, w, finalSize.Height));
            left += w;
        }

        return finalSize;
    }
}
