using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using AvaEnyo.Controls.Primitives;
using Xunit;

namespace AvaEnyo.Controls.Tests;

public class ListTests
{
    private static List<string> Words(int count) =>
        Enumerable.Range(0, count).Select(i => ((char)('A' + (i / 10))).ToString() + i.ToString("000")).ToList();

    private static Item Row(VirtualList list, int index) => (Item)list.ContainerFromIndex(index)!;

    [AvaloniaFact]
    public void Dividers_Mark_Key_Changes_And_Survive_Recycling()
    {
        var list = new VirtualList { ItemsSource = Words(200), DividerKeySelector = o => ((string)o!)[..1] };
        var w = TestHost.Show(list, 300, 800);
        Assert.True(Row(list, 0).ShowDivider);
        Assert.Equal("A", Row(list, 0).DividerCaption);
        Assert.False(Row(list, 1).ShowDivider);
        Assert.False(Row(list, 9).ShowDivider);
        Assert.True(Row(list, 10).ShowDivider);
        Assert.Equal("B", Row(list, 10).DividerCaption);

        list.ScrollViewer!.Offset = new Vector(0, 100000);
        TestHost.Settle(w);
        list.ScrollViewer.Offset = default;
        TestHost.Settle(w);
        Assert.True(Row(list, 0).ShowDivider);
        Assert.False(Row(list, 1).ShowDivider);
        Assert.True(Row(list, 10).ShowDivider);
        foreach (var c in list.GetRealizedContainers().OfType<Item>().Where(c => c.IsVisible))
        {
            var i = list.IndexFromContainer(c);
            Assert.Equal(i % 10 == 0, c.ShowDivider);
        }

        w.Close();
    }

    [AvaloniaFact]
    public void Grid_Realizes_Only_Visible_Rows()
    {
        var list = new VirtualList { Layout = VirtualListLayout.Grid, ItemsPerRow = 4, ItemsSource = Words(400) };
        var w = TestHost.Show(list, 400, 400);
        var panel = list.GetVisualDescendants().OfType<VirtualizingUniformGridPanel>().Single();
        var realized = list.GetRealizedContainers().Count();
        Assert.InRange(realized, 16, 32);
        Assert.Equal(4, panel.Columns);
        Assert.Equal(100, Row(list, 0).Bounds.Width, 0);
        Assert.Equal(100, Row(list, 5).Bounds.X, 0);
        list.ScrollViewer!.Offset = new Vector(0, 5000);
        TestHost.Settle(w);
        Assert.Null(list.ContainerFromIndex(0));
        Assert.True(panel.FirstRealizedIndex >= 180);
        Assert.InRange(list.GetRealizedContainers().Count(), 16, 32);
        w.Close();
    }

    [AvaloniaFact]
    public void Paged_Source_Acquires_And_Discards_Pages()
    {
        var source = new PagedCollection<string>(500, (page, _) => Task.FromResult<IReadOnlyList<string>>(Enumerable.Range(page * 10, 10).Select(i => "Item " + i).ToList()));
        var acquired = new List<int>();
        var discarded = new List<int>();
        var list = new VirtualList { ItemsSource = source };
        list.AcquirePage += (_, e) => acquired.Add(e.Page);
        list.DiscardPage += (_, e) => discarded.Add(e.Page);
        var w = TestHost.Show(list, 300, 400);
        Assert.Contains(0, acquired);
        var lastVisiblePage = list.GetRealizedContainers().Where(c => c.IsVisible).Max(list.IndexFromContainer) / 10;
        Assert.Contains(lastVisiblePage + 2, list.AcquiredPages);
        Assert.DoesNotContain(lastVisiblePage + 3, list.AcquiredPages);
        Assert.Equal("Item 0", source[0]);
        list.ScrollViewer!.Offset = new Vector(0, 100000);
        TestHost.Settle(w);
        Assert.Contains(0, discarded);
        Assert.Contains(49, list.AcquiredPages);
        Assert.Null(source[0]);
        w.Close();
    }

