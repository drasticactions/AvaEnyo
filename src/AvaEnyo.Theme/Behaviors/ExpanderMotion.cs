using System.Runtime.CompilerServices;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.VisualTree;
using AvaEnyo.Animations;

namespace AvaEnyo;

/// <summary>
/// The Enyo drawer motion for an <see cref="Expander"/>: the content part (<c>ExpanderContent</c>) opens over
/// 250 ms and closes over 100 ms with a cubic ease-out height animation, ending at its natural height or collapsed.
/// Only downward expanders animate; the others show and hide at once.
/// </summary>
internal static class ExpanderMotion
{
    private static readonly ConditionalWeakTable<Expander, CancellationTokenSource> s_runs = new();

    public static void Register()
    {
        TemplatedControl.TemplateAppliedEvent.AddClassHandler<Expander>((e, _) => Sync(e));
        Expander.IsExpandedProperty.Changed.AddClassHandler<Expander>((e, _) => Animate(e));
    }

    private static Control? Content(Expander expander)
    {
        foreach (var v in expander.GetVisualDescendants())
        {
            if (v is Control { Name: "ExpanderContent" } c && c.TemplatedParent == expander)
            {
                return c;
            }
        }

        return null;
    }

    private static void Sync(Expander expander)
    {
        if (Content(expander) is { } content)
        {
            content.IsVisible = expander.IsExpanded;
        }
    }

    private static void Animate(Expander expander)
    {
        if (Content(expander) is not { } content)
        {
            return;
        }

        if (s_runs.TryGetValue(expander, out var previous))
        {
            previous.Cancel();
            s_runs.Remove(expander);
        }

        if (expander.ExpandDirection != ExpandDirection.Down || !EnyoAnimations.IsActive || !expander.IsAttachedToVisualTree())
        {
            content.ClearValue(Layoutable.HeightProperty);
            content.IsVisible = expander.IsExpanded;
            return;
        }

        var cts = new CancellationTokenSource();
        s_runs.Add(expander, cts);
        _ = expander.IsExpanded
            ? EnyoAnimations.ExpandHeight(content, EnyoDurations.DrawerOpen, cts.Token)
            : EnyoAnimations.CollapseHeight(content, EnyoDurations.DrawerClose, cts.Token);
    }
}
