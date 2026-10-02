using System;
using Avalonia.Controls;

namespace AvaEnyo.Gallery;

/// <summary>One sample page of the gallery.</summary>
/// <param name="Group">The index section.</param>
/// <param name="Name">The page name, also used by <c>--page</c>.</param>
/// <param name="Summary">One line under the name.</param>
/// <param name="Create">Builds the page.</param>
/// <param name="SourcePath">The embedded XAML resource, <c>src/Group/NamePage.axaml</c>.</param>
/// <param name="Scrolls">False for pages that fill the view and scroll themselves.</param>
/// <param name="Notes">Text for the Notes tab.</param>
public sealed record GalleryPage(string Group, string Name, string Summary, Func<Control> Create, string SourcePath, bool Scrolls = true, string? Notes = null);
