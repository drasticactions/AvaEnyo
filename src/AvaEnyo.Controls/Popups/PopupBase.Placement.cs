using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using AvaEnyo.Controls.Primitives;

namespace AvaEnyo.Controls;

public partial class PopupBase
{
    private Func<Size, Rect, Visual, Point>? _placement;
    private Control? _anchor;
    private readonly List<ScrollViewer> _scrollers = new();

    /// <summary>Opens the popup with its top-left corner at <paramref name="point"/> (top-level coordinates).</summary>
    public void OpenAt(Point point)
    {
        _anchor = null;
        _placement = (_, _, _) => point;
        OpenCore();
    }

    /// <summary>Opens the popup at the pointer position of <paramref name="e"/>.</summary>
    public void OpenAtPointer(PointerEventArgs e)
    {
        var top = TopLevel.GetTopLevel(this);
        OpenAt(top is null ? default : e.GetPosition(top));
    }

    /// <summary>Opens the popup with its top-left corner at <paramref name="control"/>'s, moved by <paramref name="offset"/>.</summary>
    public void OpenAtControl(Control control, Vector offset = default)
    {
        _anchor = control;
        _placement = (_, _, host) => AnchorRect(control, host).TopLeft + offset;
        OpenCore();
    }

    /// <summary>Opens the popup next to <paramref name="control"/> on the side given by <paramref name="alignment"/>.</summary>
    public void OpenAroundControl(Control control, PopupAlignment alignment = PopupAlignment.Below)
    {
        _anchor = control;
        _placement = (size, viewport, host) =>
        {
            var r = AnchorRect(control, host);
            return alignment switch
            {
                PopupAlignment.Above => r.Top - size.Height >= viewport.Top || r.Bottom + size.Height > viewport.Bottom
                    ? new Point(r.Left, r.Top - size.Height)
                    : new Point(r.Left, r.Bottom),
                PopupAlignment.Center => new Point(r.Center.X - (size.Width / 2), r.Center.Y - (size.Height / 2)),
                PopupAlignment.Right => new Point(r.Right, r.Top),
                PopupAlignment.Left => new Point(r.Left - size.Width, r.Top),
                _ => r.Bottom + size.Height <= viewport.Bottom || r.Top - size.Height < viewport.Top
                    ? new Point(r.Left, r.Bottom)
                    : new Point(r.Left, r.Top - size.Height),
            };
        };
        OpenCore();
    }

    /// <summary>
    /// Opens the popup near <paramref name="control"/> (Enyo <c>openNear</c>): centered on it horizontally, below it,
    /// or above it when there is no room below.
    /// </summary>
    public void OpenNear(Control control)
    {
        _anchor = control;
        _placement = (size, viewport, host) =>
        {
            var r = AnchorRect(control, host);
            var x = r.Center.X - (size.Width / 2);
            var y = r.Bottom + size.Height <= viewport.Bottom ? r.Bottom : r.Top - size.Height;
            return new Point(x, y);
        };
        OpenCore();
    }

    /// <summary>Opens the popup centered in the window.</summary>
    public void OpenAtCenter()
    {
        _anchor = null;
        _placement = PlaceAtCenter;
        OpenCore();
    }

    /// <summary>Computes the top-left of the presenter for its size; the host clamps the result.</summary>
    internal Point ComputePosition(Size size, Rect viewport, Visual host) =>
        PlacePresenter(size, viewport) ?? (_placement ?? PlaceAtCenter)(size, viewport, host);

    /// <summary>
    /// Lets a popup type place its presenter itself, for example a toaster against a window edge. Return null to use
    /// the placement of the last <c>Open*</c> call.
    /// </summary>
    /// <param name="size">The measured size of the presenter.</param>
    /// <param name="viewport">The usable area of the overlay.</param>
    protected virtual Point? PlacePresenter(Size size, Rect viewport) => null;

    private static Point PlaceAtCenter(Size size, Rect viewport, Visual host) =>
        new(viewport.Center.X - (size.Width / 2), viewport.Center.Y - (size.Height / 2));

    private static Rect AnchorRect(Control control, Visual host)
    {
        if (control.TranslatePoint(default, host) is { } p)
        {
            return new Rect(p, control.Bounds.Size);
        }

        return default;
    }

    private void SubscribeAnchorScrollers()
    {
        UnsubscribeAnchorScrollers();
        if (_anchor is null)
        {
            return;
        }

        foreach (var viewer in _anchor.GetVisualAncestors())
        {
            if (viewer is ScrollViewer sv)
            {
                sv.ScrollChanged += OnAnchorScrolled;
                _scrollers.Add(sv);
            }
        }
    }

    private void UnsubscribeAnchorScrollers()
    {
        foreach (var sv in _scrollers)
        {
            sv.ScrollChanged -= OnAnchorScrolled;
        }

        _scrollers.Clear();
    }

    private void OnAnchorScrolled(object? sender, ScrollChangedEventArgs e) => _host?.InvalidateArrange();
}
