using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AvaEnyo.Theme.Tests;

/// <summary>Control-specific state changes that each template must survive.</summary>
public static class StateWalk
{
    public static void Apply(Control control, Window window)
    {
        switch (control)
        {
            case ToggleButton tb:
                tb.IsChecked = true; window.UpdateLayout();
                tb.IsChecked = null; window.UpdateLayout();
                tb.IsChecked = false; window.UpdateLayout();
                break;
            case SelectingItemsControl sic when sic.ItemCount > 0:
                sic.SelectedIndex = 0; window.UpdateLayout();
                break;
        }

        switch (control)
        {
            case ListBoxItem lbi:
                lbi.IsSelected = true; window.UpdateLayout();
                break;
            case ComboBox cb:
                cb.IsDropDownOpen = true; window.UpdateLayout();
                cb.IsDropDownOpen = false; window.UpdateLayout();
                break;
            case ProgressBar pb:
                pb.IsIndeterminate = true; window.UpdateLayout();
                pb.Classes.Add("item"); window.UpdateLayout();
                break;
            case Slider s:
                s.Orientation = Avalonia.Layout.Orientation.Vertical; window.UpdateLayout();
                s.TickPlacement = TickPlacement.Outside; window.UpdateLayout();
                break;
            case AutoCompleteBox acb:
                acb.Text = "a"; window.UpdateLayout();
                acb.IsDropDownOpen = true; window.UpdateLayout();
                acb.IsDropDownOpen = false; window.UpdateLayout();
                break;
            case Expander ex:
                ex.IsExpanded = true; window.UpdateLayout();
                ex.ExpandDirection = ExpandDirection.Left; window.UpdateLayout();
                break;
            case SplitView sv:
                sv.DisplayMode = SplitViewDisplayMode.Overlay; window.UpdateLayout();
                sv.PanePlacement = SplitViewPanePlacement.Right; window.UpdateLayout();
                sv.PanePlacement = SplitViewPanePlacement.Top; window.UpdateLayout();
                sv.PanePlacement = SplitViewPanePlacement.Bottom; window.UpdateLayout();
                break;
            case ScrollBar sb:
                sb.Orientation = Avalonia.Layout.Orientation.Horizontal; window.UpdateLayout();
                break;
            case TreeView { Items: [TreeViewItem tvi] }:
                tvi.IsExpanded = false; window.UpdateLayout();
                tvi.IsSelected = true; window.UpdateLayout();
                break;
            case PipsPager pp:
                pp.Orientation = Avalonia.Layout.Orientation.Vertical; window.UpdateLayout();
                break;
            case CalendarDatePicker cdp:
                cdp.IsDropDownOpen = true; window.UpdateLayout();
                cdp.IsDropDownOpen = false; window.UpdateLayout();
                break;
            case Calendar cal:
                cal.DisplayMode = CalendarMode.Year; window.UpdateLayout();
                cal.DisplayMode = CalendarMode.Decade; window.UpdateLayout();
                break;
            case CommandBar cbar:
                cbar.DefaultLabelPosition = CommandBarDefaultLabelPosition.Collapsed; window.UpdateLayout();
                cbar.DefaultLabelPosition = CommandBarDefaultLabelPosition.Right; window.UpdateLayout();
                cbar.IsOpen = true; window.UpdateLayout();
                cbar.IsOpen = false; window.UpdateLayout();
                break;
            case TextBox tb:
                tb.AcceptsReturn = true; window.UpdateLayout();
                tb.Classes.Add("rounded"); window.UpdateLayout();
                tb.Classes.Remove("rounded");
                tb.Classes.Add("tool"); window.UpdateLayout();
                tb.Classes.Remove("tool");
                tb.Classes.Add("search"); window.UpdateLayout();
                tb.Focus(); window.UpdateLayout();
                break;
            case DatePicker dp:
                dp.Classes.Add("x"); window.UpdateLayout();
                break;
        }
    }
}
