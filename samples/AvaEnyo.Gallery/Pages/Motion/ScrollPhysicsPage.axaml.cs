using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaEnyo.Animations;
using AvaEnyo.Controls;
using AvaEnyo.Primitives;

namespace AvaEnyo.Gallery.Pages.Motion;

/// <summary>
/// The scroll constants as sliders. Each scroller reads the defaults when it is created, so the list is rebuilt
/// after a change.
/// </summary>
public partial class ScrollPhysicsPage : UserControl
{
    private ScrollViewer? _viewer;
    private readonly DispatcherTimer _timer;

    public ScrollPhysicsPage()
    {
        InitializeComponent();
        Load();
        foreach (var s in new[] { Friction, Spring, Drag, Snap })
        {
            s.PropertyChanged += (_, e) =>
            {
                if (e.Property == Slider.ValueProperty)
                {
                    Apply();
                }
            };
        }

        ResetButton.Click += (_, _) =>
        {
            ScrollPhysics.DefaultFriction = 0.97;
            ScrollPhysics.DefaultSpring = 0.93;
            ScrollPhysics.DefaultDragDamping = 0.5;
            ScrollPhysics.DefaultSnapFriction = 0.9;
            Load();
        };
        Rebuild();
        _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(50), DispatcherPriority.Background, (_, _) => Report());
        AttachedToVisualTree += (_, _) => _timer.Start();
        DetachedFromVisualTree += (_, _) => _timer.Stop();
    }

    private void Load()
    {
        Friction.Value = ScrollPhysics.DefaultFriction;
        Spring.Value = ScrollPhysics.DefaultSpring;
        Drag.Value = ScrollPhysics.DefaultDragDamping;
        Snap.Value = ScrollPhysics.DefaultSnapFriction;
        Apply();
    }

    private void Apply()
    {
        ScrollPhysics.DefaultFriction = Friction.Value;
        ScrollPhysics.DefaultSpring = Spring.Value;
        ScrollPhysics.DefaultDragDamping = Drag.Value;
        ScrollPhysics.DefaultSnapFriction = Snap.Value;
        FrictionText.Text = $"Friction per 10 ms: {Friction.Value:0.000}";
        SpringText.Text = $"Overscroll spring: {Spring.Value:0.000}";
        DragText.Text = $"Drag past the edge: {Drag.Value:0.00}";
        SnapText.Text = $"Snap friction: {Snap.Value:0.00}";
        if (ListHost is not null)
        {
            Rebuild();
        }
    }

    private void Rebuild()
    {
        var list = new StackPanel();
        for (var i = 0; i < 200; i++)
        {
            list.Children.Add(new Item { Content = $"Row {i + 1}: drag, fling and pull past the ends" });
        }

        _viewer = new ScrollViewer { Content = list };
        ListHost.Children.Clear();
        ListHost.Children.Add(_viewer);
    }

    private void Report()
    {
        if (_viewer?.GetVisualDescendants().OfType<EnyoScrollContentPresenter>().FirstOrDefault() is { } p)
        {
            Live.Text = $"Offset {_viewer.Offset.Y:0}  Overscroll {p.Overscroll.Y:0.0}  {(p.IsDragging ? "dragging" : p.IsScrolling ? "moving" : "at rest")}";
        }
    }
}
