using Avalonia.Controls;

namespace AvaEnyo.Controls.Tests;

public static partial class ThemedControls
{
    static partial void AddPickers()
    {
        Add("PickerButton", new Row("Pickers", () => new PickerButton { Content = "Mail", IsFocusOpen = true }, ["PART_Frame", "PART_ContentPresenter"], false));
        Add("Picker", new Row("Pickers", () => new Picker { ItemsSource = new[] { "Mail", "IM", "Text" }, SelectedIndex = 1 }, ["PART_Button", "PART_Popup"], true));
        Add("IntegerPicker", new Row("Pickers", () => new IntegerPicker { Minimum = 1, Maximum = 5, Value = 3 }, ["PART_Button", "PART_Popup"], true));
        Add("PickerGroup", new Row("Pickers", () => new PickerGroup { Label = "Pref", Items = { new Picker { ItemsSource = new[] { "a", "b" } } } }, ["PART_Label", "PART_ItemsPresenter", "PART_Overlay"], true));
        Add("EnyoDatePicker", new Row("Pickers", () => new EnyoDatePicker { Label = "Date" }, ["PART_ItemsPresenter", "PART_Month", "PART_Day", "PART_Year"], true));
        Add("EnyoTimePicker", new Row("Pickers", () => new EnyoTimePicker { Label = "Time", Is24Hour = false }, ["PART_ItemsPresenter", "PART_Hour", "PART_Minute", "PART_Period"], true));
        Add("Drawer", new Row("Drawer", () => new Drawer { Header = "Header", Content = new TextBlock { Text = "x" } }, ["PART_Header", "PART_ContentHost", "PART_ContentPresenter"], true));
        Add("DividerDrawer", new Row("Drawer", () => new DividerDrawer { Caption = "Section", Content = new TextBlock { Text = "x" } }, ["PART_Header", "PART_ContentHost", "PART_Knob", "PART_Arrow"], true));
        Add("ProgressSlider", new Row("Progress", () => new ProgressSlider { Value = 30, BarValue = 50, AltBarValue = 70 }, ["PART_Track", "PART_Bar", "PART_AltBar"], false));
        Add("ProgressButton", new Row("Progress", () => new ProgressButton { Value = 40, Content = "Copying", Classes = { "affirmative" } }, ["PART_Bar", "PART_CancelButton", "PART_ContentPresenter"], false));
    }
}
