using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AvaEnyo.Controls;

namespace AvaEnyo.Gallery;

/// <summary>
/// The gallery shell, laid out like Enyo's StyleMatters: a page header, then a SlidingPane with the index, the
/// sample page and a source pullout. Below 900 px the pane is in narrow mode, so it works in a phone-sized window.
/// </summary>
public partial class MainView : UserControl
{
    private GalleryPage? _current;
    private readonly AppMenu _menu;
    private AvaEnyo.Services.ApplicationEvents? _events;

    public MainView()
    {
        InitializeComponent();
        BuildIndex();
        // The grab button on the sample toolbar pulls out the source view (StyleMatters' pullout).
        SampleGrab.Click += (_, e) =>
        {
            ShowSource();
            e.Handled = true;
        };
        SourceButton.Click += (_, _) => ShowSource();
        SourceTabs.ValueChanged += (_, _) => UpdateSource();
        _menu = GallerySettings.BuildMenu(PopupHost);
        PopupHost.Children.Add(_menu);
        AvaEnyo.Services.AppMenuService.SetIsOpenRegion(TitleText, true);
        // The pullout stops with its 400 px showing at the right edge (StyleMatters set its peek the same way).
        Sliding.SizeChanged += (_, e) => SourceView.PeekWidth = System.Math.Max(0, e.NewSize.Width - 400);
        Sliding.SelectionChanged += (_, _) =>
        {
            TitleHeader.ShowBackButton = Sliding.SelectedIndex > 0 && Sliding.IsNarrow;

            // Going back from the pullout puts it away again.
            if (Sliding.SelectedView != SourceView && SourceView.IsShowing)
            {
                SourceView.IsShowing = false;
            }

        };
        Sliding.PropertyChanged += (_, e) =>
        {
            if (e.Property == AvaEnyo.Controls.SlidingPane.IsNarrowProperty)
            {
                TitleHeader.ShowBackButton = Sliding.SelectedIndex > 0 && Sliding.IsNarrow;
            }
        };
        TitleHeader.BackRequested += (_, e) =>
        {
            e.Handled = Sliding.Back();
        };
        Navigate(GalleryPages.All.FirstOrDefault());
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (TopLevel.GetTopLevel(this) is { } top)
        {
            AvaEnyo.Services.AppMenuService.Attach(top, _menu);
            _events ??= AvaEnyo.Services.ApplicationEvents.Attach(top);
        }
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is { } top)
        {
            AvaEnyo.Services.AppMenuService.Detach(top);
        }

        _events?.Detach();
        _events = null;
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>The scroller of the current page.</summary>
    public ScrollViewer PageScrollViewer => PageScroller;

    /// <summary>Shows the page named <paramref name="name"/>. Returns false when there is none.</summary>
    public bool TryNavigate(string name)
    {
        if (GalleryPages.Find(name) is not { } page)
        {
            return false;
        }

        Navigate(page);
        ShowSample(immediate: true);
        return true;
    }

    private void BuildIndex()
    {
        foreach (var group in GalleryPages.Groups)
        {
            var pages = GalleryPages.All.Where(p => p.Group == group).ToList();
            if (pages.Count == 0)
            {
                continue;
            }

            IndexList.Children.Add(new Divider { Caption = group });
            foreach (var page in pages)
            {
                var item = new Item
                {
                    Content = new StackPanel
                    {
                        Children =
                        {
                            new TextBlock { Text = page.Name },
                            new TextBlock { Text = page.Summary, Classes = { "enyo-item-secondary", "enyo-text-ellipsis" } },
                        },
                    },
                    Tag = page,
                };
                item.Click += (_, _) =>
                {
                    Navigate(page);
                    ShowSample(immediate: false);
                };
                IndexList.Children.Add(item);
            }
        }
    }

    private void Navigate(GalleryPage? page)
    {
        if (page is null)
        {
            return;
        }

        _current = page;
        foreach (var item in IndexList.Children.OfType<Item>())
        {
            item.IsSelected = ReferenceEquals(item.Tag, page);
        }

        PageTitle.Text = page.Name;
        PageSummary.Text = page.Summary;
        var content = page.Create();
        ScrolledHost.Content = null;
        FillHost.Content = null;
        if (page.Scrolls)
        {
            ScrolledHost.Content = content;
            PageScroller.IsVisible = true;
            FillHost.IsVisible = false;
            PageScroller.Offset = default;
        }
        else
        {
            FillHost.Content = content;
            PageScroller.IsVisible = false;
            FillHost.IsVisible = true;
        }

        UpdateSource();
    }

    /// <summary>
    /// Brings the sample into view: it slides over the index when narrow, and stops beside it (its 320 px peek) when
    /// wide.
    /// </summary>
    private void ShowSample(bool immediate) => Sliding.SelectView(SampleView, immediate);

    private void ShowSource()
    {
        SourceView.IsShowing = true;
        Sliding.SelectView(SourceView);
    }

    private void UpdateSource()
    {
        if (_current is null)
        {
            return;
        }

        if (SourceTabs.SelectedIndex == 0)
        {
            SourceText.Text = _current.Notes ?? _current.Summary;
            return;
        }

        using var stream = typeof(MainView).Assembly.GetManifestResourceStream(_current.SourcePath);
        SourceText.Text = stream is null ? "(no source)" : new StreamReader(stream).ReadToEnd();
    }
}
