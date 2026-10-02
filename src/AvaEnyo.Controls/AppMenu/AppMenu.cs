using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using AvaEnyo.Controls.Primitives;

namespace AvaEnyo.Controls;

/// <summary>
/// The webOS application menu (<c>enyo.AppMenu</c>): a dark menu fixed at the top-left of the window, opened by
/// tapping the app's name in the status bar. Its items are <see cref="AppMenuItem"/>s in an
/// <see cref="OrderedPanel"/>. It does not open while a modal popup is open. Opening it from keys or a region of the
/// window is the job of <c>AvaEnyo.Services.AppMenuService</c>.
/// </summary>
public class AppMenu : PopupBase
{
    /// <summary>Initializes a new instance of the <see cref="AppMenu"/> class.</summary>
    public AppMenu()
    {
        var host = new ItemsControl
        {
            ItemsSource = Items,
            ItemsPanel = new FuncTemplate<Panel?>(() => new OrderedPanel()),
        };
        Content = host;
    }

    /// <summary>The menu items, usually <see cref="AppMenuItem"/>s.</summary>
    [Content]
    public AvaloniaList<object> Items { get; } = new();

    /// <summary>
    /// True (default): the menu follows open and close requests from the app menu service (Enyo <c>automatic</c>).
    /// False: the app opens and closes it itself.
    /// </summary>
    public bool Automatic { get; set; } = true;

    /// <summary>True when a modal popup is open in the window, which keeps the app menu closed.</summary>
    public bool IsBlockedByModal =>
        TopLevel.GetTopLevel(this) is { } top
        && OverlayHost.Existing(top) is { } host
        && host.Stack.Any(p => p.IsModal && p != this);

    /// <summary>Opens the menu at the top-left of the window unless a modal popup is open.</summary>
    public new void Open()
    {
        if (!IsBlockedByModal)
        {
            OpenAt(default);
        }
    }

    /// <summary>Opens the menu when closed, closes it when open.</summary>
    public void Toggle()
    {
        if (IsOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    /// <inheritdoc/>
    protected override PopupPresenter CreatePresenter() => new AppMenuPresenter();

    /// <inheritdoc/>
    protected override void OnPresenterAttached(PopupPresenter presenter)
    {
        base.OnPresenterAttached(presenter);
        var focused = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() as TextBox;
        foreach (var item in Items)
        {
            if (item is EditMenu edit)
            {
                edit.UpdateItems(focused);
            }
        }
    }
}
