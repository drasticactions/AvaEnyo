using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using AvaEnyo.Animations;

namespace AvaEnyo.Gallery.Pages.Motion;

public partial class DurationsPage : UserControl
{
    private readonly List<Action> _players = new();

    public DurationsPage()
    {
        InitializeComponent();
        Add("SlidingPane slide", EnyoDurations.SlidingPane, EnyoEasing.CubicOut);
        Add("Drawer open", EnyoDurations.DrawerOpen, EnyoEasing.CubicOut);
        Add("Drawer close", EnyoDurations.DrawerClose, EnyoEasing.CubicOut);
        Add("Toaster", EnyoDurations.Toaster, EnyoEasing.CubicOut);
        Add("Progress", EnyoDurations.Progress, EnyoEasing.CubicOut);
        Add("Slider tap", EnyoDurations.SliderTap, EnyoEasing.CubicOut);
        Add("Pane fade", EnyoDurations.PaneFade, EnyoEasing.CubicOut);
        Add("Pane fly-in", EnyoDurations.PaneFlyIn, EnyoEasing.CubicOut);
        Add("Modal dialog fade", EnyoDurations.ModalDialogFade, EnyoEasing.CssEase);
        Add("Scrim fade", EnyoDurations.ScrimFade, EnyoEasing.CssEase);
        Add("Toolbar fade", EnyoDurations.ToolbarFade, EnyoEasing.Linear);
        Add("Spinner turn", EnyoDurations.Spinner, EnyoEasing.Linear);
        Add("Smart zoom", EnyoDurations.SmartZoom, EnyoEasing.CubicOut);
        PlayAll.Click += (_, _) => _players.ForEach(p => p());
    }

    private void Add(string name, double ms, Easing easing)
    {
        var row = Table.RowDefinitions.Count;
        Table.RowDefinitions.Add(new RowDefinition(new GridLength(36)));
        var label = new TextBlock { Text = name, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Classes = { "enyo-text-body" } };
        var value = new TextBlock { Text = $"{ms:0} ms", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Classes = { "enyo-subtext" } };
        var track = new Border { Height = 12, CornerRadius = new CornerRadius(6), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
        track.Bind(Border.BackgroundProperty, track.GetResourceObservable("EnyoProgressTrackBrush"));
        var dot = new Border { Width = 24, Height = 24, CornerRadius = new CornerRadius(12), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, RenderTransform = new TranslateTransform() };
        dot.Bind(Border.BackgroundProperty, dot.GetResourceObservable("EnyoHandleDownBrush"));
        var lane = new Panel { Children = { track, dot }, Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand) };
        void Play()
        {
            var t = (TranslateTransform)dot.RenderTransform!;
            var end = Math.Max(0, lane.Bounds.Width - dot.Width);
            var from = t.X > end / 2 ? end : 0;
            _ = EnyoAnimations.Animate(from, from > 0 ? 0 : end, TimeSpan.FromMilliseconds(ms), easing, v => t.X = v, lane);
        }

        lane.PointerPressed += (_, _) => Play();
        _players.Add(Play);
        Grid.SetRow(label, row);
        Grid.SetRow(value, row);
        Grid.SetColumn(value, 1);
        Grid.SetRow(lane, row);
        Grid.SetColumn(lane, 2);
        Table.Children.Add(label);
        Table.Children.Add(value);
        Table.Children.Add(lane);
    }
}
