using Avalonia;
using Avalonia.Controls;

namespace AvaEnyo.Gallery.Pages.Theme;

public partial class DensityPage : UserControl
{
    private static readonly string[] s_keys =
    [
        "EnyoBaseFontSize", "EnyoButtonFontSize", "EnyoButtonLineHeight", "EnyoButtonPadding", "EnyoButtonFrameThickness",
        "EnyoButtonMargin", "EnyoToolbarMinHeight", "EnyoHeaderMinHeight", "EnyoHeaderInnerPadding", "EnyoItemPadding",
        "EnyoInputFrameThickness", "EnyoCheckBoxWidth", "EnyoCheckBoxHeight", "EnyoToggleHeight", "EnyoSliderHandleSize",
        "EnyoSliderTrackHeight", "EnyoProgressBarHeight", "EnyoPickerPillHeight", "EnyoDividerMargin", "EnyoGroupFrame",
        "EnyoGroupLabeledTop", "EnyoPopupFrame", "EnyoGrabButtonWidth", "EnyoGrabButtonHeight", "EnyoSpinnerSize",
        "EnyoSpinnerLargeSize", "EnyoSlidingShadowWidth",
    ];

    public DensityPage()
    {
        InitializeComponent();
        Choice.SelectedIndex = App.Theme.Density == EnyoDensity.Compact ? 1 : 0;
        Choice.ValueChanged += (_, _) => App.Theme.Density = Choice.SelectedIndex == 1 ? EnyoDensity.Compact : EnyoDensity.Default;
        for (var i = 0; i < s_keys.Length; i++)
        {
            Table.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            var name = new TextBlock { Text = s_keys[i], Classes = { "enyo-text-body" } };
            var value = new TextBlock { Classes = { "enyo-text-body" }, Margin = new Thickness(16, 0, 0, 0) };
            value.Bind(TextBlock.TextProperty, value.GetResourceObservable(s_keys[i], v => v?.ToString()));
            Grid.SetRow(name, i);
            Grid.SetRow(value, i);
            Grid.SetColumn(value, 1);
            Table.Children.Add(name);
            Table.Children.Add(value);
        }
    }
}
