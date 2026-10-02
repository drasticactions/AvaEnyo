using AvaEnyo.Animations;
using AvaEnyo.Controls;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;

[assembly: AvaloniaTestApplication(typeof(AvaEnyo.Services.Tests.TestApplication))]
[assembly: AvaloniaTestIsolation(AvaloniaTestIsolationLevel.PerTest)]

namespace AvaEnyo.Services.Tests;

public class TestApplication : Application
{
    public TestApplication()
    {
        EnyoAnimations.TimeScale = 0;
        Styles.Add(new AvaEnyoTheme());
        Styles.Add(new AvaEnyoControlsTheme());
        Styles.Add(new AvaEnyoServicesTheme());
    }

    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<TestApplication>()
        .UseSkia()
        .UseHarfBuzz()
        .WithAvaEnyoFonts()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
}
