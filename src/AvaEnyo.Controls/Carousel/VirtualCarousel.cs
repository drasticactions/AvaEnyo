using System;
using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvaEnyo.Controls;

/// <summary>
/// A <see cref="Carousel"/> over numbered views that reuses three view controls (<c>enyo.VirtualCarousel</c>).
/// <see cref="RenderViews"/> starts at an index; for every index the carousel calls <see cref="SetupView"/> on a reused
/// view, which returns false past either end. Or bind <see cref="ItemsSource"/> and <see cref="SelectedIndex"/>.
/// </summary>
public class VirtualCarousel : Carousel
{
    /// <summary>Defines the <see cref="ItemsSource"/> property.</summary>
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<VirtualCarousel, IEnumerable?>(nameof(ItemsSource));

    /// <summary>Defines the <see cref="SelectedIndex"/> property.</summary>
    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<VirtualCarousel, int>(nameof(SelectedIndex), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Defines the <see cref="SetupViewRequested"/> event.</summary>
    public static readonly RoutedEvent<SetupViewEventArgs> SetupViewRequestedEvent =
        RoutedEvent.Register<VirtualCarousel, SetupViewEventArgs>(nameof(SetupViewRequested), RoutingStrategies.Bubble);

    private readonly Control?[] _views = new Control?[3];
    private bool _settingIndex;

    static VirtualCarousel()
    {
        ItemsSourceProperty.Changed.AddClassHandler<VirtualCarousel>((c, _) => c.RenderViews(Math.Max(0, c.SelectedIndex)));
        SelectedIndexProperty.Changed.AddClassHandler<VirtualCarousel>((c, e) =>
        {
            if (!c._settingIndex)
            {
                c.RenderViews(e.GetNewValue<int>());
            }
        });
    }

    /// <summary>Items to show, one per view. Optional when <see cref="SetupViewRequested"/> is handled.</summary>
    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>The index of the view shown. Setting it jumps there.</summary>
    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    /// <summary>The index of the view shown (Enyo <c>viewIndex</c>).</summary>
    public int ViewIndex { get; private set; }

    /// <summary>The view control shown now (Enyo <c>fetchCurrentView</c>).</summary>
    public Control? CurrentView => CenterView as Control;

    /// <summary>Raised to fill a reused view for an index.</summary>
    public event EventHandler<SetupViewEventArgs>? SetupViewRequested
    {
        add => AddHandler(SetupViewRequestedEvent, value);
        remove => RemoveHandler(SetupViewRequestedEvent, value);
    }

    /// <summary>Shows the view at <paramref name="index"/> and fills its neighbors (Enyo <c>renderViews</c>).</summary>
    public void RenderViews(int index)
    {
        StopSnap();
        ViewIndex = index;
        for (var i = 0; i < 3; i++)
        {
            _views[i] ??= CreateView();
        }

        var showLeft = SetupView(_views[0]!, index - 1);
        var showCenter = SetupView(_views[1]!, index);
        var showRight = SetupView(_views[2]!, index + 1);
        SetSlots(_views[0], _views[1], _views[2], showLeft, showCenter, showRight);
        SetIndex(CenterIndex);
        SetSelectedSilently(index);
        Rest();
    }

    /// <summary>Creates one of the three reused views. The default is a content control with the content template.</summary>
    protected virtual Control CreateView() => new ContentControl
    {
        [!ContentControl.ContentTemplateProperty] = this[!ContentTemplateProperty],
        HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
        VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
    };

    /// <summary>
    /// Fills <paramref name="view"/> for <paramref name="index"/>. Returns false when there is no such view. The default
    /// raises <see cref="SetupViewRequested"/>, then falls back to <see cref="ItemsSource"/>.
    /// </summary>
    protected virtual bool SetupView(Control view, int index)
    {
        var args = new SetupViewEventArgs(SetupViewRequestedEvent, this, view, index);
        RaiseEvent(args);
        if (args.Handled)
        {
            return args.Accepted;
        }

        if (ItemAt(index) is { } found)
        {
            ApplyItem(view, found.Item);
            return true;
        }

        return false;
    }

    /// <summary>Puts an item from <see cref="ItemsSource"/> into a view.</summary>
    protected virtual void ApplyItem(Control view, object? item)
    {
        if (view is ContentControl cc)
        {
            cc.Content = item;
        }
    }

    /// <summary>The item at <paramref name="index"/> of <see cref="ItemsSource"/>, or null when out of range.</summary>
    protected (object? Item, bool Found)? ItemAt(int index)
    {
        if (index < 0 || ItemsSource is null)
        {
            return null;
        }

        if (ItemsSource is IList list)
        {
            return index < list.Count ? (list[index], true) : null;
        }

        var i = 0;
        foreach (var item in ItemsSource)
        {
            if (i++ == index)
            {
                return (item, true);
            }
        }

        return null;
    }

    /// <inheritdoc/>
    protected override void AdjustViews(int oldIndex, int newIndex)
    {
        var goRight = newIndex > oldIndex;
        ViewIndex += goRight ? 1 : -1;
        if (newIndex != CenterIndex)
        {
            var left = Slots[0] as Control;
            var center = Slots[1] as Control;
            var right = Slots[2] as Control;
            var far = goRight ? left : right;
            if (far is not null && SetupView(far, ViewIndex + (goRight ? 1 : -1)))
            {
                if (goRight)
                {
                    SetSlots(center, right, far, true, true, true);
                }
                else
                {
                    SetSlots(far, left, center, true, true, true);
                }

                SetIndex(CenterIndex);
            }
        }

        SetSelectedSilently(ViewIndex);
    }

    private void SetSelectedSilently(int index)
    {
        _settingIndex = true;
        try
        {
            SetCurrentValue(SelectedIndexProperty, index);
        }
        finally
        {
            _settingIndex = false;
        }
    }
}
