using AvaEnyo.Animations;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;

[assembly: AvaloniaTestApplication(typeof(AvaEnyo.Theme.Tests.TestApplication))]
[assembly: AvaloniaTestIsolation(AvaloniaTestIsolationLevel.PerTest)]

namespace AvaEnyo.Theme.Tests;

public class TestApplication : Application
{
    public TestApplication()
    {
        EnyoAnimations.TimeScale = 0;
        Theme = new AvaEnyoTheme();
        Styles.Add(Theme);
    }

    public AvaEnyoTheme Theme { get; }

    public static TestApplication Instance => (TestApplication)Current!;

    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<TestApplication>()
        .UseSkia()
        .UseHarfBuzz()
        .WithAvaEnyoFonts()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
}
