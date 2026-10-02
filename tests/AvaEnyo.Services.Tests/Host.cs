using System;
using Avalonia.Controls;
using Avalonia.Threading;

namespace AvaEnyo.Services.Tests;

public static class Host
{
    public static Window Show(Control content, double width = 800, double height = 600)
    {
        var window = new Window { Content = content, Width = width, Height = height };
        window.Show();
        Settle(window);
        return window;
    }

    public static void Settle(Window window)
    {
        for (var i = 0; i < 4; i++)
        {
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
