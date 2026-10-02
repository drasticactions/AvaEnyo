using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;

namespace AvaEnyo.Controls;

/// <summary>
/// The Enyo date picker (<c>enyo.DatePicker</c>): a <see cref="PickerGroup"/> with one picker per field — month, day
/// and year — each opening its own list. The field order and month names come from
/// <see cref="CultureInfo.CurrentCulture"/>. Named with the prefix so it does not clash with Avalonia's
/// <c>DatePicker</c>, which AvaEnyo also restyles.
/// </summary>
public class EnyoDatePicker : PickerGroup
{
    /// <summary>Defines the <see cref="Value"/> property.</summary>
    public static readonly StyledProperty<DateTime> ValueProperty =
        AvaloniaProperty.Register<EnyoDatePicker, DateTime>(nameof(Value), DateTime.Today, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Defines the <see cref="HideDay"/> property.</summary>
    public static readonly StyledProperty<bool> HideDayProperty =
        AvaloniaProperty.Register<EnyoDatePicker, bool>(nameof(HideDay));

    /// <summary>Defines the <see cref="HideMonth"/> property.</summary>
    public static readonly StyledProperty<bool> HideMonthProperty =
        AvaloniaProperty.Register<EnyoDatePicker, bool>(nameof(HideMonth));

    /// <summary>Defines the <see cref="HideYear"/> property.</summary>
    public static readonly StyledProperty<bool> HideYearProperty =
        AvaloniaProperty.Register<EnyoDatePicker, bool>(nameof(HideYear));

    /// <summary>Defines the <see cref="MinYear"/> property.</summary>
    public static readonly StyledProperty<int> MinYearProperty =
        AvaloniaProperty.Register<EnyoDatePicker, int>(nameof(MinYear), 1900);

    /// <summary>Defines the <see cref="MaxYear"/> property.</summary>
    public static readonly StyledProperty<int> MaxYearProperty =
        AvaloniaProperty.Register<EnyoDatePicker, int>(nameof(MaxYear), 2099);

    private readonly Picker _month = new() { Name = "PART_Month" };
    private readonly IntegerPicker _day = new() { Name = "PART_Day", Minimum = 1, Maximum = 31 };
    private readonly IntegerPicker _year = new() { Name = "PART_Year" };
    private bool _syncing;

    static EnyoDatePicker()
    {
        // Enyo DatePicker: label "date".
        LabelProperty.OverrideDefaultValue<EnyoDatePicker>("date");
        ValueProperty.Changed.AddClassHandler<EnyoDatePicker>((p, e) => p.OnValueChanged(e));
        HideDayProperty.Changed.AddClassHandler<EnyoDatePicker>((p, _) => p.BuildFields());
        HideMonthProperty.Changed.AddClassHandler<EnyoDatePicker>((p, _) => p.BuildFields());
        HideYearProperty.Changed.AddClassHandler<EnyoDatePicker>((p, _) => p.BuildFields());
        MinYearProperty.Changed.AddClassHandler<EnyoDatePicker>((p, _) => p.UpdateRanges());
        MaxYearProperty.Changed.AddClassHandler<EnyoDatePicker>((p, _) => p.UpdateRanges());
    }

    /// <summary>Initializes a new instance of the <see cref="EnyoDatePicker"/> class.</summary>
    public EnyoDatePicker()
    {
        foreach (var name in CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthGenitiveNames)
        {
            if (!string.IsNullOrEmpty(name))
            {
                _month.Items.Add(name);
            }
        }

        _month.ValueChanged += (_, _) => OnFieldChanged();
        _day.ValueChanged += (_, _) => OnFieldChanged();
        _year.ValueChanged += (_, _) => OnFieldChanged();
        UpdateRanges();
        BuildFields();
    }

    /// <summary>The date. Only the date part is used.</summary>
    public DateTime Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>True to hide the day picker.</summary>
    public bool HideDay
    {
        get => GetValue(HideDayProperty);
        set => SetValue(HideDayProperty, value);
    }

    /// <summary>True to hide the month picker.</summary>
    public bool HideMonth
    {
        get => GetValue(HideMonthProperty);
        set => SetValue(HideMonthProperty, value);
    }

    /// <summary>True to hide the year picker.</summary>
    public bool HideYear
    {
        get => GetValue(HideYearProperty);
        set => SetValue(HideYearProperty, value);
    }

    /// <summary>The first year in the year list. Default 1900.</summary>
    public int MinYear
    {
        get => GetValue(MinYearProperty);
        set => SetValue(MinYearProperty, value);
    }

    /// <summary>The last year in the year list. Default 2099.</summary>
    public int MaxYear
    {
        get => GetValue(MaxYearProperty);
        set => SetValue(MaxYearProperty, value);
    }

    /// <summary>The month, day and year pickers in display order for <paramref name="culture"/>.</summary>
    internal static IReadOnlyList<char> FieldOrder(CultureInfo culture)
    {
        var pattern = culture.DateTimeFormat.ShortDatePattern;
        var order = new List<(int Index, char Field)>();
        foreach (var field in new[] { 'M', 'd', 'y' })
        {
            var i = pattern.IndexOf(field, StringComparison.Ordinal);
            order.Add((i < 0 ? int.MaxValue : i, field));
        }

        order.Sort((a, b) => a.Index.CompareTo(b.Index));
        return order.ConvertAll(o => o.Field);
    }

    private void BuildFields()
    {
        Items.Clear();
        foreach (var field in FieldOrder(CultureInfo.CurrentCulture))
        {
            switch (field)
            {
                case 'M' when !HideMonth:
                    Items.Add(_month);
                    break;
                case 'd' when !HideDay:
                    Items.Add(_day);
                    break;
                case 'y' when !HideYear:
                    Items.Add(_year);
                    break;
            }
        }

        PushValue(Value);
    }

    private void UpdateRanges()
    {
        _year.Minimum = MinYear;
        _year.Maximum = Math.Max(MinYear, MaxYear);
        PushValue(Value);
    }

    private void OnValueChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_syncing)
        {
            return;
        }

        PushValue(e.GetNewValue<DateTime>());
        RaiseValueChanged(e.OldValue, e.NewValue);
    }

    private void PushValue(DateTime value)
    {
        _syncing = true;
        try
        {
            _year.Value = Math.Clamp(value.Year, _year.Minimum, _year.Maximum);
            _month.SelectedIndex = value.Month - 1;
            _day.Maximum = DateTime.DaysInMonth(value.Year, value.Month);
            _day.Value = value.Day;
        }
        finally
        {
            _syncing = false;
        }
    }

    private void OnFieldChanged()
    {
        if (_syncing)
        {
            return;
        }

        var year = _year.Value is int y ? y : Value.Year;
        var month = _month.SelectedIndex >= 0 ? _month.SelectedIndex + 1 : Value.Month;
        var days = DateTime.DaysInMonth(year, month);
        var day = Math.Min(_day.Value is int d ? d : Value.Day, days);
        var old = Value;
        var date = new DateTime(year, month, day);
        _syncing = true;
        try
        {
            _day.Maximum = days;
            _day.Value = day;
            SetCurrentValue(ValueProperty, date);
        }
        finally
        {
            _syncing = false;
        }

        if (old != date)
        {
            RaiseValueChanged(old, date);
        }
    }
}
