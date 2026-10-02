using System;
using Avalonia.Controls;
using AvaEnyo.Animations;

namespace AvaEnyo.Controls;

public partial class SlidingPane
{
    private readonly EnyoAnimator _animator = new() { Duration = TimeSpan.FromMilliseconds(EnyoDurations.SlidingPane), Easing = EnyoEasing.CubicOut };
    private SlidingView? _animating;
    private bool _immediate;
    private int _animationGeneration;

    /// <summary>True while a slide animation runs.</summary>
    public bool IsSlideAnimating => _animator.IsAnimating;

    private void AnimateSelected()
    {
        if (_selected is null)
        {
            return;
        }

        var s = FindAnimateable(_selected);
        if (s is not null)
        {
            if (CanAnimate && !_immediate && EnyoAnimations.IsActive)
            {
                PlayAnimation(s);
            }
            else
            {
                ValidateViews(immediateSizes: true);
                OnSlideComplete();
            }
        }
        else
        {
            OnSlideComplete();
        }
    }

    private void AnimateOverSlide(SlidingView overSlide)
    {
        if (CanAnimate && EnyoAnimations.IsActive)
        {
            overSlide.ValidateSlideBefore();
            PlayAnimation(overSlide);
        }
        else
        {
            ValidateViews(immediateSizes: true);
            OnSlideComplete();
        }
    }

    private SlidingView? FindAnimateable(SlidingView sliding)
    {
        if (sliding.CanAnimate())
        {
            return sliding;
        }

        var n = _selected?.LastShowingSibling;
        return n is not null && n.CanAnimate() ? n : null;
    }

    private async void PlayAnimation(SlidingView sliding)
    {
        _animator.Host = this;
        _animator.Duration = TimeSpan.FromMilliseconds(EnyoDurations.SlidingPane);
        _animating = sliding;
        var generation = ++_animationGeneration;
        PseudoClasses.Set(":animating", true);
        var from = sliding.SlidePosition;
        var to = sliding.CalcSlide();
        _animator.Animating -= OnAnimationStep;
        _animator.Animating += OnAnimationStep;
        var done = await _animator.Play(from, to);
        if (generation != _animationGeneration)
        {
            return;
        }

        PseudoClasses.Set(":animating", false);
        _animating = null;

        // Enyo's animator fired onStop both when it ended and when it was stopped; either way the slide is complete.
        OnSlideComplete();
        _ = done;
    }

    private void OnAnimationStep(double value, double progress)
    {
        if (_animating is { } s)
        {
            s.AnimateMove(Math.Round(value), s.OverSliding);
            FitForSlide();
        }
    }

    /// <summary>Stops a running slide where it is.</summary>
    internal void StopAnimation()
    {
        if (_animator.IsAnimating)
        {
            _animationGeneration++;
            _animator.Stop();
            PseudoClasses.Set(":animating", false);
            _animating = null;
        }
    }

    private void OnSlideComplete()
    {
        ValidateViewSizes();
        ResetOverSliding();
        RaiseEvent(new SlideCompleteEventArgs(SlideCompleteEvent, this, _selected));
    }

    /// <summary>Enyo <c>showingChanged</c>: a view slides in or out when it is shown or hidden.</summary>
    internal void OnViewShowingChanged(SlidingView view, bool showing)
    {
        if (IsDragging)
        {
            return;
        }

        StopAnimation();
        if (showing)
        {
            view.IsVisible = true;
            InvalidateViews();
            UpdateLayout();
            view.SetSlideImmediate(view.CalcSlideHidden());
            ValidateViewSizes(sliding: FitWhileSliding && !IsNarrow);
        }
        else if (view == _selected && view.Previous is { } previous)
        {
            // AvaEnyo selects the view on the left when the selected view is hidden, so the pane keeps a visible
            // selection; Enyo left the hidden view selected.
            var last = _selected;
            _selected = previous;
            UpdateSelectedPseudoClasses();
            RaisePropertyChanged(SelectedViewProperty, last, previous);
            RaiseEvent(new PaneSelectionChangedEventArgs(SelectionChangedEvent, this, previous, last));
        }

        view.OverSliding = true;
        if (CanAnimate && EnyoAnimations.IsActive)
        {
            HideWhenDone(view, showing);
            PlayAnimation(view);
        }
        else
        {
            ValidateViews(immediateSizes: true);
            if (!showing)
            {
                view.IsVisible = false;
            }

            OnSlideComplete();
        }
    }

    private void HideWhenDone(SlidingView view, bool showing)
    {
        if (showing)
        {
            return;
        }

        void Handler(object? sender, SlideCompleteEventArgs e)
        {
            SlideComplete -= Handler;
            if (!view.IsShowing)
            {
                view.IsVisible = false;
            }
        }

        SlideComplete += Handler;
    }
}
