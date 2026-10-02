using System;
using Avalonia.Controls;
using AvaEnyo.Controls.Primitives;

namespace AvaEnyo.Gallery.Pages.Motion;

public partial class GesturesPage : UserControl
{
    public GesturesPage()
    {
        InitializeComponent();
        EnyoGestures.AddHoldHandler(Target, (_, e) => Write($"Hold after {e.Elapsed.TotalMilliseconds:0} ms ({e.PointerType})"));
        EnyoGestures.AddHoldPulseHandler(Target, (_, e) => Write($"HoldPulse at {e.Elapsed.TotalMilliseconds:0} ms"));
        EnyoGestures.AddDragStartedHandler(Target, (_, e) => Write($"DragStarted dx {e.Delta.X:0} dy {e.Delta.Y:0} {(e.IsHorizontal ? "horizontal" : "vertical")}{(e.IsLockable ? ", lockable" : string.Empty)}"));
        EnyoGestures.AddFlickHandler(Target, (_, e) => Write($"Flick {e.XVelocity:0.00}, {e.YVelocity:0.00} px/ms"));
        ClearButton.Click += (_, _) => Log.Text = string.Empty;
    }

    private void Write(string line) => Log.Text = line + Environment.NewLine + Log.Text;
}
