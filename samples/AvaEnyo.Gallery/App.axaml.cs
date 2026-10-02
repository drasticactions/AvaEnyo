using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace AvaEnyo.Gallery;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public static AvaEnyoTheme Theme => Current!.Styles.OfType<AvaEnyoTheme>().First();

    /// <summary>Set by the Desktop head before start; creates the window that hosts <see cref="MainView"/>.</summary>
    public static Func<Window>? DesktopWindowFactory { get; set; }

    public override void OnFrameworkInitializationCompleted()
    {
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                desktop.MainWindow = DesktopWindowFactory?.Invoke() ?? new Window { Content = new MainView(), Width = 1024, Height = 768 };
                break;
            case ISingleViewApplicationLifetime single:
                single.MainView = new MainView();
                break;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
