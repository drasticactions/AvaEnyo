using Avalonia.Controls;
using Avalonia.Layout;

namespace AvaEnyo.Gallery.Pages.StockControls;

public partial class PagesPage : UserControl
{
    public PagesPage()
    {
        InitializeComponent();
        Kind.ValueChanged += (_, _) => Show();
        Show();
    }

    private void Show()
    {
        Host.Content = Kind.SelectedIndex switch
        {
            1 => new TabbedPage { Pages = new[] { Page("Inbox"), Page("Sent"), Page("Drafts") } },
            2 => new DrawerPage { Drawer = Page("Folders"), Content = Page("Messages") },
            3 => new CarouselPage { Pages = new[] { Page("One"), Page("Two"), Page("Three") } },
            _ => Navigation(),
        };
    }

    private static NavigationPage Navigation()
    {
        var nav = new NavigationPage();
        var depth = 0;
        ContentPage Next()
        {
            depth++;
            var push = new Button { Content = "Push page " + (depth + 1), Classes = { "blue" } };
            var page = new ContentPage { Header = "Page " + depth, Content = new StackPanel { Margin = new Avalonia.Thickness(16), Spacing = 8, Children = { new TextBlock { Text = "Page " + depth + " of the navigation stack." }, push } } };
            push.Click += async (_, _) => await nav.PushAsync(Next());
            return page;
        }

        _ = nav.PushAsync(Next());
        return nav;
    }

    private static ContentPage Page(string title) => new()
    {
        Header = title,
        Content = new TextBlock { Text = title, Margin = new Avalonia.Thickness(16), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center },
    };
}
