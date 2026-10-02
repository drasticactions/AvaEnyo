using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AvaEnyo.Animations;

namespace AvaEnyo;

/// <summary>
/// Slider input as in Enyo: a press keeps a scrolling ancestor from taking the drag, and a tap on the track animates
/// the thumb to the tapped point in 350 ms with a cubic ease-out.
/// </summary>
internal static class SliderGestures
{
    public static void Register()
    {
        InputElement.PointerPressedEvent.AddClassHandler<Slider>(OnPointerPressed, RoutingStrategies.Tunnel);
    }

    private static void OnPointerPressed(Slider slider, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(slider).Properties.IsLeftButtonPressed)
        {
            return;
        }

        e.PreventGestureRecognition();
        if (e.Source is Visual source && source.FindAncestorOfType<Thumb>(includeSelf: true) is not null)
        {
            return;
        }

        if (slider.GetVisualDescendants() is { } descendants && FindTrack(slider) is { } track && track.Thumb is { } thumb)
        {
            var p = e.GetPosition(track);
            double ratio;
            if (track.Orientation == Avalonia.Layout.Orientation.Horizontal)
            {
                var usable = Math.Max(1, track.Bounds.Width - thumb.Bounds.Width);
                ratio = (p.X - (thumb.Bounds.Width / 2)) / usable;
                if (track.IsDirectionReversed)
                {
                    ratio = 1 - ratio;
                }
            }
            else
            {
                var usable = Math.Max(1, track.Bounds.Height - thumb.Bounds.Height);
                ratio = 1 - ((p.Y - (thumb.Bounds.Height / 2)) / usable);
                if (track.IsDirectionReversed)
                {
                    ratio = 1 - ratio;
                }
            }

            ratio = Math.Clamp(ratio, 0, 1);
            var value = slider.Minimum + (ratio * (slider.Maximum - slider.Minimum));
            if (slider.IsSnapToTickEnabled && slider.TickFrequency > 0)
            {
                value = slider.Minimum + (Math.Round((value - slider.Minimum) / slider.TickFrequency) * slider.TickFrequency);
            }

            _ = EnyoAnimations.ProgressTo(slider, value, EnyoDurations.SliderTap);
            e.Handled = true;
        }
    }

    private static Track? FindTrack(Slider slider)
    {
        foreach (var d in slider.GetVisualDescendants())
        {
            if (d is Track t)
            {
                return t;
            }
        }

        return null;
    }
}
