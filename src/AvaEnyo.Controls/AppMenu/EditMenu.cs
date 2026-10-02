using System;
using Avalonia;
using Avalonia.Controls;

namespace AvaEnyo.Controls;

/// <summary>
/// The Edit item of an <see cref="AppMenu"/> (<c>enyo.EditMenu</c>): opens to Select All, Cut, Copy and Paste,
/// which act on the text box that had focus when the menu opened. With <see cref="AutoDisableItems"/> (default) the
/// items are enabled only when a text box had focus; Cut and Copy also need a selection.
/// </summary>
public class EditMenu : AppMenuItem
{
    /// <summary>Defines the <see cref="AutoDisableItems"/> property.</summary>
    public static readonly StyledProperty<bool> AutoDisableItemsProperty =
        AvaloniaProperty.Register<EditMenu, bool>(nameof(AutoDisableItems), true);

    /// <summary>Defines the <see cref="ShowShortcuts"/> property.</summary>
    public static readonly StyledProperty<bool> ShowShortcutsProperty =
        AvaloniaProperty.Register<EditMenu, bool>(nameof(ShowShortcuts));

    private TextBox? _target;

    static EditMenu()
    {
        ShowShortcutsProperty.Changed.AddClassHandler<EditMenu>((m, e) => m.ApplyShortcuts(e.GetNewValue<bool>()));
    }

    /// <summary>Initializes a new instance of the <see cref="EditMenu"/> class.</summary>
    public EditMenu()
    {
        Header = "Edit";
        SelectAllItem = Make("Select All", "A", t => t.SelectAll());
        CutItem = Make("Cut", "X", t => t.Cut());
        CopyItem = Make("Copy", "C", t => t.Copy());
        PasteItem = Make("Paste", "V", t => t.Paste());
        Items.Add(SelectAllItem);
        Items.Add(CutItem);
        Items.Add(CopyItem);
        Items.Add(PasteItem);
    }

    /// <summary>True (default): enable the items by the focused text box's state.</summary>
    public bool AutoDisableItems
    {
        get => GetValue(AutoDisableItemsProperty);
        set => SetValue(AutoDisableItemsProperty, value);
    }

    /// <summary>True to show the Ctrl shortcut of each item.</summary>
    public bool ShowShortcuts
    {
        get => GetValue(ShowShortcutsProperty);
        set => SetValue(ShowShortcutsProperty, value);
    }

    /// <summary>The Select All item.</summary>
    public AppMenuItem SelectAllItem { get; }

    /// <summary>The Cut item.</summary>
    public AppMenuItem CutItem { get; }

    /// <summary>The Copy item.</summary>
    public AppMenuItem CopyItem { get; }

    /// <summary>The Paste item.</summary>
    public AppMenuItem PasteItem { get; }

    /// <summary>The text box the items act on.</summary>
    public TextBox? Target => _target;

    /// <summary>Sets the text box to act on and, with <see cref="AutoDisableItems"/>, enables the items by its state.</summary>
    public void UpdateItems(TextBox? target)
    {
        _target = target;
        if (!AutoDisableItems)
        {
            return;
        }

        var hasSelection = target is not null && target.SelectionStart != target.SelectionEnd;
        SelectAllItem.IsEnabled = target is not null;
        CutItem.IsEnabled = hasSelection && !target!.IsReadOnly;
        CopyItem.IsEnabled = hasSelection;
        PasteItem.IsEnabled = target is not null && !target.IsReadOnly;
    }

    private AppMenuItem Make(string header, string shortcut, Action<TextBox> action)
    {
        var item = new AppMenuItem { Header = header, Shortcut = shortcut, Value = header };
        item.Click += (_, _) =>
        {
            if (_target is { } t)
            {
                action(t);
            }
        };
        return item;
    }

    private void ApplyShortcuts(bool show)
    {
        SelectAllItem.ShowShortcut = show;
        CutItem.ShowShortcut = show;
        CopyItem.ShowShortcut = show;
        PasteItem.ShowShortcut = show;
    }
}