    [AvaloniaFact]
    public void Toggle_Mode_Toggles_On_Tap()
    {
        var list = new VirtualList { ItemsSource = Words(20), SelectionMode = SelectionMode.Toggle };
        var w = TestHost.Show(list, 300, 400);
        var touch = new TouchInput(w);
        var p = Row(list, 1).TranslatePoint(new Point(20, 10), w)!.Value;
        touch.Tap(p);
        Assert.True(list.IsSelected(1));
        Assert.True(Row(list, 1).IsSelected);
        touch.Tap(p);
        Assert.False(list.IsSelected(1));
        w.Close();
    }

    [AvaloniaFact]
    public void Multiple_Mode_Keeps_Several()
    {
        var list = new VirtualList { ItemsSource = Words(20), SelectionMode = SelectionMode.Multiple };
        var w = TestHost.Show(list, 300, 400);
        list.Select(0);
        list.Select(3);
        Assert.Equal(2, list.SelectedItems!.Count);
        Assert.True(list.IsSelected(3));
        list.Deselect(0);
        Assert.False(list.IsSelected(0));
        w.Close();
    }

    [AvaloniaFact]
    public void Single_Mode_Selects_On_Tap()
    {
        var list = new VirtualList { ItemsSource = Words(20) };
        var w = TestHost.Show(list, 300, 400);
        var touch = new TouchInput(w);
        touch.Tap(Row(list, 2).TranslatePoint(new Point(20, 10), w)!.Value);
        Assert.Equal(2, list.SelectedIndex);
        touch.Tap(Row(list, 4).TranslatePoint(new Point(20, 10), w)!.Value);
        Assert.Equal(4, list.SelectedIndex);
        Assert.Single(list.SelectedItems!);
        w.Close();
    }

    [AvaloniaFact]
    public void Refresh_Rebuilds_Rows_And_Reset_Clears()
    {
        var words = new List<string>(Words(100));
        var list = new VirtualList { ItemsSource = words, SelectionMode = SelectionMode.Multiple };
        var w = TestHost.Show(list, 300, 400);
        words[0] = "Changed";
        list.Refresh();
        Assert.Equal("Changed", Row(list, 0).Content);
        list.Select(1);
        list.ScrollViewer!.Offset = new Vector(0, 300);
        TestHost.Settle(w);
        list.Reset();
        TestHost.Settle(w);
        Assert.Equal(0, list.ScrollViewer.Offset.Y);
        Assert.Empty(list.SelectedItems!);
        w.Close();
    }

    private static (VirtualList List, ObservableCollection<string> Items, Window Window) Reorderable()
    {
        var items = new ObservableCollection<string>(Words(20));
        var list = new VirtualList { ItemsSource = items, CanReorder = true };
        var w = TestHost.Show(list, 300, 600);
        return (list, items, w);
    }

    private static void HoldAndDrag(Window w, VirtualList list, int from, int rows)
    {
        var row = Row(list, from);
        var start = row.TranslatePoint(new Point(40, row.Bounds.Height / 2), w)!.Value;
        var touch = new TouchInput(w);
        var old = EnyoGestures.HoldDelay;
        EnyoGestures.HoldDelay = 20;
        try
        {
            touch.Down(start);
            TestHost.Pump(1000, () => list.IsReordering);
            Assert.True(list.IsReordering);
            touch.MoveBy(start, new Vector(0, row.Bounds.Height * rows), 6);
            touch.Up(start + new Vector(0, row.Bounds.Height * rows));
        }
        finally
        {
            EnyoGestures.HoldDelay = old;
        }
    }

    [AvaloniaFact]
    public void Hold_And_Drag_Reorders()
    {
        var (list, items, w) = Reorderable();
        var moved = items[0];
        HoldAndDrag(w, list, 0, 2);
        TestHost.Settle(w);
        Assert.False(list.IsReordering);
        Assert.Equal(moved, items[2]);
        Assert.Null(Row(list, 1).RenderTransform);
        w.Close();
    }

    [AvaloniaFact]
    public void Reordering_Can_Be_Canceled()
    {
        var (list, items, w) = Reorderable();
        ReorderingEventArgs? seen = null;
        list.Reordering += (_, e) =>
        {
            seen = e;
            e.Cancel = true;
        };
        var first = items[0];
        HoldAndDrag(w, list, 0, 3);
        Assert.NotNull(seen);
        Assert.Equal(0, seen!.From);
        Assert.Equal(3, seen.To);
        Assert.Equal(first, items[0]);
        w.Close();
    }

