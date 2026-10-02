using System;
using System.Collections.Generic;
using Avalonia.Controls;
using AvaEnyo.Gallery.Pages.Widgets;

namespace AvaEnyo.Gallery;

public static partial class GalleryPages
{
    static partial void AddWidgets(List<GalleryPage> pages)
    {
        const string g = "Widgets";
        void Add(string name, string file, string summary, Func<Control> create, string notes) =>
            pages.Add(new(g, name, summary, create, $"src/Widgets/{file}Page.axaml", Notes: notes));

        Add("Buttons", "Buttons", "Every button class, custom tints and disabled", () => new ButtonsPage(),
            "Buttons draw the button-up.png frame over their Background, so any Background tints them. Pressed shows the inner shadow of button-down.png, with no scale.");
        Add("Activity & Progress Buttons", "ActivityButtons", "A spinner next to the caption, and button-styled progress", () => new ActivityButtonsPage(),
            "ActivityButton shows a Spinner while IsActive. ProgressButton fills a light bar over a dark base and raises Cancel from its cross.");
        Add("Check Boxes", "CheckBoxes", "Check boxes in rows, with labels and disabled", () => new CheckBoxesPage(),
            "As in Enyo, a check box toggles when it is pressed, not on release. GestureSettings.CheckBoxTogglesOnPress restores release.");
        Add("Toggle Switches", "ToggleSwitches", "ON/OFF switches in rows", () => new ToggleSwitchesPage(),
            "A switch toggles on tap, on a flick in the direction of travel, or on a drag over 15 px. The labels are uppercase.");
        Add("Radio Groups & Tabs", "RadioGroups", "Segmented choices, dark groups and tab groups", () => new RadioGroupsPage(),
            "A RadioGroup lays its segments in an ordered panel, so the first and last get rounded ends. Value defaults to the index.");
        Add("List Selectors", "ListSelectors", "ComboBox in the Enyo ListSelector look", () => new ListSelectorsPage(),
            "The value sits inline with a small arrow. The list opens so the selected row covers the control where space allows.");
        Add("Sliders & ProgressSlider", "Sliders", "Sliders and the progress slider with a buffered bar", () => new SlidersPage(),
            "A tap on the track animates the handle to the tapped point in 350 ms. A press on a slider keeps a parent scroller from taking the drag.");
        Add("Progress Bars", "ProgressBars", "Animated, item and indeterminate progress", () => new ProgressBarsPage(),
            "With enyo:Progress.Animate each value change slides over 350 ms with a cubic ease-out.");
        Add("Spinners", "Spinners", "Normal, large and light spinners", () => new SpinnersPage(),
            "A spinner turns once a second and is hidden until IsActive.");
        Add("Pickers", "Pickers", "Pill pickers, integer pickers and picker groups", () => new PickersPage(),
            "A picker opens a list over a scrim. While a group's list is open, tapping a sibling pill switches to it.");
        Add("Date & Time", "DateTime", "Enyo date and time pickers, and the restyled stock pickers", () => new DateTimePage(),
            "Field order and names come from the current culture.");
        Add("Text Fields", "TextFields", "Single and multi-line inputs, accessories, rounded and row groups", () => new TextFieldsPage(),
            "An input shows no frame until it has focus. Inside a RowGroup the focus frame follows the row's position.");
        Add("Tool Inputs", "ToolInputs", "Dark inputs for toolbars", () => new ToolInputsPage(),
            "The tool class draws input-tool.png: dark and translucent until focused, then white.");
        Add("Search", "Search", "The search input with its committed and cancelled events", () => new SearchPage(),
            "Committed fires 250 ms after typing stops, or at once on Enter. The clear button raises Cancelled.");
        Add("Token Input", "TokenInput", "Recipient chips with autocomplete", () => new TokenInputPage(),
            "A comma, semicolon, Enter or losing focus turns the text into a chip. FilterTextChanged drives the suggestions.");
        Add("Numeric Keypad", "NumericKeypad", "Phone and PIN keypads", () => new NumericKeypadPage(),
            "In Phone mode, holding 0 gives +. PIN mode has a backspace key.");
    }
}
