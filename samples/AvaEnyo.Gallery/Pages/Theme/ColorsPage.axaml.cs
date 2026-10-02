using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using AvaEnyo.Controls;

namespace AvaEnyo.Gallery.Pages.Theme;

public partial class ColorsPage : UserControl
{
    /// <summary>The palette (Layer 1) keys shown as swatches.</summary>
    private static readonly string[] s_palette =
    [
        "EnyoBodyColor", "EnyoTextColor", "EnyoChromeColor", "EnyoChromeLightColor", "EnyoHeaderColor", "EnyoHeaderTextColor",
        "EnyoLabelColor", "EnyoDividerCaptionColor", "EnyoAlphaDividerColor", "EnyoSubtextColor", "EnyoFootnoteColor",
        "EnyoErrorColor", "EnyoAffirmativeColor", "EnyoNegativeColor", "EnyoBlueColor", "EnyoGrayColor", "EnyoHeldTopColor",
        "EnyoHeldBottomColor", "EnyoPlaceholderColor", "EnyoPopupTopColor", "EnyoPopupBottomColor", "EnyoAppMenuTopColor",
    ];

    /// <summary>Semantic (Layer 2) brushes shown as swatches; gradients show as drawn.</summary>
    private static readonly string[] s_brushes =
    [
        "EnyoButtonBackgroundBrush", "EnyoButtonSheenBrush", "EnyoToolbarBackgroundBrush", "EnyoToolbarSheenBrush",
        "EnyoHeaderSheenBrush", "EnyoItemHeldBrush", "EnyoDividerRuleBrush", "EnyoGroupBandBrush", "EnyoCheckOnBrush",
        "EnyoToggleOffBrush", "EnyoSegmentBrush", "EnyoSegmentOnBrush", "EnyoTrackBrush", "EnyoTrackFillBrush",
        "EnyoHandleBrush", "EnyoProgressFillBrush", "EnyoPopupBackgroundBrush", "EnyoMenuBackgroundBrush",
        "EnyoDialogBackgroundBrush", "EnyoAppMenuBackgroundBrush", "EnyoPillSheenBrush", "EnyoSlidingShadowBrush",
    ];

    public ColorsPage()
    {
        InitializeComponent();
        Fill(LightHost);
        Fill(DarkHost);
    }

    private static void Fill(Panel host)
    {
        host.Children.Add(new Divider { Caption = "Palette" });
        var palette = new WrapPanel();
        foreach (var key in s_palette)
        {
            palette.Children.Add(Swatch(key, isColor: true));
        }

        host.Children.Add(palette);
        host.Children.Add(new Divider { Caption = "Brushes" });
        var brushes = new WrapPanel();
        foreach (var key in s_brushes)
        {
            brushes.Children.Add(Swatch(key, isColor: false));
        }

        host.Children.Add(brushes);
        host.Children.Add(new Divider { Caption = "In use" });
        host.Children.Add(new WrapPanel
        {
            Children =
            {
                new Button { Content = "Button" },
                new Button { Content = "Affirmative", Classes = { "affirmative" } },
                new CheckBox { IsChecked = true },
                new ToggleSwitch { IsChecked = true },
            },
        });
        host.Children.Add(new Item { Content = "An item", IsSelected = true });
        host.Children.Add(new TextBlock { Text = "Label", Classes = { "enyo-label" } });
        host.Children.Add(new TextBlock { Text = "Subtext", Classes = { "enyo-subtext" } });
    }

    private static Control Swatch(string key, bool isColor)
    {
        var chip = new Border { Width = 72, Height = 36, CornerRadius = new CornerRadius(4), BorderThickness = new Thickness(1), BorderBrush = new SolidColorBrush(Color.FromArgb(0x40, 0, 0, 0)) };
        if (isColor)
        {
            chip.Bind(Border.BackgroundProperty, chip.GetResourceObservable(key, v => v is Color c ? new SolidColorBrush(c) : null));
        }
        else
        {
            chip.Bind(Border.BackgroundProperty, chip.GetResourceObservable(key));
        }

        var label = new TextBlock
        {
            Text = key.Replace("Enyo", string.Empty).Replace("Brush", string.Empty).Replace("Color", string.Empty),
            FontSize = 11,
            Width = 76,
            TextTrimming = TextTrimming.CharacterEllipsis,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        label.Bind(TextBlock.ForegroundProperty, label.GetResourceObservable("EnyoTextBrush"));
        ToolTip.SetTip(chip, key);
        return new StackPanel { Margin = new Thickness(0, 0, 6, 6), Children = { chip, label } };
    }
}
