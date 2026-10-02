using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace AvaEnyo.Animations;

/// <summary>
/// Calls back once per rendered frame with the real time since the previous frame. It uses
/// <see cref="TopLevel.RequestAnimationFrame"/> when the host is in a visual tree, otherwise a 16 ms dispatcher timer.
/// </summary>
internal sealed class FrameClock : IDisposable
{
    private readonly Visual? _host;
    private readonly Func<TimeSpan, bool> _onFrame;
    private readonly Stopwatch _watch = Stopwatch.StartNew();
    private TimeSpan _last;
    private DispatcherTimer? _timer;
    private bool _disposed;

    /// <summary>Starts the clock. <paramref name="onFrame"/> gets the elapsed time and returns false to stop.</summary>
    public FrameClock(Visual? host, Func<TimeSpan, bool> onFrame)
    {
        _host = host;
        _onFrame = onFrame;
        _last = _watch.Elapsed;
        Schedule();
    }

    /// <summary>True until the callback returns false or the clock is disposed.</summary>
    public bool IsRunning => !_disposed;

    private void Schedule()
    {
        if (_disposed)
        {
            return;
        }

        if (_host is not null && TopLevel.GetTopLevel(_host) is { } top)
        {
            top.RequestAnimationFrame(_ => Tick());
            return;
        }

        if (_timer is null)
        {
            _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Render, (_, _) => Tick());
            _timer.Start();
        }
    }

    private void Tick()
    {
        if (_disposed)
        {
            return;
        }

        var now = _watch.Elapsed;
        var elapsed = now - _last;
        _last = now;
        if (!_onFrame(elapsed))
        {
            Dispose();
            return;
        }

        if (_timer is null)
        {
            Schedule();
        }
    }

    /// <summary>Stops the clock.</summary>
    public void Dispose()
    {
        _disposed = true;
        _timer?.Stop();
        _timer = null;
    }
}
