using System;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AvaEnyo.Animations;

/// <summary>Stops a <see cref="EnyoAnimations.Spin"/> rotation and removes its transform.</summary>
internal sealed class SpinHandle(Control control, ITransform rotate, CancellationTokenSource cancellation) : IDisposable
{
    private bool _disposed;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        cancellation.Cancel();
        cancellation.Dispose();
        if (ReferenceEquals(control.RenderTransform, rotate))
        {
            control.ClearValue(Visual.RenderTransformProperty);
            control.ClearValue(Visual.RenderTransformOriginProperty);
        }
    }
}
