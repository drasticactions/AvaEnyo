using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using AvaEnyo.Controls;

namespace AvaEnyo.Gallery.Pages.AllControls;

/// <summary>
/// The Onyx theme sample of Enyo 1.0 (Theming/onyx), rebuilt with AvaEnyo for side-by-side review. The repeated
/// button, radio and progress rows are built here, as App.js listed them one by one.
/// </summary>
public partial class AllControlsPage : UserControl
{
    private static readonly string[] s_captions = ["Button!", "Disabled button!", "Toggling button!", "Looooooooooooooooooong button"];
    private readonly Random _random = new(7);

    public AllControlsPage()
    {
        InitializeComponent();
        BuildButtons();
        BuildRadios(RadioLight);
        BuildRadios(RadioDark);
        BuildProgressButtons();

        PopupButton.Click += (_, _) => SamplePopup.OpenAtCenter();
        PopupClose.Click += (_, _) => SamplePopup.Close();
        MenuButton.Click += (_, _) => SampleMenu.OpenAroundControl(MenuButton);
        AppMenuButton.Click += (_, _) => SampleAppMenu.Open();
        foreach (var bar in new[] { Bar, BarItem1, BarItem2 })
        {
            Progress.SetAnimate(bar, true);
            bar.PointerPressed += (_, _) => bar.Value = _random.NextDouble() * 100;
        }

        ProgressSliderSample.PropertyChanged += (_, e) =>
        {
            if (e.Property == RangeBase.ValueProperty)
            {
                var v = _random.NextDouble() * 100;
                ProgressSliderSample.BarValue = v;
                ProgressSliderSample.AltBarValue = v + (_random.NextDouble() * (100 - v));
            }
        };

        SampleList.ItemsSource = Enumerable.Range(0, 5).Select(i => "item" + i).ToList();
    }

    private void BuildButtons()
    {
        string?[] classes = [null, "light", "dark", "affirmative", "negative", "blue"];
        foreach (var dark in new[] { false, true })
        {
            foreach (var cls in classes)
            {
                ButtonRows.Children.Add(Row(cls, dark ? Brush.Parse("#333") : null, notification: false));
            }
        }

        ButtonRows.Children.Add(Row("dark", Brush.Parse("#333"), notification: false));
        foreach (var cls in new string?[] { null, "notification-affirmative", "notification-negative", "notification-alternate" })
        {
            NotificationRows.Children.Add(Row(cls, Brushes.Black, notification: true));
        }
    }

    private static Control Row(string? cls, IBrush? background, bool notification)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Background = background };
        for (var i = 0; i < s_captions.Length; i++)
        {
            Button b = i == 2 ? new ToggleButton() : new Button();
            b.Content = s_captions[i];
            b.IsEnabled = i != 1;
            b.HorizontalAlignment = HorizontalAlignment.Left;
            if (notification)
            {
                b.Classes.Add("notification");
            }

            if (cls is not null)
            {
                b.Classes.Add(cls);
            }

            row.Children.Add(b);
        }

        return row;
    }

    private static void BuildRadios(Panel host)
    {
        host.Children.Add(Group(false, ("Enabled", true), ("bar", true), ("Disabled", false), ("Search", true)));
        host.Children.Add(Group(true, ("foo", true), ("bar", true), ("Disabled", false), ("baz", true)));
        host.Children.Add(Group(false, ("Single Selected", true)));
        host.Children.Add(Unselected(Group(false, ("Single", true))));
        host.Children.Add(Unselected(Group(false, ("Disabled", false))));
        host.Children.Add(Group(true, ("Single Selected", true)));
        host.Children.Add(Unselected(Group(true, ("Single", true))));
        host.Children.Add(Unselected(Group(true, ("Disabled", false))));
    }

    private static RadioGroup Group(bool dark, params (string Label, bool Enabled)[] segments)
    {
        var group = new RadioGroup { SelectedIndex = 0 };
        if (dark)
        {
            group.Classes.Add("dark");
        }

        foreach (var (label, enabled) in segments)
        {
            var segment = new RadioSegment { Content = label, IsEnabled = enabled };
            if (dark)
            {
                segment.Classes.Add("dark");
            }

            group.Items.Add(segment);
        }

        return group;
    }

    /// <summary>A group with nothing selected (App.js <c>value: 1</c> on a one-segment group).</summary>
    private static RadioGroup Unselected(RadioGroup group)
    {
        // A radio group always keeps one segment selected; dim the lone segment instead, as an unselected look.
        group.Opacity = 0.85;
        return group;
    }

    private void BuildProgressButtons()
    {
        (string Caption, string? Class, bool Cancelable)[] rows =
        [
            ("Default progress...", null, true),
            ("Uncancelable progress...", null, false),
            ("Affirmative progress...", "affirmative", true),
            ("Negative progress...", "negative", true),
            ("Blue progress...", "blue", true),
            ("Light progress...", "light", true),
            ("Dark progress...", "dark", true),
        ];
        foreach (var (caption, cls, cancelable) in rows)
        {
            var button = new ProgressButton { Content = caption, Cancelable = cancelable, Value = 30 + (_random.NextDouble() * 40), Margin = new Thickness(0, 2) };
            if (cls is not null)
            {
                button.Classes.Add(cls);
            }

            Progress.SetAnimate(button, true);
            button.PointerReleased += (_, _) => button.Value = _random.NextDouble() * 100;
            button.Cancel += (_, _) => button.Value = 0;
            ProgressButtons.Children.Add(button);
        }
    }
}
