using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Xunit;

namespace AvaEnyo.Services.Tests;

public class NotificationTests
{
    [AvaloniaFact]
    public void At_Most_Three_Dashboards_Show_Newest_At_The_Bottom()
    {
        var w = Host.Show(new Canvas());
        var service = new InAppNotificationService(w);
        for (var i = 0; i < 5; i++)
        {
            service.Show(new Notification("n" + i) { Title = "Mail " + i, Text = "From someone" });
        }

        Host.Settle(w);
        Assert.Equal(5, service.Notifications.Count);
        var visible = service.VisiblePresenters;
        Assert.Equal(3, visible.Count);
        Assert.Equal("n4", visible[^1].Notification!.Id);
        Assert.True(visible[^1].Bounds.Height > 0);
        var bottom = visible[^1].TranslatePoint(new Point(0, visible[^1].Bounds.Height), w)!.Value.Y;
        Assert.InRange(bottom, 580, 600);
        w.Close();
    }

    [AvaloniaFact]
    public void Layers_Count_In_The_Badge()
    {
        var w = Host.Show(new Canvas());
        var service = new InAppNotificationService(w);
        var n = new Notification { Title = "Email" };
        service.Show(n);
        n.AddLayer("Alice", "Lunch?");
        n.AddLayer("Bob", "Meeting");
        Host.Settle(w);
        Assert.Equal(2, n.LayerCount);
        Assert.Equal("Bob", n.DisplayTitle);
        Assert.Contains(":layered", service.VisiblePresenters[0].Classes);
        w.Close();
    }

    [AvaloniaFact]
    public void Swipe_Dismisses_And_Tap_Activates()
    {
        var w = Host.Show(new Canvas());
        var service = new InAppNotificationService(w);
        var a = new Notification("a") { Title = "A" };
        var b = new Notification("b") { Title = "B" };
        service.Show(a);
        service.Show(b);
        Host.Settle(w);
        Notification? activated = null;
        Notification? dismissed = null;
        service.Activated += (_, e) => activated = e.Notification;
        service.Dismissed += (_, e) => dismissed = e.Notification;

        var pb = service.VisiblePresenters[^1];
        var center = pb.TranslatePoint(new Point(pb.Bounds.Width / 2, pb.Bounds.Height / 2), w)!.Value;
        new TouchInput(w).Tap(center);
        Assert.Same(b, activated);

        var touch = new TouchInput(w);
        touch.Drag(center, new Vector(400, 0), steps: 10);
        touch.Up(center + new Vector(400, 0));
        Host.Settle(w);
        Assert.Same(b, dismissed);
        Assert.Single(service.Notifications);
        Assert.Single(service.VisiblePresenters);
        w.Close();
    }

    [AvaloniaFact]
    public void Short_Swipe_Springs_Back()
    {
        var w = Host.Show(new Canvas());
        var service = new InAppNotificationService(w);
        service.Show(new Notification("a") { Title = "A" });
        Host.Settle(w);
        var p = service.VisiblePresenters[0];
        var center = p.TranslatePoint(new Point(p.Bounds.Width / 2, p.Bounds.Height / 2), w)!.Value;
        var touch = new TouchInput(w);
        touch.Drag(center, new Vector(40, 0), steps: 8);
        touch.StepMs = 200;
        touch.Up(center + new Vector(40, 0));
        Host.Settle(w);
        Assert.Single(service.Notifications);
        w.Close();
    }

    [AvaloniaFact]
    public void Show_With_The_Same_Id_Updates_And_Backend_Sees_It()
    {
        var w = Host.Show(new Canvas());
        var backend = new RecordingBackend();
        var service = new InAppNotificationService(w) { Backend = backend };
        service.Show(new Notification("x") { Title = "One" });
        service.Show(new Notification("x") { Title = "Two" });
        service.Dismiss("x");
        Assert.Equal("show update dismiss", string.Join(' ', backend.Log));
        Assert.Empty(service.Notifications);
        w.Close();
    }

    [Fact]
    public void Test_Double_Keeps_A_List()
    {
        var service = new TestNotificationService();
        var n = new Notification("t");
        service.Show(n);
        Notification? activated = null;
        service.Activated += (_, e) => activated = e.Notification;
        service.Activate(n);
        Assert.Same(n, activated);
        service.Dismiss("t");
        Assert.Empty(service.Notifications);
    }

    private sealed class RecordingBackend : INotificationBackend
    {
        public System.Collections.Generic.List<string> Log { get; } = new();

        public void Show(Notification notification) => Log.Add("show");

        public void Update(Notification notification) => Log.Add("update");

        public void Dismiss(Notification notification) => Log.Add("dismiss");
    }
}
