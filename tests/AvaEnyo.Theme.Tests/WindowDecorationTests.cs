using System;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Xunit;

namespace AvaEnyo.Theme.Tests;

public class WindowDecorationTests
{
    [AvaloniaFact]
    public void Use_Enyo_Decorations_Extends_The_Client_Area()
    {
        var window = new Window();
        EnyoWindow.SetUseEnyoDecorations(window, true);
        Assert.Equal(!OperatingSystem.IsMacOS(), window.ExtendClientAreaToDecorationsHint);
        EnyoWindow.SetUseEnyoDecorations(window, false);
        Assert.False(window.ExtendClientAreaToDecorationsHint);
    }

    [AvaloniaFact]
    public void App_Menu_Button_Raises_App_Menu_Requested_On_Its_Window()
    {
        var button = new Button();
        EnyoWindow.SetOpensAppMenu(button, true);
        var window = ThemeTestHelpers.Host(button, "Light", EnyoDensity.Default);
        var raised = 0;
        EnyoWindow.AddAppMenuRequestedHandler(window, (_, _) => raised++);
        var click = new RoutedEventArgs(Button.ClickEvent);
        button.RaiseEvent(click);
        Assert.Equal(1, raised);
        Assert.True(click.Handled);
        window.Close();
    }

    [AvaloniaFact]
    public void Plain_Button_Does_Not_Request_The_App_Menu()
    {
        var button = new Button();
        var window = ThemeTestHelpers.Host(button, "Light", EnyoDensity.Default);
        var raised = 0;
        EnyoWindow.AddAppMenuRequestedHandler(window, (_, _) => raised++);
        button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.Equal(0, raised);
        window.Close();
    }
}
