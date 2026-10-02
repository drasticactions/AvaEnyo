using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;

namespace AvaEnyo.Theme.Tests;

public static partial class ThemedControls
{
    static partial void AddPickers()
    {
        Add("DateTimePickerShared", () => new ListBoxItem { Content = "x", Theme = (ControlTheme)Avalonia.Application.Current!.FindResource("EnyoDateTimePickerItem")! },
            ["PART_ContentPresenter"], transparent: true);
        Add("DatePicker", () => new DatePicker { SelectedDate = new DateTimeOffset(2011, 7, 1, 0, 0, 0, TimeSpan.Zero) },
            ["PART_FlyoutButton", "PART_ButtonContentGrid", "PART_DayTextBlock", "PART_MonthTextBlock", "PART_YearTextBlock",
             "PART_FirstSpacer", "PART_SecondSpacer", "PART_Popup", "PART_PickerPresenter", "PART_DayPill", "PART_MonthPill", "PART_YearPill"]);
        Add("TimePicker", () => new TimePicker { SelectedTime = new TimeSpan(9, 30, 0) },
            ["PART_FlyoutButton", "PART_FlyoutButtonContentGrid", "PART_FirstPickerHost", "PART_SecondPickerHost", "PART_ThirdPickerHost",
             "PART_FourthPickerHost", "PART_HourTextBlock", "PART_MinuteTextBlock", "PART_SecondTextBlock", "PART_PeriodTextBlock",
             "PART_FirstColumnDivider", "PART_SecondColumnDivider", "PART_ThirdColumnDivider", "PART_Popup", "PART_PickerPresenter"]);
        Add("Calendar", () => new Calendar { DisplayDate = new DateTime(2011, 7, 1), SelectedDate = new DateTime(2011, 7, 8) },
            ["PART_Root", "PART_CalendarItem"]);
        Add("CalendarItem", () => new Calendar { DisplayDate = new DateTime(2011, 7, 1) },
            ["PART_HeaderButton", "PART_PreviousButton", "PART_NextButton", "PART_MonthView", "PART_YearView"]);
        Add("CalendarButton", () => new CalendarButton { Content = "Jul" }, ["PART_ContentPresenter"], transparent: true);
        Add("CalendarDayButton", () => new CalendarDayButton { Content = "8" }, ["PART_ContentPresenter"], transparent: true);
        Add("CalendarDatePicker", () => new CalendarDatePicker { SelectedDate = new DateTime(2011, 7, 8) },
            ["PART_TextBox", "PART_Button", "PART_Popup", "PART_Calendar"]);
    }
}
