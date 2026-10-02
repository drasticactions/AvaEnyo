using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvaEnyo;

/// <summary>
/// Lets the Enyo back chain go back in a <see cref="NavigationPage"/>: <see cref="EnyoBack.BackRequestedEvent"/>
/// reaching a navigation page is turned into the page's own system-back event, which closes the top modal or pops
/// the top page. The navigation page marks it handled, so a <c>TopLevel</c> back request does not pop twice.
/// </summary>
internal static class NavigationBack
{
    public static void Register()
    {
        EnyoBack.BackRequestedEvent.AddClassHandler<NavigationPage>(OnBackRequested);
    }

    private static void OnBackRequested(NavigationPage page, RoutedEventArgs e)
    {
        if (e.Handled || (page.StackDepth <= 1 && page.ModalStack.Count == 0))
        {
            return;
        }

        var forwarded = new RoutedEventArgs(Page.PageNavigationSystemBackButtonPressedEvent);
        page.RaiseEvent(forwarded);
        e.Handled = forwarded.Handled;
    }
}
