using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace AvaEnyo.Controls.Tests;

/// <summary>
/// Touch for the headless platform, whose raw touch input is not public. It routes pointer events of type Touch the
/// way the input manager does: to the captured element when there is one, else to the element under the point.
/// Timestamps advance 16 ms per event unless given.
/// </summary>
public sealed class TouchInput(TopLevel top, int id = 1, PointerType type = PointerType.Touch)
{
    private static ulong s_time = 1000;
    private readonly Pointer _pointer = new(id, type, true);

    public IPointer Pointer => _pointer;

    public ulong StepMs { get; set; } = 16;

    public void Down(Point point)
    {
        var target = Hit(point);
        _pointer.Capture(target as IInputElement);
        Raise(target, new PointerPressedEventArgs(target, _pointer, top, point, Tick(), Pressed(PointerUpdateKind.LeftButtonPressed), KeyModifiers.None));
    }

    public void Move(Point point)
    {
        var target = _pointer.Captured as Interactive ?? Hit(point);
        Raise(target, new PointerEventArgs(InputElement.PointerMovedEvent, target, _pointer, top, point, Tick(), Pressed(PointerUpdateKind.Other), KeyModifiers.None));
    }

    public void Up(Point point)
    {
        var target = _pointer.Captured as Interactive ?? Hit(point);
        Raise(target, new PointerReleasedEventArgs(target, _pointer, top, point, Tick(), new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased), KeyModifiers.None, MouseButton.Left));
        _pointer.Capture(null);
        Dispatcher.UIThread.RunJobs();
    }

    public void Drag(Point from, Vector by, int steps = 10)
    {
        Down(from);
        MoveBy(from, by, steps);
    }

    public void MoveBy(Point from, Vector by, int steps = 10)
    {
        for (var i = 1; i <= steps; i++)
        {
            Move(from + (by * i / steps));
        }
    }

    public void Tap(Point point)
    {
        Down(point);
        Up(point);
    }

    private static PointerPointProperties Pressed(PointerUpdateKind kind) => new(RawInputModifiers.LeftMouseButton, kind);

    private ulong Tick() => s_time += StepMs;

    private Interactive Hit(Point point)
    {
        for (var i = 0; i < 3; i++)
        {
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        }

        Dispatcher.UIThread.RunJobs();
        return top.InputHitTest(point) as Interactive ?? top;
    }

    private static void Raise(Interactive target, RoutedEventArgs args)
    {
        target.RaiseEvent(args);
        Dispatcher.UIThread.RunJobs();
    }
}
