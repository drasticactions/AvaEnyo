using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Xunit;

namespace AvaEnyo.Theme.Tests;

public class WindowDecorationTests
{
    [AvaloniaFact]
    public void Use_Enyo_Decorations_Extends_The_Client_Area()
    {
        var window = new Window();
        EnyoWindow.SetUseEnyoDecorations(window, true);
        Assert.True(window.ExtendClientAreaToDecorationsHint);
        EnyoWindow.SetUseEnyoDecorations(window, false);
        Assert.False(window.ExtendClientAreaToDecorationsHint);
    }

    [AvaloniaFact]
    public void Native_Caption_Bar_Shows_Only_With_Native_Caption_Buttons()
    {
        var window = ThemeTestHelpers.Host(new Border(), "Light", EnyoDensity.Default);
        EnyoWindow.SetUseEnyoDecorations(window, true);
        Assert.False(EnyoWindow.GetUsesNativeCaptionButtons(window));
        Assert.False(Part<Panel>(window, "PART_NativeCaptionBar").IsVisible);
        window.Close();
    }

    [AvaloniaFact]
    public void On_MacOS_The_Window_Draws_The_Bar_Beside_The_Traffic_Lights()
    {
        var isMacOS = EnyoWindow.IsMacOS;
        EnyoWindow.IsMacOS = () => true;
        try
        {
            var window = ThemeTestHelpers.Host(new Border(), "Light", EnyoDensity.Default);
            EnyoWindow.SetAppName(window, "Mail");
            EnyoWindow.SetUseEnyoDecorations(window, true);
            window.UpdateLayout();
            Assert.True(window.ExtendClientAreaToDecorationsHint);
            Assert.True(EnyoWindow.GetUsesNativeCaptionButtons(window));
            Assert.True(Part<Panel>(window, "PART_NativeCaptionBar").IsVisible);
            Assert.Equal("Mail", Part<TextBlock>(window, "PART_NativeCaptionAppNameText").Text);
            Assert.False(Part<Button>(window, "PART_NativeCaptionAppMenuButton").IsVisible);

            EnyoWindow.SetHasAppMenu(window, true);
            Assert.True(Part<Button>(window, "PART_NativeCaptionAppMenuButton").IsVisible);
            Assert.False(Part<TextBlock>(window, "PART_NativeCaptionAppNameText").IsVisible);

            EnyoWindow.SetUseEnyoDecorations(window, false);
            Assert.False(Part<Panel>(window, "PART_NativeCaptionBar").IsVisible);
            window.Close();
        }
        finally
        {
            EnyoWindow.IsMacOS = isMacOS;
        }
    }

    private static T Part<T>(Window window, string name) where T : Control =>
        window.GetVisualDescendants().OfType<T>().Single(c => c.Name == name);

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
