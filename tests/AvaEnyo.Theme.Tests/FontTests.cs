using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Xunit;

namespace AvaEnyo.Theme.Tests;

public class FontTests
{
    [AvaloniaTheory]
    [InlineData(FontWeight.Light, FontStyle.Normal)]
    [InlineData(FontWeight.Normal, FontStyle.Normal)]
    [InlineData(FontWeight.Normal, FontStyle.Italic)]
    [InlineData(FontWeight.SemiBold, FontStyle.Normal)]
    [InlineData(FontWeight.Bold, FontStyle.Normal)]
    public void Every_Embedded_Face_Registers(FontWeight weight, FontStyle style)
    {
        var family = new FontFamily("fonts:AvaEnyo#Source Sans 3");
        var typeface = new Typeface(family, style, weight);
        Assert.True(FontManager.Current.TryGetGlyphTypeface(typeface, out var glyph), $"{weight}/{style} missing");
        Assert.Equal(weight, glyph!.Weight);
        Assert.Equal(style == FontStyle.Italic ? FontStyle.Italic : FontStyle.Normal, glyph.Style);
        Assert.Equal(FontSimulations.None, glyph.FontSimulations);
    }

    [AvaloniaFact]
    public void Collection_Has_No_Weight_Collision()
    {
        var collection = new AvaEnyoFontCollection();
        Assert.True(collection.TryGetFamilyTypefaces(AvaEnyoFontCollection.FamilyName, out var faces));
        Assert.Equal(5, faces!.Count);
        Assert.Equal(5, faces.Select(f => (f.Weight, f.Style)).Distinct().Count());
    }

    [AvaloniaFact]
    public void Theme_Font_Family_Resolves_To_Source_Sans()
    {
        var text = new TextBlock { Text = "Enyo" };
        var window = ThemeTestHelpers.Host(text, "Light", EnyoDensity.Default);
        Assert.Contains("Source Sans 3", text.FontFamily.ToString());
        Assert.Equal(20, text.FontSize);
        window.Close();
    }
}
