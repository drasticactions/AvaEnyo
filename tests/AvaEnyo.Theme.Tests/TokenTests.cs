using System.Linq;
using Avalonia.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Xunit;

namespace AvaEnyo.Theme.Tests;

public class TokenTests
{
    public static TheoryData<string, EnyoDensity> Combinations => new()
    {
        { "Light", EnyoDensity.Default },
        { "Light", EnyoDensity.Compact },
        { "Dark", EnyoDensity.Default },
        { "Dark", EnyoDensity.Compact },
    };

    [AvaloniaTheory]
    [MemberData(nameof(Combinations))]
    public void Every_Layer2_And_Palette_Key_Resolves(string variant, EnyoDensity density)
    {
        var probe = new Border();
        var window = ThemeTestHelpers.Host(probe, variant, density);
        var theme = ThemeTestHelpers.ToVariant(variant);
        foreach (var key in TokenKeys.Layer2)
        {
            Assert.True(probe.TryFindResource(key, theme, out var value) && value is not null, $"{key} did not resolve under {variant}/{density}.");
        }

        foreach (var key in TokenKeys.Palette)
        {
            Assert.True(probe.TryFindResource(key, theme, out var value) && value is Color, $"{key} is not a Color under {variant}/{density}.");
        }

        window.Close();
    }

    [AvaloniaFact]
    public void Compact_Overlay_Changes_Metrics()
    {
        var probe = new Border();
        var window = ThemeTestHelpers.Host(probe, "Light", EnyoDensity.Default);
        Assert.True(probe.TryFindResource("EnyoToolbarMinHeight", out var normal));
        Assert.Equal(54d, normal);
        TestApplication.Instance.Theme.Density = EnyoDensity.Compact;
        Assert.True(probe.TryFindResource("EnyoToolbarMinHeight", out var compact));
        Assert.Equal(40d, compact);
        TestApplication.Instance.Theme.Density = EnyoDensity.Default;
        window.Close();
    }

    [AvaloniaFact]
    public void Density_Switch_Remeasures_A_Bound_Button()
    {
        var button = new Button { Content = "Hello" };
        var window = ThemeTestHelpers.Host(new StackPanel { Children = { button } }, "Light", EnyoDensity.Default);
        var tall = button.Bounds.Height;
        TestApplication.Instance.Theme.Density = EnyoDensity.Compact;
        window.UpdateLayout();
        var small = button.Bounds.Height;
        Assert.True(small < tall, $"default {tall}, compact {small}");
        Assert.Equal(13d, button.FontSize);
        TestApplication.Instance.Theme.Density = EnyoDensity.Default;
        window.UpdateLayout();
        Assert.Equal(tall, button.Bounds.Height);
        window.Close();
    }

    [AvaloniaFact]
    public void App_Level_Override_Wins()
    {
        var app = Application.Current!;
        app.Resources["EnyoButtonAffirmativeBackgroundBrush"] = Brushes.DeepPink;
        try
        {
            var button = new Button { Content = "x", Classes = { "affirmative" } };
            var window = ThemeTestHelpers.Host(button, "Light", EnyoDensity.Default);
            Assert.Equal(Brushes.DeepPink, button.Background);
            window.Close();
        }
        finally
        {
            app.Resources.Remove("EnyoButtonAffirmativeBackgroundBrush");
        }
    }

    [AvaloniaFact]
    public void Aliases_Resolve_To_Their_Targets()
    {
        var probe = new Border();
        var window = ThemeTestHelpers.Host(probe, "Light", EnyoDensity.Compact);
        Assert.True(probe.TryFindResource("ButtonPadding", out var padding));
        Assert.Equal(new Thickness(6, 2), padding);
        TestApplication.Instance.Theme.Density = EnyoDensity.Default;
        window.Close();
    }

    [AvaloniaFact]
    public void Background_Tints_A_Button()
    {
        var button = new Button { Content = "x", Background = Brushes.DeepPink };
        var window = ThemeTestHelpers.Host(button, "Light", EnyoDensity.Default);
        var bg = Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(button).OfType<Border>().First(c => c.Name == "PART_Background");
        Assert.Equal(Brushes.DeepPink, bg.Background);
        window.Close();
    }
}
