using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace AvaEnyo.Theme.Tests;

/// <summary>Names of every named element in a control's template, including the children of closed popups.</summary>
public static class PartFinder
{
    public static HashSet<string?> Collect(Control control)
    {
        var names = new HashSet<string?>();
        var seen = new HashSet<StyledElement>();
        void Walk(StyledElement e)
        {
            if (!seen.Add(e))
            {
                return;
            }

            names.Add(e.Name);
            var children = new List<StyledElement>();
            if (e is Visual v)
            {
                children.AddRange(v.GetVisualDescendants().OfType<StyledElement>());
            }

            children.AddRange(e.GetLogicalDescendants().OfType<StyledElement>());
            foreach (var child in children)
            {
                names.Add(child.Name);
                if (child is Avalonia.Controls.Primitives.Popup { Child: StyledElement pc })
                {
                    Walk(pc);
                }
            }
        }

        Walk(control);
        return names;
    }
}
