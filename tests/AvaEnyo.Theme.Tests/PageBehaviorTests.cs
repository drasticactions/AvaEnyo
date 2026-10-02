using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace AvaEnyo.Theme.Tests;

public class PageBehaviorTests
{
    [AvaloniaFact]
    public async Task Enyo_Back_Pops_One_Page()
    {
        var nav = new NavigationPage();
        var window = ThemeTestHelpers.Host(nav, "Light", EnyoDensity.Default);
        await nav.PushAsync(new ContentPage { Header = "Root" });
        await nav.PushAsync(new ContentPage { Header = "Second" });
        await nav.PushAsync(new ContentPage { Header = "Third" });
        Assert.Equal(3, nav.StackDepth);

        var handled = EnyoBack.RaiseFrom(nav);
        Dispatcher.UIThread.RunJobs();
        Assert.True(handled);
        Assert.Equal(2, nav.StackDepth);

        EnyoBack.RaiseFrom(nav);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(1, nav.StackDepth);

        // At the root nothing is popped and the request goes on to the app.
        Assert.False(EnyoBack.RaiseFrom(nav));
        Assert.Equal(1, nav.StackDepth);
        window.Close();
    }

    [AvaloniaFact]
    public void Navigation_Page_Uses_The_Pane_Fly_In()
    {
        var nav = new NavigationPage();
        var window = ThemeTestHelpers.Host(nav, "Light", EnyoDensity.Default);
        Assert.Same(AvaEnyo.Animations.EnyoPaneTransitions.FlyIn, nav.PageTransition);
        window.Close();
    }

    [AvaloniaTheory]
    [InlineData(true, true, true, 3)]
    [InlineData(true, true, false, 2)]
    [InlineData(false, true, true, 2)]
    [InlineData(false, true, false, 1)]
    public void Date_Picker_Shows_One_Pill_Per_Visible_Field(bool day, bool month, bool year, int expected)
    {
        var picker = new DatePicker { DayVisible = day, MonthVisible = month, YearVisible = year, SelectedDate = new DateTimeOffset(2011, 7, 1, 0, 0, 0, TimeSpan.Zero) };
        var window = ThemeTestHelpers.Host(picker, "Light", EnyoDensity.Default);
        var pills = picker.GetVisualDescendants().OfType<Border>()
            .Where(b => b.Classes.Contains("pill") && b.IsEffectivelyVisible)
            .ToList();
        Assert.Equal(expected, pills.Count);

        // Each pill sits in the column of its field text.
        foreach (var name in new[] { "Day", "Month", "Year" })
        {
            var text = picker.GetVisualDescendants().OfType<TextBlock>().First(t => t.Name == $"PART_{name}TextBlock");
            var pill = picker.GetVisualDescendants().OfType<Border>().First(b => b.Name == $"PART_{name}Pill");
            Assert.Equal(text.IsVisible, pill.IsVisible);
            Assert.Equal(Grid.GetColumn(text), Grid.GetColumn(pill));
        }

        window.Close();
    }

    [AvaloniaFact]
    public void Picker_Presenters_Template_With_Their_Parts()
    {
        var date = new DatePickerPresenter();
        var window = ThemeTestHelpers.Host(date, "Dark", EnyoDensity.Compact);
        var names = PartFinder.Collect(date);
        foreach (var part in new[] { "PART_AcceptButton", "PART_DismissButton", "PART_PickerContainer", "PART_DayHost", "PART_MonthHost",
                     "PART_YearHost", "PART_DaySelector", "PART_MonthSelector", "PART_YearSelector", "PART_FirstSpacer", "PART_SecondSpacer" })
        {
            Assert.Contains(part, names);
        }

        window.Close();

        var time = new TimePickerPresenter();
        window = ThemeTestHelpers.Host(time, "Light", EnyoDensity.Default);
        names = PartFinder.Collect(time);
        foreach (var part in new[] { "PART_AcceptButton", "PART_DismissButton", "PART_PickerContainer", "PART_HourSelector", "PART_MinuteSelector",
                     "PART_SecondHost", "PART_PeriodSelector", "PART_PeriodHost", "PART_SecondSpacer", "PART_ThirdSpacer" })
        {
            Assert.Contains(part, names);
        }

        TestApplication.Instance.Theme.Density = EnyoDensity.Default;
        window.Close();
    }

    [AvaloniaFact]
    public void Time_Picker_Hosts_Are_Pills_And_Hide_With_Their_Field()
    {
        var picker = new TimePicker { ClockIdentifier = "24HourClock", SelectedTime = new TimeSpan(14, 5, 0) };
        var window = ThemeTestHelpers.Host(picker, "Light", EnyoDensity.Default);
        var pills = picker.GetVisualDescendants().OfType<Border>()
            .Where(b => b.Classes.Contains("pill") && b.IsEffectivelyVisible)
            .ToList();
        Assert.Equal(2, pills.Count);
        window.Close();
    }
}