    private static (SwipeableItem Row, Window Window) SwipeRow(bool allowLeft = true, bool confirmRequired = true)
    {
        var row = new SwipeableItem { Content = "Swipe", Width = 300, AllowLeft = allowLeft, ConfirmRequired = confirmRequired };
        var w = TestHost.Show(new StackPanel { Children = { row } }, 400, 300);
        return (row, w);
    }

    private static void Swipe(Window w, SwipeableItem row, double dx)
    {
        var start = row.TranslatePoint(new Point(150, row.Bounds.Height / 2), w)!.Value;
        var touch = new TouchInput(w);
        touch.Drag(start, new Vector(dx, 0), 10);
        touch.Up(start + new Vector(dx, 0));
        TestHost.Settle(w);
    }

    [AvaloniaFact]
    public void Swipe_Below_35_Percent_Returns()
    {
        var (row, w) = SwipeRow();
        var swiped = 0;
        row.Swiped += (_, _) => swiped++;
        Swipe(w, row, 300 * 0.34);
        Assert.Equal(0, swiped);
        Assert.False(row.ConfirmShowing);
        Assert.Equal(0, row.SwipeOffset);
        w.Close();
    }

    [AvaloniaFact]
    public void Swipe_Past_35_Percent_Shows_The_Prompt()
    {
        var (row, w) = SwipeRow();
        var swiped = 0;
        var confirmed = 0;
        var cancelled = 0;
        row.Swiped += (_, _) => swiped++;
        row.Confirmed += (_, _) => confirmed++;
        row.Cancelled += (_, _) => cancelled++;
        Swipe(w, row, 300 * 0.36);
        Assert.Equal(1, swiped);
        Assert.True(row.ConfirmShowing);
        var prompt = row.GetVisualDescendants().OfType<ConfirmPrompt>().Single();
        Assert.True(prompt.IsVisible);
        prompt.Cancel();
        Assert.Equal(1, cancelled);
        Assert.False(row.ConfirmShowing);
        Swipe(w, row, -300 * 0.5);
        Assert.True(row.ConfirmShowing);
        prompt.Confirm();
        Assert.Equal(1, confirmed);
        Assert.False(row.ConfirmShowing);
        w.Close();
    }

    [AvaloniaFact]
    public void Prompt_Shows_Under_The_Row_While_Swiping()
    {
        var (row, w) = SwipeRow();
        var prompt = row.GetVisualDescendants().OfType<ConfirmPrompt>().Single();
        var border = row.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_Border");
        Assert.False(prompt.IsVisible);

        var start = row.TranslatePoint(new Point(150, row.Bounds.Height / 2), w)!.Value;
        var touch = new TouchInput(w);
        touch.Drag(start, new Vector(60, 0), 10);
        Assert.True(prompt.IsVisible);
        Assert.False(prompt.IsHitTestVisible);
        Assert.Equal(1, border.Opacity);
        var height = row.Bounds.Height;

        touch.Drag(start + new Vector(60, 0), new Vector(60, 0), 10);
        touch.Up(start + new Vector(120, 0));
        TestHost.Settle(w);
        Assert.True(row.ConfirmShowing);
        Assert.True(prompt.IsHitTestVisible);
        Assert.Equal(0, border.Opacity);
        Assert.Equal(height, row.Bounds.Height);

        prompt.Cancel();
        TestHost.Settle(w);
        Assert.Equal(1, border.Opacity);
        Assert.Equal(0, row.SwipeOffset);
        Assert.False(prompt.IsVisible);
        w.Close();
    }

    [AvaloniaFact]
    public void Swipe_Without_Confirmation_Confirms_At_Once()
    {
        var (row, w) = SwipeRow(confirmRequired: false);
        var confirmed = 0;
        row.Confirmed += (_, _) => confirmed++;
        Swipe(w, row, 200);
        Assert.Equal(1, confirmed);
        Assert.False(row.ConfirmShowing);
        w.Close();
    }

