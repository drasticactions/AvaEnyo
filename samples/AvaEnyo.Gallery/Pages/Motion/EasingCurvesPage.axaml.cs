using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using AvaEnyo.Animations;
using AvaEnyo.Controls;

namespace AvaEnyo.Gallery.Pages.Motion;

public partial class EasingCurvesPage : UserControl
{
    public EasingCurvesPage()
    {
        InitializeComponent();
        foreach (var curve in new[] { EnyoEasing.Linear, EnyoEasing.CubicIn, EnyoEasing.CubicOut, EnyoEasing.ExpoOut, EnyoEasing.QuadInOut })
        {
            Curves.Children.Add(Plot(curve));
        }
    }

    private static Control Plot(EnyoEasingFunction curve)
    {
        const double w = 180, h = 140;
        var points = new List<Point>();
        for (var i = 0; i <= 60; i++)
        {
            var t = i / 60.0;
            points.Add(new Point(t * w, h - (curve.Ease(t) * h)));
        }

        var line = new Polyline { Points = points, StrokeThickness = 2.5 };
        line.Bind(Shape.StrokeProperty, line.GetResourceObservable("EnyoLabelBrush"));
        var frame = new Border
        {
            Width = w,
            Height = h,
            BorderThickness = new Thickness(1, 0, 0, 1),
            BorderBrush = new SolidColorBrush(Color.FromArgb(0x60, 0x80, 0x80, 0x80)),
            Child = new Canvas { Children = { line } },
        };
        return new StackPanel
        {
            Margin = new Thickness(0, 0, 24, 24),
            Children = { new Divider { Caption = curve.Name }, frame },
        };
    }
}
