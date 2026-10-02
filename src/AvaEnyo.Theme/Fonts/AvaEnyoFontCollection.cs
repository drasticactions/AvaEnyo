using System;
using Avalonia.Media.Fonts;

namespace AvaEnyo;

/// <summary>
/// The embedded font collection <c>fonts:AvaEnyo</c>: Source Sans 3 Regular, Italic, Light, SemiBold and Bold
/// (static faces, for predictable shaping in the browser). Register it with
/// <see cref="AppBuilderExtensions.WithAvaEnyoFonts"/>.
/// </summary>
public sealed class AvaEnyoFontCollection : EmbeddedFontCollection
{
    /// <summary>The collection key, <c>fonts:AvaEnyo</c>.</summary>
    public static readonly Uri CollectionKey = new("fonts:AvaEnyo", UriKind.Absolute);

    /// <summary>The folder of the embedded faces.</summary>
    public static readonly Uri Source = new("avares://AvaEnyo.Theme/Assets/Fonts", UriKind.Absolute);

    /// <summary>The family name of the bundled face.</summary>
    public const string FamilyName = "Source Sans 3";

    /// <summary>Initializes the collection and loads the embedded faces.</summary>
    public AvaEnyoFontCollection() : base(CollectionKey, Source)
    {
    }
}
