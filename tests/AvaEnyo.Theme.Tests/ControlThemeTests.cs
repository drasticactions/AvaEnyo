using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Xunit;

namespace AvaEnyo.Theme.Tests;

public class ControlThemeTests
{
    [AvaloniaTheory]
    [MemberData(nameof(ThemedControls.Matrix), MemberType = typeof(ThemedControls))]
    public void Templates_And_Resolves_Keys(string name, string variant, EnyoDensity density)
    {
        var control = ThemedControls.Factories[name]();
        var window = ThemeTestHelpers.Host(control, variant, density);

        // 1. Non-empty visual tree.
        Assert.True(control.GetVisualChildren().Any() || control is TextBlock,
            $"{name} produced an empty visual tree under {variant}/{density}.");

        // 2. Required parts.
        if (ThemedControls.RequiredParts.TryGetValue(name, out var parts))
        {
            var present = PartFinder.Collect(control);
            foreach (var part in parts)
            {
                Assert.True(present.Contains(part), $"{name} template is missing {part}.");
            }
        }

        // 3. Every key the theme file references resolves in this variant and density.
        var theme = ThemeTestHelpers.ToVariant(variant);
        var scoped = ThemedControls.TemplateScopedKeys.TryGetValue(name, out var sk) ? sk : Array.Empty<string>();
        foreach (var key in ThemeTestHelpers.KeysReferencedBy("ThemeXaml", name))
        {
            if (scoped.Contains(key))
            {
                continue;
            }

            Assert.True(control.TryFindResource(key, theme, out var value) && value is not null,
                $"{name}: resource '{key}' did not resolve under {variant}/{density}.");
        }

        // 4. Background resolves (proof the theme applied).
        if (control is TemplatedControl templated && !ThemedControls.TransparentByDesign.Contains(name))
        {
            Assert.True(templated.Background is not null, $"{name}.Background did not resolve under {variant}/{density}.");
        }

        // 5. The template survives IsEnabled toggling and the control-specific states.
        control.IsEnabled = false;
        window.UpdateLayout();
        control.IsEnabled = true;
        StateWalk.Apply(control, window);
        Assert.True(control.GetVisualChildren().Any() || control is TextBlock);
        window.Close();
    }
}
