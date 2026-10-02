using Avalonia.Styling;

namespace AvaEnyo;

/// <summary>
/// A resource entry that resolves to another key at lookup time. The per-control keys (Layer 3) are expressed with
/// these, so the Compact overlay, which redefines the Layer 2 target, is honored. A <c>StaticResource</c> alias would
/// capture the Default object at load time.
/// </summary>
/// <remarks>
/// Only <see cref="AvaEnyoTheme"/> resolves aliases. Use them only in dictionaries merged into the theme.
/// </remarks>
public sealed class ResourceAlias
{
    /// <summary>Initializes an empty alias.</summary>
    public ResourceAlias()
    {
    }

    /// <summary>Initializes an alias to <paramref name="target"/>.</summary>
    /// <param name="target">The key the alias resolves to.</param>
    public ResourceAlias(string target)
    {
        Target = target;
    }

    /// <summary>The key this alias resolves to.</summary>
    public string Target { get; set; } = string.Empty;

    /// <summary>Optional: the key to resolve to instead of <see cref="Target"/> under the Dark variant.</summary>
    public string? DarkTarget { get; set; }

    internal string TargetFor(ThemeVariant? theme) =>
        DarkTarget is not null && theme == ThemeVariant.Dark ? DarkTarget : Target;
}
