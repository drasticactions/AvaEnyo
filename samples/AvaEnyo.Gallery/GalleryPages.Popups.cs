using System.Collections.Generic;
using AvaEnyo.Gallery.Pages.Popups;

namespace AvaEnyo.Gallery;

public static partial class GalleryPages
{
    static partial void AddPopups(List<GalleryPage> pages)
    {
        const string g = "Popups";
        pages.Add(new(g, "Modal Dialogs", "ModalDialog with ShowAsync, and DialogPrompt", () => new ModalDialogsPage(), "src/Popups/ModalDialogsPage.axaml",
            Notes: "A ModalDialog is 320 px wide in the dialog.png frame, over a scrim, and fades over 500 ms. An outside click does not close it. ShowAsync returns the close value of the button that closed it."));
        pages.Add(new(g, "Popups", "Every placement method, modal and scrim options, close reasons", () => new PopupsPage(), "src/Popups/PopupsPage.axaml",
            Notes: "A popup is a zero-size placeholder; its presenter opens in the window's overlay layer. Placement is kept inside the window. Escape or back close the topmost popup; a click outside closes it unless it is modal."));
        pages.Add(new(g, "Menus", "Menus with nested and check items, PopupSelect and PopupList", () => new MenusPage(), "src/Popups/MenusPage.axaml",
            Notes: "A Menu draws the menu-background.png frame. A leaf item closes the menu with its Value; an item with children opens an indented drawer. PopupList virtualizes its rows."));
        pages.Add(new(g, "Toasters & Dialogs", "Toasters from each edge and the bottom dialog", () => new ToastersPage(), "src/Popups/ToastersPage.axaml",
            Notes: "A Toaster slides in from its edge over 350 ms with a cubic ease-out. Dragging a sliding handle toward the edge dismisses it."));
        pages.Add(new(g, "App Menu", "The dark app menu with Edit and Help", () => new AppMenuPage(), "src/Popups/AppMenuPage.axaml",
            Notes: "The app menu opens at the top-left in the appmenu.png look. In the gallery, tapping the header title opens the gallery's own app menu; AppMenuService also opens it with Ctrl+` and F10. EditMenu acts on the focused text box."));
        pages.Add(new(g, "Notifications", "Dashboard notifications with layers", () => new NotificationsPage(), "src/Popups/NotificationsPage.axaml",
            Notes: "InAppNotificationService shows webOS dashboards at the bottom edge, up to three at once. A notification can stack layers, newest on top, with a count badge."));
    }
}
