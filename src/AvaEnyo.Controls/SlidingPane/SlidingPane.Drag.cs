using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using AvaEnyo.Primitives;

namespace AvaEnyo.Controls;

public partial class SlidingPane
{
    private DragTracker? _drag;
    private SlidingView? _dragging;
    private SlidingView? _dragStartSliding;
    private double _dx0;

    /// <summary>True while a drag moves the views.</summary>
    public bool IsDragging => _dragging is not null;

    private void InitializeDrag()
    {
        _drag = new DragTracker(this) { CanStart = CanStartDrag };
        _drag.Started += (_, e) => OnDragStarted(e);
        _drag.Moved += (_, e) => OnDragMoved(e);
        _drag.Completed += (_, e) => OnDragFinished();
        _drag.Canceled += (_, e) => OnDragFinished();
    }

    private bool CanStartDrag(DragStartInfo info)
    {
        if (!info.IsHorizontal)
        {
            return false;
        }

        var view = (info.Source as Visual)?.FindAncestorOfType<SlidingView>(includeSelf: true);
        while (view is not null && view.Pane != this)
        {
            view = view.FindAncestorOfType<SlidingView>();
        }

        if (view is null || !view.IsDraggableFrom(info.Source))
        {
            return false;
        }

        _dragStartSliding = view;
        _pendingStart = FindDraggable(info.Delta.X);
        return _pendingStart is not null;
    }

    private SlidingView? _pendingStart;

    /// <summary>Enyo <c>findDraggable</c>: the first view that can move by <paramref name="dx"/>, else the start view overslides.</summary>
    private SlidingView? FindDraggable(double dx)
    {
        ResetOverSliding();
        var start = _dragStartSliding!;
        for (var i = 0; i <= start.Index && i < Views.Count; i++)
        {
            if (Views[i].CanDrag(dx))
            {
                return Views[i];
            }
        }

        start.OverSliding = start.SlidePosition + dx >= 0;
        return start;
    }

    private void OnDragStarted(DragTrackerEventArgs e)
    {
        if (_pendingStart is not { } d)
        {
            return;
        }

        StopAnimation();
        _dx0 = 0;
        BeginDragging(d, e.Delta.X, 0);
        PseudoClasses.Set(":dragging", true);
    }

    private void BeginDragging(SlidingView view, double dx, double startDx)
    {
        _dragging?.SetDragging(false);
        _dragging = view;
        view.SetDragging(true);
        view.BeginDrag(dx, startDx);
    }

    private void OnDragMoved(DragTrackerEventArgs e)
    {
        var s = _dragging;
        var dx = e.Delta.X;
        if (s is not null)
        {
            var (boundary, select) = s.Drag(dx);
            if (boundary)
            {
                if (select is not null)
                {
                    SelectView(select, immediate: true);
                }

                var step = dx - _dx0;
                if (step == 0)
                {
                    step = s.IsMovingToSelect ? -1 : 1;
                }

                // Hand the drag to the next view that can move, so one gesture pushes through the stack.
                if (FindDraggable(step) is { } next)
                {
                    BeginDragging(next, dx, dx);
                }
            }
        }

        FitForSlide();
        _dx0 = dx;
    }

    private void OnDragFinished()
    {
        if (_dragging is not { } dragging)
        {
            return;
        }

        var select = dragging.GetDragSelect();
        var start = _dragStartSliding!;
        dragging.SetDragging(false);
        _dragging = null;
        PseudoClasses.Set(":dragging", false);
        if (start.SlidePosition > DismissDistance && start.Dismissible)
        {
            start.IsShowing = false;
        }
        else if (select is not null)
        {
            SelectView(select);
        }
        else
        {
            AnimateOverSlide(start);
        }
    }
}
