using System;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Metadata;
using Avalonia.VisualTree;
using Xunit;

namespace AvaEnyo.Controls.Tests;

public class ControlThemeTests
{
    [AvaloniaTheory]
    [MemberData(nameof(ThemedControls.Matrix), MemberType = typeof(ThemedControls))]
    public void Templates_And_Resolves_Keys(string name, string variant, EnyoDensity density)
    {
        var row = ThemedControls.Rows[name];
        var control = row.Create();
        var window = TestHost.Show(control, 800, 400, variant, density);
        Assert.True(control.GetVisualChildren().Any(), $"{name} has an empty visual tree.");

        var names = control.GetVisualDescendants().Select(v => (v as StyledElement)?.Name)
            .Concat(control.GetLogicalDescendants().Select(l => (l as StyledElement)?.Name))
            .ToHashSet();
        foreach (var part in row.Parts)
        {
            Assert.True(names.Contains(part), $"{name} is missing {part}.");
        }

        var theme = TestHost.ToVariant(variant);
        foreach (var key in TestHost.KeysReferencedBy(row.File))
        {
            Assert.True(control.TryFindResource(key, theme, out var value) && value is not null, $"{name}: '{key}' did not resolve under {variant}/{density}.");
        }

        if (!row.Transparent && control is TemplatedControl t)
        {
            Assert.NotNull(t.Background);
        }

        control.IsEnabled = false;
        window.UpdateLayout();
        control.IsEnabled = true;
        window.UpdateLayout();
        window.Close();
    }

    [Fact]
    public void Every_Templated_Control_Declares_Its_Required_Parts()
    {
        var types = typeof(AvaEnyoControlsTheme).Assembly.GetExportedTypes()
            .Where(t => typeof(TemplatedControl).IsAssignableFrom(t) && !t.IsAbstract);
        foreach (var type in types)
        {
            var declared = type.GetCustomAttributes<Avalonia.Controls.Metadata.TemplatePartAttribute>(inherit: true).Select(a => a.Name).ToList();
            foreach (var part in declared.Where(p => type.GetCustomAttributes<Avalonia.Controls.Metadata.TemplatePartAttribute>(true).First(a => a.Name == p).IsRequired))
            {
                var row = ThemedControls.Rows.Values.FirstOrDefault(r => r.Create().GetType() == type);
                if (row is not null)
                {
                    Assert.Contains(part, row.Parts);
                }
            }
        }
    }

    [Fact]
    public void Every_Public_Namespace_Is_Mapped()
    {
        var assembly = typeof(AvaEnyoControlsTheme).Assembly;
        var mapped = assembly.GetCustomAttributes<XmlnsDefinitionAttribute>()
            .Where(a => a.XmlNamespace == "https://github.com/avaenyo").Select(a => a.ClrNamespace).ToHashSet();
        foreach (var ns in assembly.GetExportedTypes().Select(t => t.Namespace).Where(n => n is not null && n != "CompiledAvaloniaXaml").Distinct())
        {
            Assert.True(mapped.Contains(ns!), $"namespace {ns} is not mapped");
        }
    }

    [AvaloniaFact]
    public void Controls_Theme_Must_Follow_The_Theme()
    {
        var theme = new AvaEnyoTheme();
        var controls = new AvaEnyoControlsTheme();
        Assert.Same(theme, AvaEnyoControlsTheme.FindThemeBefore(new Avalonia.Styling.IStyle[] { theme, controls }, controls));
        Assert.Null(AvaEnyoControlsTheme.FindThemeBefore(new Avalonia.Styling.IStyle[] { controls, theme }, controls));
    }

    [AvaloniaFact]
    public void Compact_Overlay_Of_The_Controls_Theme_Is_Registered()
    {
        var probe = new Border();
        var w = TestHost.Show(probe, density: EnyoDensity.Compact);
        Assert.True(probe.TryFindResource("EnyoControlsCompactMarker", out var v));
        Assert.Equal(1d, v);
        TestApplication.Instance.Theme.Density = EnyoDensity.Default;
        Assert.True(probe.TryFindResource("EnyoControlsCompactMarker", out v));
        Assert.Equal(0d, v);
        w.Close();
    }
}
