using System;
using System.Globalization;
using Avalonia;

namespace AvaEnyo.Controls;

/// <summary>
/// The Enyo time picker (<c>enyo.TimePicker</c>): a <see cref="PickerGroup"/> with an hour picker, a minute picker
/// in steps of <see cref="MinuteInterval"/>, and an AM/PM picker on a 12-hour clock. The AM/PM names and their place
/// come from <see cref="CultureInfo.CurrentCulture"/>. Named with the prefix so it does not clash with Avalonia's
/// <c>TimePicker</c>.
/// </summary>
public class EnyoTimePicker : PickerGroup
{
    /// <summary>Defines the <see cref="Value"/> property.</summary>
    public static readonly StyledProperty<TimeSpan> ValueProperty =
        AvaloniaProperty.Register<EnyoTimePicker, TimeSpan>(nameof(Value), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Defines the <see cref="MinuteInterval"/> property.</summary>
    public static readonly StyledProperty<int> MinuteIntervalProperty =
        AvaloniaProperty.Register<EnyoTimePicker, int>(nameof(MinuteInterval), 1, validate: v => v >= 1 && v <= 30);

    /// <summary>Defines the <see cref="Is24Hour"/> property.</summary>
    public static readonly StyledProperty<bool> Is24HourProperty =
        AvaloniaProperty.Register<EnyoTimePicker, bool>(nameof(Is24Hour), CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.Contains('H'));

    private readonly Picker _hour = new() { Name = "PART_Hour" };
    private readonly Picker _minute = new() { Name = "PART_Minute" };
    private readonly Picker _period = new() { Name = "PART_Period" };
    private bool _syncing;

    static EnyoTimePicker()
    {
        // Enyo TimePicker: label "time".
        LabelProperty.OverrideDefaultValue<EnyoTimePicker>("time");
        ValueProperty.Changed.AddClassHandler<EnyoTimePicker>((p, e) => p.OnValueChanged(e));
        MinuteIntervalProperty.Changed.AddClassHandler<EnyoTimePicker>((p, _) => p.BuildFields());
        Is24HourProperty.Changed.AddClassHandler<EnyoTimePicker>((p, _) => p.BuildFields());
    }

    /// <summary>Initializes a new instance of the <see cref="EnyoTimePicker"/> class.</summary>
    public EnyoTimePicker()
    {
        var format = CultureInfo.CurrentCulture.DateTimeFormat;
        _period.Items.Add(string.IsNullOrEmpty(format.AMDesignator) ? "AM" : format.AMDesignator);
        _period.Items.Add(string.IsNullOrEmpty(format.PMDesignator) ? "PM" : format.PMDesignator);
        _hour.ValueChanged += (_, _) => OnFieldChanged();
        _minute.ValueChanged += (_, _) => OnFieldChanged();
        _period.ValueChanged += (_, _) => OnFieldChanged();
        BuildFields();
    }

    /// <summary>The time of day.</summary>
    public TimeSpan Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>The step of the minute list, 1 to 30. Default 1.</summary>
    public int MinuteInterval
    {
        get => GetValue(MinuteIntervalProperty);
        set => SetValue(MinuteIntervalProperty, value);
    }

    /// <summary>True for a 24-hour clock without AM/PM. Defaults to the current culture's clock.</summary>
    public bool Is24Hour
    {
        get => GetValue(Is24HourProperty);
        set => SetValue(Is24HourProperty, value);
    }

    /// <summary>The hour picker.</summary>
    internal Picker HourPicker => _hour;

    /// <summary>The minute picker.</summary>
    internal Picker MinutePicker => _minute;

    /// <summary>The AM/PM picker.</summary>
    internal Picker PeriodPicker => _period;

    private void BuildFields()
    {
        _syncing = true;
        try
        {
            _hour.Items.Clear();
            if (Is24Hour)
            {
                for (var h = 0; h < 24; h++)
                {
                    _hour.Items.Add(h.ToString("00", CultureInfo.CurrentCulture));
                }
            }
            else
            {
                for (var h = 1; h <= 12; h++)
                {
                    _hour.Items.Add(h.ToString(CultureInfo.CurrentCulture));
                }
            }

            _minute.Items.Clear();
            for (var m = 0; m < 60; m += MinuteInterval)
            {
                _minute.Items.Add(m.ToString("00", CultureInfo.CurrentCulture));
            }
        }
        finally
        {
            _syncing = false;
        }

        Items.Clear();
        var periodFirst = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.TrimStart().StartsWith('t');
        if (!Is24Hour && periodFirst)
        {
            Items.Add(_period);
        }

        Items.Add(_hour);
        Items.Add(_minute);
        if (!Is24Hour && !periodFirst)
        {
            Items.Add(_period);
        }

        PushValue(Value);
    }

    private void OnValueChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_syncing)
        {
            return;
        }

        PushValue(e.GetNewValue<TimeSpan>());
        RaiseValueChanged(e.OldValue, e.NewValue);
    }

    private void PushValue(TimeSpan value)
    {
        _syncing = true;
        try
        {
            var hours = value.Hours;
            if (Is24Hour)
            {
                _hour.SelectedIndex = hours;
            }
            else
            {
                var h12 = hours % 12 == 0 ? 12 : hours % 12;
                _hour.SelectedIndex = h12 - 1;
                _period.SelectedIndex = hours < 12 ? 0 : 1;
            }

            _minute.SelectedIndex = Math.Min(_minute.ItemCount - 1, value.Minutes / MinuteInterval);
        }
        finally
        {
            _syncing = false;
        }
    }

    private void OnFieldChanged()
    {
        if (_syncing || _hour.SelectedIndex < 0 || _minute.SelectedIndex < 0)
        {
            return;
        }

        int hours;
        if (Is24Hour)
        {
            hours = _hour.SelectedIndex;
        }
        else
        {
            var h12 = _hour.SelectedIndex + 1;
            var pm = _period.SelectedIndex == 1;
            hours = (h12 % 12) + (pm ? 12 : 0);
        }

        var time = new TimeSpan(hours, _minute.SelectedIndex * MinuteInterval, 0);
        var old = Value;
        _syncing = true;
        try
        {
            SetCurrentValue(ValueProperty, time);
        }
        finally
        {
            _syncing = false;
        }

        if (old != time)
        {
            RaiseValueChanged(old, time);
        }
    }
}