    [AvaloniaFact]
    public void AllowLeft_False_Clamps_Leftward_Drags()
    {
        var (row, w) = SwipeRow(allowLeft: false);
        var start = row.TranslatePoint(new Point(150, row.Bounds.Height / 2), w)!.Value;
        var touch = new TouchInput(w);
        touch.Drag(start, new Vector(-200, 0), 10);
        Assert.Equal(0, row.SwipeOffset);
        touch.Up(start + new Vector(-200, 0));
        Assert.False(row.ConfirmShowing);
        w.Close();
    }

    [AvaloniaFact]
    public void Swiping_A_Row_Does_Not_Select_It()
    {
        var list = new VirtualList { ItemsSource = Words(30), Swipeable = true };
        var w = TestHost.Show(list, 300, 500);
        var row = (SwipeableItem)Row(list, 1);

        Swipe(w, row, 300 * 0.2);
        Assert.Equal(-1, list.SelectedIndex);
        Assert.DoesNotContain(":held", row.Classes);

        Swipe(w, row, 300 * 0.5);
        Assert.True(row.ConfirmShowing);
        row.GetVisualDescendants().OfType<ConfirmPrompt>().Single().Cancel();
        TestHost.Settle(w);
        Assert.Equal(-1, list.SelectedIndex);
        Assert.DoesNotContain(":held", row.Classes);

        var tap = new TouchInput(w);
        var p = row.TranslatePoint(new Point(150, row.Bounds.Height / 2), w)!.Value;
        tap.Down(p);
        tap.Up(p);
        TestHost.Settle(w);
        Assert.Equal(1, list.SelectedIndex);
        w.Close();
    }

    [AvaloniaFact]
    public void Only_One_Row_Confirms_At_A_Time()
    {
        var list = new VirtualList { ItemsSource = Words(30), Swipeable = true };
        var w = TestHost.Show(list, 300, 500);
        var a = (SwipeableItem)Row(list, 0);
        var b = (SwipeableItem)Row(list, 1);
        a.ConfirmWhenAutoHidden = true;
        var autoConfirmed = 0;
        a.Confirmed += (_, _) => autoConfirmed++;
        ConfirmShowingChangedEventArgs? change = null;
        b.ConfirmShowingChanged += (_, e) => change = e;
        a.ConfirmShowing = true;
        b.ConfirmShowing = true;
        Assert.False(a.ConfirmShowing);
        Assert.True(b.ConfirmShowing);
        Assert.Equal(1, autoConfirmed);
        Assert.True(change!.DidAutoConfirm);
        w.Close();
    }

    [AvaloniaFact]
    public void Vertical_Drag_On_A_Swipeable_List_Scrolls()
    {
        var list = new VirtualList { ItemsSource = Words(100), Swipeable = true };
        var w = TestHost.Show(list, 300, 400);
        var touch = new TouchInput(w);
        touch.Drag(new Point(150, 300), new Vector(0, -150));
        touch.Up(new Point(150, 150));
        TestHost.Settle(w);
        Assert.True(list.ScrollViewer!.Offset.Y > 100);
        Assert.All(list.GetRealizedContainers().OfType<SwipeableItem>(), r => Assert.False(r.ConfirmShowing));
        w.Close();
    }

    [AvaloniaFact]
    public void A_Selected_Grid_Cell_Shows_Its_Selection_Over_Its_Content()
    {
        var list = new VirtualList
        {
            Layout = VirtualListLayout.Grid,
            ItemsPerRow = 4,
            ItemHeight = 100,
            SelectionMode = SelectionMode.Multiple | SelectionMode.Toggle,
            ItemsSource = Enumerable.Range(0, 16).ToList(),
            ItemTemplate = new FuncDataTemplate<int>((_, _) => new Border { Width = 90, Height = 90, Background = Brushes.White }),
        };
        var w = TestHost.Show(list, 400, 400);
        list.Select(1);
        var frame = TestHost.Render(w);
        Color Center(int index) => TestHost.PixelAt(frame, Row(list, index).TranslatePoint(new Point(50, 50), w)!.Value);
        Assert.Equal(Colors.White, Center(0));
        Assert.NotEqual(Colors.White, Center(1));
        w.Close();
    }
}
