using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using AvaEnyo.Animations;
using AvaEnyo.Controls;

namespace AvaEnyo.Gallery;

/// <summary>
/// The gallery settings, in the app menu: theme, density, slow motion, animations and About. The menu opens from the
/// header title (as webOS opened it from the status bar), Ctrl+` and F10.
/// </summary>
public static class GallerySettings
{
    public static bool IsDark
    {
        get => Application.Current!.ActualThemeVariant == ThemeVariant.Dark;
        set => Application.Current!.RequestedThemeVariant = value ? ThemeVariant.Dark : ThemeVariant.Light;
    }

    public static bool IsCompact
    {
        get => App.Theme.Density == EnyoDensity.Compact;
        set => App.Theme.Density = value ? EnyoDensity.Compact : EnyoDensity.Default;
    }

    public static bool SlowMotion
    {
        get => EnyoAnimations.TimeScale > 1;
        set => EnyoAnimations.TimeScale = value ? 5 : 1;
    }

    public static bool AnimationsOff
    {
        get => !EnyoAnimations.IsEnabled;
        set => EnyoAnimations.IsEnabled = !value;
    }

    /// <summary>Builds the app menu. Item captions name the state the item switches to.</summary>
    public static AppMenu BuildMenu(Panel dialogHost)
    {
        var theme = new AppMenuItem { Value = "theme" };
        var density = new AppMenuItem { Value = "density" };
        var slow = new AppMenuItem { Value = "slow" };
        var anim = new AppMenuItem { Value = "anim" };
        var about = new AppMenuItem { Header = "About AvaEnyo", Value = "about" };
        var menu = new AppMenu { Items = { theme, density, slow, anim, about } };

        void Refresh()
        {
            theme.Header = IsDark ? "Light Theme" : "Dark Theme";
            density.Header = IsCompact ? "Default Density" : "Compact Density";
            slow.Header = SlowMotion ? "Normal Motion" : "Slow Motion ×5";
            anim.Header = AnimationsOff ? "Enable Animations" : "Disable Animations";
        }

        Refresh();
        menu.BeforeOpen += (_, _) => Refresh();
        menu.Closed += (_, e) =>
        {
            if (e.Reason != PopupCloseReason.Handle)
            {
                return;
            }

            switch (e.Value as string)
            {
                case "theme": IsDark = !IsDark; break;
                case "density": IsCompact = !IsCompact; break;
                case "slow": SlowMotion = !SlowMotion; break;
                case "anim": AnimationsOff = !AnimationsOff; break;
                case "about": _ = ShowAbout(dialogHost); break;
            }
        };
        return menu;
    }

    private static async System.Threading.Tasks.Task ShowAbout(Panel host)
    {
        var ok = new Button { Content = "OK", Classes = { "affirmative" }, HorizontalAlignment = HorizontalAlignment.Stretch };
        AvaEnyo.Controls.Popup.SetCloseValue(ok, true);
        var dialog = new ModalDialog
        {
            Caption = "AvaEnyo",
            Content = new StackPanel
            {
                Spacing = 6,
                Children =
                {
                    new TextBlock { Text = "The webOS Enyo 1.0 Onyx look and motion for Avalonia.", TextWrapping = TextWrapping.Wrap, Classes = { "enyo-text-body" } },
                    new TextBlock { Text = "Demos on the gallery site: Mail at ./mail/ and Photos at ./photos/.", TextWrapping = TextWrapping.Wrap, Classes = { "enyo-footnote" } },
                    ok,
                },
            },
        };
        host.Children.Add(dialog);
        await dialog.ShowAsync();
        host.Children.Remove(dialog);
    }
}
