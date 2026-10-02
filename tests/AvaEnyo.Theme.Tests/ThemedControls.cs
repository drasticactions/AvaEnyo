using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Xunit;

namespace AvaEnyo.Theme.Tests;

/// <summary>One row per theme file: a factory, its required parts, and whether it is transparent by design.</summary>
public static partial class ThemedControls
{
    public static readonly Dictionary<string, Func<Control>> Factories = new();

    public static readonly Dictionary<string, string[]> RequiredParts = new();

    /// <summary>Keys declared inside a template's own resources, which the resolver check skips.</summary>
    public static readonly Dictionary<string, string[]> TemplateScopedKeys = new();

    /// <summary>Controls whose Background is transparent or null by design.</summary>
    public static readonly HashSet<string> TransparentByDesign = new();

    static ThemedControls()
    {
        AddRoots();
        AddButtons();
        AddInputs();
        AddItems();
        AddContainers();
        AddPopups();
        AddPickers();
        AddPages();
        AddMisc();
    }

    static partial void AddRoots();
    static partial void AddButtons();
    static partial void AddInputs();
    static partial void AddItems();
    static partial void AddContainers();
    static partial void AddPopups();
    static partial void AddPickers();
    static partial void AddPages();
    static partial void AddMisc();

    public static void Add(string name, Func<Control> factory, string[]? parts = null, bool transparent = false, string[]? scopedKeys = null)
    {
        Factories[name] = factory;
        if (parts is not null)
        {
            RequiredParts[name] = parts;
        }

        if (transparent)
        {
            TransparentByDesign.Add(name);
        }

        if (scopedKeys is not null)
        {
            TemplateScopedKeys[name] = scopedKeys;
        }
    }

    public static TheoryData<string> Names => new(Factories.Keys);

    /// <summary>name × {Light, Dark} × {Default, Compact}.</summary>
    public static TheoryData<string, string, EnyoDensity> Matrix
    {
        get
        {
            var data = new TheoryData<string, string, EnyoDensity>();
            foreach (var name in Factories.Keys)
            {
                foreach (var variant in ThemeTestHelpers.Variants)
                {
                    foreach (var density in ThemeTestHelpers.Densities)
                    {
                        data.Add(name, variant, density);
                    }
                }
            }

            return data;
        }
    }
}
