using System;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace AvaEnyo.Services;

/// <summary>
/// The control themes of <c>AvaEnyo.Services</c> (the notification dashboard). Add it after the other AvaEnyo
/// themes:
/// <code>
/// &lt;Application.Styles&gt;
///   &lt;enyo:AvaEnyoTheme /&gt;
///   &lt;enyo:AvaEnyoControlsTheme /&gt;
///   &lt;enyo:AvaEnyoServicesTheme /&gt;
/// &lt;/Application.Styles&gt;
/// </code>
/// </summary>
public class AvaEnyoServicesTheme : Styles
{
    /// <summary>Initializes a new instance of the <see cref="AvaEnyoServicesTheme"/> class.</summary>
    /// <param name="sp">The service provider of the parent.</param>
    public AvaEnyoServicesTheme(IServiceProvider? sp = null)
    {
        AvaloniaXamlLoader.Load(sp, this);
    }
}
