using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Xunit;

namespace AvaEnyo.Controls.Tests;

public class PaneTests
{
    [AvaloniaFact]
    public void First_View_Is_Selected_And_Others_Hidden()
    {
        var a = new Border { Name = "a" };
        var b = new Border { Name = "b" };
        var pane = new Pane { Views = { a, b } };
        var w = TestHost.Show(pane);
        Assert.Same(a, pane.SelectedView);
        Assert.True(a.IsVisible);
        Assert.False(b.IsVisible);
        w.Close();
    }

    [AvaloniaFact]
    public void History_And_Back()
    {
        var pane = new Pane { Views = { new Border { Name = "a" }, new Border { Name = "b" }, new Border { Name = "c" } } };
        var w = TestHost.Show(pane);
        pane.SelectViewByName("b");
        pane.SelectViewByName("c");
        Assert.True(pane.CanGoBack);
        Assert.True(pane.Back());
        Assert.Equal("b", pane.SelectedView!.Name);
        Assert.True(pane.Back());
        Assert.Equal("a", pane.SelectedView!.Name);
        Assert.False(pane.CanGoBack);
        Assert.False(pane.Back());
        w.Close();
    }

    [AvaloniaFact]
    public void History_Is_Capped()
    {
        var pane = new Pane { MaxHistory = 3, Views = { new Border(), new Border() } };
        var w = TestHost.Show(pane);
        for (var i = 0; i < 10; i++)
        {
            pane.Next();
        }

        var backs = 0;
        while (pane.Back())
        {
            backs++;
        }

        Assert.Equal(3, backs);
        w.Close();
    }

    [AvaloniaFact]
    public void Lazy_View_Builds_On_First_Select()
    {
        var built = 0;
        var lazy = new PaneView
        {
            Name = "lazy",
            IsLazy = true,
            ContentTemplate = new Avalonia.Controls.Templates.FuncDataTemplate<object?>((_, _) =>
            {
                built++;
                return new TextBlock { Text = "made" };
            }),
        };
        var pane = new Pane { Views = { new Border(), lazy } };
        var w = TestHost.Show(pane);
        Assert.Equal(0, built);
        Assert.False(lazy.IsRealized);
        pane.SelectViewByName("lazy");
        Assert.Equal(1, built);
        Assert.True(lazy.IsRealized);
        w.Close();
    }

    [AvaloniaFact]
    public void Unknown_Name_Raises_CreateView()
    {
        var pane = new Pane { Views = { new Border() } };
        pane.CreateView += (_, e) => e.View = new TextBlock { Text = e.Name };
        var w = TestHost.Show(pane);
        var v = pane.SelectViewByName("dynamic");
        Assert.NotNull(v);
        Assert.Equal("dynamic", v!.Name);
        Assert.Same(v, pane.SelectedView);
        Assert.Equal(2, pane.Views.Count);
        w.Close();
    }

    [AvaloniaFact]
    public void Requests_During_A_Transition_Are_Queued()
    {
        var old = Animations.EnyoAnimations.TimeScale;
        Animations.EnyoAnimations.TimeScale = 1;
        try
        {
            var transition = new GatedTransition();
            var a = new Border { Name = "a" };
            var b = new Border { Name = "b" };
            var c = new Border { Name = "c" };
            var pane = new Pane { Transition = transition, Views = { a, b, c } };
            var w = TestHost.Show(pane);
            pane.SelectView(b);
            Assert.True(pane.IsTransitioning);
            pane.SelectView(c);
            Assert.Equal(1, transition.Started);
            transition.Release();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(2, transition.Started);
            transition.Release();
            Dispatcher.UIThread.RunJobs();
            Assert.Same(c, pane.SelectedView);
            Assert.False(pane.IsTransitioning);
            Assert.True(c.IsVisible);
            Assert.False(b.IsVisible);
            w.Close();
        }
        finally
        {
            Animations.EnyoAnimations.TimeScale = old;
        }
    }

    [AvaloniaFact]
    public void Escape_Goes_Back()
    {
        var pane = new Pane { Views = { new Border { Name = "a" }, new Border { Name = "b" } } };
        var w = TestHost.Show(pane);
        pane.SelectViewByIndex(1);
        Assert.True(EnyoBack.RequestBack(w));
        Assert.Equal(0, pane.SelectedIndex);
        w.Close();
    }

    private sealed class GatedTransition : IPageTransition
    {
        private TaskCompletionSource? _gate;

        public int Started { get; private set; }

        public void Release() => _gate?.TrySetResult();

        public async Task Start(Visual? from, Visual? to, bool forward, CancellationToken cancellationToken)
        {
            Started++;
            _gate = new TaskCompletionSource();
            await _gate.Task;
        }
    }
}
