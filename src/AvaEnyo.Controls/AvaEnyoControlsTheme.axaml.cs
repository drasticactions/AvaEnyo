using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using AvaEnyo.Controls.Primitives;

namespace AvaEnyo.Controls;

/// <summary>
/// Adds the control themes of <c>AvaEnyo.Controls</c>. Add it after <see cref="AvaEnyoTheme"/>:
/// <code>
/// &lt;Application.Styles&gt;
///   &lt;enyo:AvaEnyoTheme /&gt;
///   &lt;enyo:AvaEnyoControlsTheme /&gt;
/// &lt;/Application.Styles&gt;
/// </code>
/// </summary>
public class AvaEnyoControlsTheme : Styles, IResourceNode
{
    private readonly IResourceProvider _compact;
    private AvaEnyoTheme? _theme;

    /// <summary>Initializes a new instance of the <see cref="AvaEnyoControlsTheme"/> class.</summary>
    /// <param name="sp">The service provider of the parent.</param>
    public AvaEnyoControlsTheme(IServiceProvider? sp = null)
    {
        AvaloniaXamlLoader.Load(sp, this);
        _compact = Resources["CompactMetrics"] as IResourceProvider
            ?? throw new KeyNotFoundException("AvaEnyoControlsTheme was loaded without its CompactMetrics dictionary.");
        Resources.Remove("CompactMetrics");
        EnyoGestures.EnsureRegistered();
        OwnerChanged += OnOwnerChanged;
    }

    private void OnOwnerChanged(object? sender, EventArgs e)
    {
        if (Owner is null)
        {
            return;
        }

        var theme = FindTheme(Owner)
            ?? throw new InvalidOperationException("AvaEnyoControlsTheme requires AvaEnyoTheme to be added first.");
        _theme = theme;
        theme.RegisterDensityOverlay(_compact);
    }

    bool IResourceNode.TryGetResource(object key, ThemeVariant? theme, out object? value)
    {
        // Styles added later are searched first, so the compact overlay of this theme is checked here too; otherwise
        // this theme's default value would answer before AvaEnyoTheme saw the overlay.
        if (_theme?.Density == EnyoDensity.Compact && _compact.TryGetResource(key, theme, out value))
        {
            return true;
        }

        return base.TryGetResource(key, theme, out value);
    }

    private AvaEnyoTheme? FindTheme(IResourceHost owner)
    {
        if (owner is Application app)
        {
            return Find(app.Styles);
        }

        if (owner is StyledElement element)
        {
            for (StyledElement? e = element; e is not null; e = e.Parent)
            {
                if (Find(e.Styles) is { } t)
                {
                    return t;
                }
            }

            return Application.Current is { } current ? Find(current.Styles) : null;
        }

        return null;
    }

    private AvaEnyoTheme? Find(IEnumerable<IStyle> styles) => FindThemeBefore(styles, this);

    /// <summary>
    /// The <see cref="AvaEnyoTheme"/> in <paramref name="styles"/> that comes before <paramref name="self"/>, or null
    /// when <paramref name="self"/> comes first or there is none.
    /// </summary>
    internal static AvaEnyoTheme? FindThemeBefore(IEnumerable<IStyle> styles, IStyle self)
    {
        foreach (var style in styles)
        {
            if (ReferenceEquals(style, self))
            {
                return null;
            }

            if (style is AvaEnyoTheme theme)
            {
                return theme;
            }

            if (style is Styles nested && nested.OfType<AvaEnyoTheme>().FirstOrDefault() is { } inner)
            {
                return inner;
            }
        }

        return null;
    }
}
