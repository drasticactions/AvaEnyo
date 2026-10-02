using Avalonia;

namespace AvaEnyo;

/// <summary><see cref="AppBuilder"/> extensions for AvaEnyo.</summary>
public static class AppBuilderExtensions
{
    /// <summary>
    /// Registers the <see cref="AvaEnyoFontCollection"/>, so the <c>fonts:AvaEnyo#Source Sans 3</c> family of the theme
    /// resolves. Without it the theme falls back to Prelude, Segoe UI, Arial or the system sans-serif.
    /// </summary>
    /// <param name="builder">The app builder.</param>
    /// <returns>The same builder.</returns>
    public static AppBuilder WithAvaEnyoFonts(this AppBuilder builder) =>
        builder.ConfigureFonts(fontManager => fontManager.AddFontCollection(new AvaEnyoFontCollection()));
}
