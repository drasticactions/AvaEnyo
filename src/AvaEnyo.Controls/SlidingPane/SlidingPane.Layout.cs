using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using AvaEnyo.Controls.Primitives;

namespace AvaEnyo.Controls;

public partial class SlidingPane
{
    private bool _sizesPending;
    private bool _positionsValid;

    /// <summary>
    /// Lays the views out side by side at their natural widths (explicit <c>Width</c>, a flex share, or their
    /// desired width) and records each view's natural left edge. Called by <see cref="SlidingPanePanel"/>.
    /// </summary>
    internal void ComputeNaturalLayout(Size size)
    {
        var widthChanged = size.Width != PaneWidth;
        PaneWidth = size.Width;
        UpdateNarrow(size.Width);
        var visible = new List<SlidingView>();
        foreach (var v in Views)
        {
            if (v.IsVisible)
            {
                visible.Add(v);
            }
        }

        double fixedTotal = 0, flexTotal = 0;
        foreach (var v in visible)
        {
            if (IsNarrow)
            {
                v.NaturalWidth = size.Width;
                fixedTotal += size.Width;
                continue;
            }

            var flex = Flex.GetValue(v);
            if (!double.IsNaN(v.DeclaredWidth))
            {
                v.NaturalWidth = v.DeclaredWidth;
                fixedTotal += v.DeclaredWidth;
            }
            else if (flex > 0)
            {
                flexTotal += flex;
            }
            else
            {
                v.Measure(new Size(double.PositiveInfinity, size.Height));
                v.NaturalWidth = v.DesiredSize.Width;
                fixedTotal += v.NaturalWidth;
            }
        }

        var leftover = Math.Max(0, size.Width - fixedTotal);
        double left = 0;
        foreach (var v in visible)
        {
            if (!IsNarrow && double.IsNaN(v.DeclaredWidth) && Flex.GetValue(v) is var flex && flex > 0)
            {
                v.NaturalWidth = leftover * flex / flexTotal;
            }

            // The panel measured this view at its previous width; content such as wrapped text must measure again.
            if (v.MeasuredWidth != v.NaturalWidth)
            {
                v.MeasuredWidth = v.NaturalWidth;
                v.InvalidateMeasure();
                _panel?.InvalidateMeasure();
            }

            v.NaturalLeft = left;
            left += v.NaturalWidth;
        }

        if (!_positionsValid || widthChanged)
        {
            _positionsValid = true;

            // Positions depend on the natural layout. Revalidate after this arrange, not in the middle of it. A slide
            // that is running aims at a target computed for the old layout, so it stops first.
            var resized = widthChanged;
            Dispatcher.UIThread.Post(() =>
            {
                if (resized && !IsDragging)
                {
                    StopAnimation();
                }

                ValidateViews(immediateSizes: true);
            }, DispatcherPriority.Render);
        }
    }

    /// <summary>The width a view is arranged at: its fitted width when the pane fitted it.</summary>
    internal static double ArrangedWidth(SlidingView view) => view.FitWidth ?? view.NaturalWidth;

    /// <summary>Recomputes positions and sizes on the next layout.</summary>
    internal void InvalidateViews()
    {
        _positionsValid = false;
        _panel?.InvalidateArrange();
        InvalidateArrange();
    }

    /// <summary>Puts every view where the model says, then fits the last showing view (Enyo <c>validateViews</c>).</summary>
    internal void ValidateViews(bool immediateSizes = false)
    {
        ValidateViewPositions();
        ResetOverSliding();
        if (immediateSizes)
        {
            ValidateViewSizes();
        }
        else if (!_sizesPending)
        {
            // Enyo debounced the fit by 10 ms; the next dispatcher pass does the same job.
            _sizesPending = true;
            Dispatcher.UIThread.Post(() =>
            {
                _sizesPending = false;
                ValidateViewSizes();
            }, DispatcherPriority.Background);
        }
    }

    private void ValidateViewPositions()
    {
        var first = _selected?.First ?? _selected;
        first?.ValidateSlide();
    }

    internal void ValidateViewSizes() => ValidateViewSizes(sliding: false);

    /// <summary>Fits the last showing view and, while sliding, the view it uncovers up to the moving view's edge.</summary>
    private void ValidateViewSizes(bool sliding)
    {
        var last = _selected?.LastShowingSibling;
        SlidingView? uncovered = null;
        double? edge = null;
        if (sliding && last is not null)
        {
            if (last.Next is { IsVisible: true, IsShowing: false } leaving)
            {
                uncovered = last;
                edge = leaving.CurrentLeft;
            }
            else if (last.Previous is { } previous && last.CurrentLeft > previous.CurrentLeft + previous.NaturalWidth)
            {
                uncovered = previous;
                edge = last.CurrentLeft;
            }
        }

        foreach (var v in Views)
        {
            v.ApplySize(v == last || v == uncovered, v == uncovered ? edge : null);
        }
    }

    /// <summary>Refits for a drag move or an animation frame.</summary>
    private void FitForSlide()
    {
        if (FitWhileSliding && !IsNarrow)
        {
            ValidateViewSizes(sliding: true);
        }
    }

    private void ResetOverSliding()
    {
        foreach (var v in Views)
        {
            v.OverSliding = false;
        }
    }

    /// <summary>
    /// Narrow mode makes a view full width, as Enyo's single-view layout set width 100% after caching the view's own
    /// width. The cached width comes back when the pane widens.
    /// </summary>
    internal static void ApplyNarrow(SlidingView view, bool narrow)
    {
        if (narrow)
        {
            if (view.CachedWidth is null)
            {
                view.CachedWidth = view.Width;
                view.SetCurrentValue(WidthProperty, double.NaN);
            }
        }
        else if (view.CachedWidth is { } w)
        {
            view.CachedWidth = null;
            view.SetCurrentValue(WidthProperty, w);
        }
    }

    private void UpdateNarrow(double width)
    {
        var narrow = !MultiView || (width > 0 && width <= MultiViewMinWidth);
        if (narrow == IsNarrow)
        {
            return;
        }

        IsNarrow = narrow;
        PseudoClasses.Set(":narrow", narrow);
        foreach (var v in Views)
        {
            v.FitWidth = null;
            ApplyNarrow(v, narrow);
        }

        _positionsValid = false;
    }
}
