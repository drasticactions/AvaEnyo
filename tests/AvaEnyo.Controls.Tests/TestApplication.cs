using AvaEnyo.Animations;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;

[assembly: AvaloniaTestApplication(typeof(AvaEnyo.Controls.Tests.TestApplication))]
[assembly: AvaloniaTestIsolation(AvaloniaTestIsolationLevel.PerTest)]

namespace AvaEnyo.Controls.Tests;

public class TestApplication : Application
{
    public TestApplication()
    {
        EnyoAnimations.TimeScale = 0;
        Theme = new AvaEnyoTheme();
        Styles.Add(Theme);
        Styles.Add(new AvaEnyoControlsTheme());
    }

    public AvaEnyoTheme Theme { get; }

    public static TestApplication Instance => (TestApplication)Current!;

    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<TestApplication>()
        .UseSkia()
        .UseHarfBuzz()
        .WithAvaEnyoFonts()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
}
