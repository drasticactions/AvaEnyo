namespace AvaEnyo;

/// <summary>
/// The resource keys of the vector glyphs in <c>Glyphs/EnyoGlyphs.axaml</c>, redrawn from the glyph-like Onyx
/// images. Each is a <c>StreamGeometry</c>; draw it with a <c>PathIcon</c> or a <c>Path</c> with
/// <c>Stretch="Uniform"</c>.
/// </summary>
public static class EnyoGlyph
{
    /// <summary>The magnifier of the search input (<c>search-input-search.png</c>).</summary>
    public const string Search = "EnyoGlyphSearch";

    /// <summary>The clear cross of the search input (<c>search-input-cancel.png</c>).</summary>
    public const string Cancel = "EnyoGlyphCancel";

    /// <summary>The check mark of menu check items (<c>checkmark.png</c>) and check boxes.</summary>
    public const string Checkmark = "EnyoGlyphCheckmark";

    /// <summary>A right-pointing chevron (<c>menuitem-arrow.png</c>, collapsed).</summary>
    public const string ChevronRight = "EnyoGlyphChevronRight";

    /// <summary>A down-pointing chevron (<c>menuitem-arrow.png</c>, expanded).</summary>
    public const string ChevronDown = "EnyoGlyphChevronDown";

    /// <summary>A left-pointing chevron, for back and previous.</summary>
    public const string ChevronLeft = "EnyoGlyphChevronLeft";

    /// <summary>An up-pointing chevron.</summary>
    public const string ChevronUp = "EnyoGlyphChevronUp";

    /// <summary>The small down triangle of the list selector (<c>menu-arrow.png</c>).</summary>
    public const string MenuArrow = "EnyoGlyphMenuArrow";

    /// <summary>The up triangle of a menu's top scroll fade (<c>menu-fade-arrow-up.png</c>).</summary>
    public const string FadeArrowUp = "EnyoGlyphFadeArrowUp";

    /// <summary>The down triangle of a menu's bottom scroll fade (<c>menu-fade-arrow-down.png</c>).</summary>
    public const string FadeArrowDown = "EnyoGlyphFadeArrowDown";

    /// <summary>The play-style triangle inside the collapsible divider knob (<c>divider-arrow.png</c>).</summary>
    public const string DividerArrow = "EnyoGlyphDividerArrow";

    /// <summary>The three bars of the grab button (<c>grabbutton.png</c>).</summary>
    public const string Grab = "EnyoGlyphGrab";

    /// <summary>A back arrow for page headers.</summary>
    public const string Back = "EnyoGlyphBack";

    /// <summary>A plus sign.</summary>
    public const string Plus = "EnyoGlyphPlus";

    /// <summary>A minus sign.</summary>
    public const string Minus = "EnyoGlyphMinus";

    /// <summary>A backspace key, for the numeric keypad.</summary>
    public const string Backspace = "EnyoGlyphBackspace";

    /// <summary>A window close cross.</summary>
    public const string Close = "EnyoGlyphClose";

    /// <summary>A window maximize square.</summary>
    public const string Maximize = "EnyoGlyphMaximize";

    /// <summary>A window restore pair of squares.</summary>
    public const string Restore = "EnyoGlyphRestore";

    /// <summary>A window minimize bar.</summary>
    public const string Minimize = "EnyoGlyphMinimize";

    /// <summary>A folder, for file choosers.</summary>
    public const string Folder = "EnyoGlyphFolder";

    /// <summary>A document, for file choosers.</summary>
    public const string File = "EnyoGlyphFile";

    /// <summary>A circled exclamation, for errors.</summary>
    public const string Error = "EnyoGlyphError";

    /// <summary>A circled i, for information.</summary>
    public const string Info = "EnyoGlyphInfo";

    /// <summary>A refresh arrow, for pull to refresh.</summary>
    public const string Refresh = "EnyoGlyphRefresh";

    /// <summary>The overflow dots of a command bar.</summary>
    public const string More = "EnyoGlyphMore";

    /// <summary>A three-line menu.</summary>
    public const string Hamburger = "EnyoGlyphHamburger";

    /// <summary>Every glyph key.</summary>
    public static readonly string[] All =
    [
        Search, Cancel, Checkmark, ChevronRight, ChevronDown, ChevronLeft, ChevronUp, MenuArrow, FadeArrowUp,
        FadeArrowDown, DividerArrow, Grab, Back, Plus, Minus, Backspace, Close, Maximize, Restore, Minimize, Folder,
        File, Error, Info, Refresh, More, Hamburger,
    ];
}
