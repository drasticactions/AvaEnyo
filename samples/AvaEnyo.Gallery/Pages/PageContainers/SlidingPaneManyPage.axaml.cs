using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using AvaEnyo.Controls;
using AvaEnyo.Controls.Primitives;

namespace AvaEnyo.Gallery.Pages.PageContainers;

public partial class SlidingPaneManyPage : UserControl
{
    private const string Lorem = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged.";

    public SlidingPaneManyPage()
    {
        InitializeComponent();
        for (var i = 1; i <= 3; i++)
        {
            var index = i;
            var item = new Item { TapHighlight = false, Content = new Button { Content = "Button " + i, HorizontalAlignment = HorizontalAlignment.Stretch } };
            ((Button)item.Content).Click += (_, _) => Pane.SelectViewByIndex(System.Math.Min(index, Pane.Views.Count - 1));
            PeekItems.Children.Add(item);
        }

        Pane.Views.Add(MakeSecondView());
        Pane.Views.Add(MakeView("Panel 3", flex: true));
        MultiViewButton.Click += (_, _) => Pane.MultiView = MultiViewButton.IsChecked;
        AddButton.Click += (_, _) =>
        {
            var last = Pane.Views.Last();
            Flex.SetValue(last, 0);
            last.Width = 320;
            var view = MakeView("Panel " + (Pane.Views.Count + 1), flex: true);
            Pane.Views.Add(view);
            Pane.SelectView(view);
        };
    }

    private SlidingView MakeSecondView()
    {
        var rows = new RowGroup { Header = "Inputs", Items = { new TextBox(), new TextBox(), new TextBox() } };
        var hide = new Button { Content = "Hide next", HorizontalAlignment = HorizontalAlignment.Stretch };
        var show = new Button { Content = "Show next", HorizontalAlignment = HorizontalAlignment.Stretch };
        hide.Click += (_, _) => SetNextShowing(1, false);
        show.Click += (_, _) => SetNextShowing(1, true);
        var body = new StackPanel { Margin = new Thickness(8), Children = { rows, hide, show } };
        var view = MakeView("Panel 2", flex: false, body);
        view.FixedWidth = true;
        return view;
    }

    private SlidingView MakeView(string title, bool flex, Control? body = null)
    {
        var go = new ToolButton { Content = "Go" };
        var view = new SlidingView
        {
            Content = new DockPanel
            {
                Children =
                {
                    new Header { Content = title, [DockPanel.DockProperty] = Dock.Top },
                    new Toolbar { Items = { new GrabButton(), go }, [DockPanel.DockProperty] = Dock.Bottom },
                    new ScrollViewer
                    {
                        Content = body ?? new TextBlock { Text = Lorem + "\n\n" + Lorem, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(12), Classes = { "enyo-paragraph" } },
                    },
                },
            },
        };
        if (flex)
        {
            Flex.SetValue(view, 1);
        }
        else
        {
            view.Width = 320;
        }

        go.Click += (_, _) => view.SelectNext();
        return view;
    }

    private void SetNextShowing(int index, bool showing)
    {
        if (index + 1 < Pane.Views.Count)
        {
            Pane.Views[index + 1].IsShowing = showing;
        }
    }
}
