using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace AvaEnyo.Theme.Tests;

public static partial class ThemedControls
{
    static partial void AddInputs()
    {
        Add("CheckBox", () => new CheckBox { Content = "x" }, ["PART_Border", "NormalRectangle", "CheckGlyph", "PART_ContentPresenter"], transparent: true);
        Add("RadioButton", () => new RadioButton { Content = "x" }, ["PART_Background", "PART_ContentPresenter", "PART_FocusVisual"]);
        Add("ToggleSwitch", () => new ToggleSwitch { Content = "x" },
            ["PART_ContentPresenter", "PART_OnContentPresenter", "PART_OffContentPresenter", "PART_SwitchKnob", "PART_MovingKnobs", "OuterBorder", "Knob"], transparent: true);
        Add("TextBox", () => new TextBox { Text = "x" },
            ["PART_BorderElement", "PART_TextPresenter", "PART_ScrollViewer", "PART_InnerDockPanel", "PART_Placeholder", "PART_FloatingPlaceholder"]);
        Add("Slider", () => new Slider { Value = 40 }, ["PART_Track", "PART_DecreaseButton", "PART_IncreaseButton", "TrackBackground"]);
        Add("ProgressBar", () => new ProgressBar { Value = 40 }, ["PART_Indicator", "IndeterminateStripes", "PART_LayoutTransformControl"]);
        Add("HyperlinkButton", () => new HyperlinkButton { Content = "x" }, ["PART_ContentPresenter"], transparent: true);
        Add("DropDownButton", () => new DropDownButton { Content = "x" }, ["PART_Background", "PART_Frame", "PART_ContentPresenter", "DropDownGlyph"]);
        Add("SplitButton", () => new SplitButton { Content = "x" }, ["PART_PrimaryButton", "PART_SecondaryButton"]);
        Add("ButtonSpinner", () => new ButtonSpinner { Content = "x" }, ["PART_IncreaseButton", "PART_DecreaseButton", "PART_ContentPresenter", "PART_SpinnerPanel"]);
        Add("NumericUpDown", () => new NumericUpDown { Value = 1 }, ["PART_Spinner", "PART_TextBox"]);
        Add("AutoCompleteBox", () => new AutoCompleteBox { ItemsSource = new[] { "apple", "avocado" } },
            ["PART_TextBox", "PART_Popup", "PART_SelectingItemsControl"]);
        Add("SelectableTextBlock", () => new SelectableTextBlock { Text = "x" }, transparent: true);
        Add("Label", () => new Label { Content = "x" }, ["PART_ContentPresenter"], transparent: true);
        Add("PathIcon", () => new PathIcon { Data = StreamGeometry.Parse("M0,0 L10,0 10,10 Z") }, transparent: true);
        Add("TextSelectionHandle", () => new TextSelectionHandle(), ["PART_HandlePathIcon", "PART_Indicator"]);
        Add("DataValidationErrors", () => new DataValidationErrors { Content = new TextBox() }, ["PART_ContentPresenter"], transparent: true);
    }
}
