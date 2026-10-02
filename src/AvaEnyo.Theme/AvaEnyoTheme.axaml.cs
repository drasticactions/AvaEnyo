using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace AvaEnyo;

/// <summary>
/// Adds the Enyo 1.0 Onyx theme to an application.
/// </summary>
/// <remarks>
/// <code>
/// &lt;Application.Styles&gt;
///   &lt;enyo:AvaEnyoTheme Density="Default" /&gt;
///   &lt;enyo:AvaEnyoControlsTheme /&gt;   &lt;!-- from AvaEnyo.Controls, optional --&gt;
/// &lt;/Application.Styles&gt;
/// </code>
/// Call <see cref="AppBuilderExtensions.WithAvaEnyoFonts"/> on the <c>AppBuilder</c>, so Source Sans 3 loads.
/// The palette is fixed. To change a color or metric, redefine its <c>Enyo*</c> key in
/// <c>Application.Resources</c>; that definition wins over the theme.
/// </remarks>
public class AvaEnyoTheme : Styles, IResourceNode
{
    private readonly List<IResourceProvider> _overlays = new();
    private EnyoDensity _density = EnyoDensity.Default;

    /// <summary>Initializes a new instance of the <see cref="AvaEnyoTheme"/> class.</summary>
    /// <param name="sp">The service provider of the parent.</param>
    public AvaEnyoTheme(IServiceProvider? sp = null)
    {
        AvaloniaXamlLoader.Load(sp, this);

        var compact = Resources["CompactMetrics"] as IResourceProvider
            ?? throw new KeyNotFoundException("AvaEnyoTheme was loaded without its CompactMetrics dictionary.");
        Resources.Remove("CompactMetrics");
        _overlays.Add(compact);

        OwnerChanged += (_, _) => SyncOverlayOwners();
        ThemeBehaviors.EnsureRegistered();
    }

    /// <summary>Defines the <see cref="Density"/> property.</summary>
    public static readonly DirectProperty<AvaEnyoTheme, EnyoDensity> DensityProperty =
        AvaloniaProperty.RegisterDirect<AvaEnyoTheme, EnyoDensity>(nameof(Density), o => o.Density, (o, v) => o.Density = v);

    /// <summary>
    /// The metric set: <see cref="EnyoDensity.Default"/> matches Enyo (20 px base font), and
    /// <see cref="EnyoDensity.Compact"/> swaps in tighter, hand-tuned metrics. Changing it re-resolves every
    /// <c>DynamicResource</c> at once.
    /// </summary>
    public EnyoDensity Density
    {
        get => _density;
        set => SetAndRaise(DensityProperty, ref _density, value);
    }

    /// <summary>
    /// Registers one more density overlay. <c>AvaEnyo.Controls</c> uses it for the compact metrics of its own
    /// keys. When <see cref="Density"/> is <see cref="EnyoDensity.Compact"/>, overlays are searched newest first,
    /// before the theme resources.
    /// </summary>
    internal void RegisterDensityOverlay(IResourceProvider overlay)
    {
        if (_overlays.Contains(overlay))
        {
            return;
        }

        _overlays.Add(overlay);
        if (Owner is { } owner)
        {
            overlay.AddOwner(owner);
        }

        NotifyChanged();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == DensityProperty)
        {
            NotifyChanged();
        }
    }

    private void NotifyChanged() => Owner?.NotifyHostedResourcesChanged(ResourcesChangedEventArgs.Create());

    private void SyncOverlayOwners()
    {
        // The overlays are not in Resources, so they are not always active. Their DynamicResource references still
        // need a host to resolve against, so they get the same owner as the theme.
        foreach (var overlay in _overlays)
        {
            if (overlay.Owner is { } old && old != Owner)
            {
                overlay.RemoveOwner(old);
            }

            if (Owner is { } owner && overlay.Owner != owner)
            {
                overlay.AddOwner(owner);
            }
        }
    }

    bool IResourceNode.TryGetResource(object key, ThemeVariant? theme, out object? value) =>
        TryGetResourceCore(key, theme, out value, 0);

    private bool TryGetResourceCore(object key, ThemeVariant? theme, out object? value, int depth)
    {
        if (_density == EnyoDensity.Compact)
        {
            for (var i = _overlays.Count - 1; i >= 0; --i)
            {
                if (_overlays[i].TryGetResource(key, theme, out value))
                {
                    return Unalias(theme, ref value, depth);
                }
            }
        }

        if (base.TryGetResource(key, theme, out value))
        {
            return Unalias(theme, ref value, depth);
        }

        return false;
    }

    private bool Unalias(ThemeVariant? theme, ref object? value, int depth)
    {
        if (value is ResourceAlias alias)
        {
            if (depth >= 8)
            {
                throw new InvalidOperationException($"Resource alias chain too deep at '{alias.Target}'.");
            }

            return TryGetResourceCore(alias.TargetFor(theme), theme, out value, depth + 1);
        }

        return true;
    }
}
