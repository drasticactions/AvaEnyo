using System;
using System.Collections.Generic;

namespace AvaEnyo.Animations;

/// <summary>
/// Estimates pointer velocity with a least-squares line over the samples of the last 100 ms.
/// </summary>
public sealed class VelocityTracker
{
    /// <summary>The time window of samples used: 100 ms.</summary>
    public static double WindowMilliseconds = 100;

    /// <summary>
    /// The shortest sample span that yields a velocity: 5 ms. Samples closer together than any real pointer report
    /// (synthesized or coalesced input) would give a meaningless, huge speed, so they count as no motion.
    /// </summary>
    public static double MinimumSpanMilliseconds = 5;

    private readonly List<(double Time, double X, double Y)> _samples = new();

    /// <summary>Forgets every sample.</summary>
    public void Clear() => _samples.Clear();

    /// <summary>Adds a sample at <paramref name="timeMilliseconds"/> (any monotonic clock).</summary>
    public void Add(double timeMilliseconds, double x, double y)
    {
        _samples.Add((timeMilliseconds, x, y));
        var cutoff = timeMilliseconds - WindowMilliseconds;
        var drop = 0;
        while (drop < _samples.Count - 2 && _samples[drop].Time < cutoff)
        {
            drop++;
        }

        if (drop > 0)
        {
            _samples.RemoveRange(0, drop);
        }
    }

    /// <summary>The number of samples in the window.</summary>
    public int Count => _samples.Count;

    /// <summary>The velocity in px/ms on each axis. Zero with fewer than two samples.</summary>
    public (double X, double Y) GetVelocity()
    {
        if (_samples.Count < 2)
        {
            return (0, 0);
        }

        var t0 = _samples[^1].Time;
        if (t0 - _samples[0].Time < MinimumSpanMilliseconds)
        {
            return (0, 0);
        }

        double st = 0, sx = 0, sy = 0, stt = 0, stx = 0, sty = 0;
        var n = 0;
        foreach (var s in _samples)
        {
            if (t0 - s.Time > WindowMilliseconds)
            {
                continue;
            }

            var t = s.Time - t0;
            st += t;
            sx += s.X;
            sy += s.Y;
            stt += t * t;
            stx += t * s.X;
            sty += t * s.Y;
            n++;
        }

        var d = (n * stt) - (st * st);
        if (n < 2 || Math.Abs(d) < 1e-9)
        {
            return (0, 0);
        }

        return (((n * stx) - (st * sx)) / d, ((n * sty) - (st * sy)) / d);
    }
}
