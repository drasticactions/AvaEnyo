namespace AvaEnyo;

/// <summary>What started a back request.</summary>
public enum BackSource
{
    /// <summary>Code called <see cref="EnyoBack.RequestBack(Avalonia.Controls.TopLevel)"/>.</summary>
    Code,

    /// <summary>The Escape key.</summary>
    Escape,

    /// <summary>The system back: a back button, Alt+Left, the mouse back button.</summary>
    System,
}
