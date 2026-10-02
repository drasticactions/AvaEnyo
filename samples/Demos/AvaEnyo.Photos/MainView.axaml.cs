using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvaEnyo.Controls;
using AvaEnyo.Photos.ViewModels;
using AvaEnyo.Services;

namespace AvaEnyo.Photos;

/// <summary>
/// The Photos demo: albums, an album's photo grid with a select mode, a full-screen viewer that swipes between
/// photos and zooms, and cropping in a modal dialog. Back (Escape, the system back, the header button) walks crop →
/// viewer → grid → albums through the Pane history.
/// </summary>
public partial class MainView : UserControl
{
    private readonly PhotosViewModel _vm = new();
    private readonly DispatcherTimer _slideshow;
    private ApplicationEvents? _events;

    public MainView()
    {
        InitializeComponent();
        DataContext = _vm;
        Pages.Transition = Animations.EnyoPaneTransitions.FlyIn;

        _slideshow = new DispatcherTimer(TimeSpan.FromSeconds(3), DispatcherPriority.Normal, (_, _) => Viewer.Next());

        AlbumList.AddHandler(Item.ClickEvent, OnAlbumClick);
        PhotoGrid.AddHandler(Item.ClickEvent, OnPhotoClick);
        PhotoGrid.SelectionChanged += (_, _) => _vm.SelectedCount = PhotoGrid.SelectedItems?.Count ?? 0;
        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(PhotosViewModel.IsSelecting))
            {
                ApplySelectMode();
            }
        };

        SlideshowButton.Click += (_, _) => OpenViewer(0, slideshow: true);
        ViewerBack.Click += (_, _) => Pages.Back();
        ViewerSlideshow.Click += (_, _) => SetSlideshow(ViewerSlideshow.IsChecked);
        CropButton.Click += (_, _) => OpenCrop();
        Viewer.Tapped += (_, _) => ViewerToolbar.IsVisible = !ViewerToolbar.IsVisible;
        Viewer.SnapFinished += (_, _) => UpdateCaption();
        Pages.SelectionChanged += (_, e) =>
        {
            if (e.View != viewer)
            {
                SetSlideshow(false);
            }
        };

        CropDialog.Closed += OnCropClosed;
        ZoomSlider.PropertyChanged += (_, e) =>
        {
            if (e.Property == Avalonia.Controls.Primitives.RangeBase.ValueProperty && CropDialog.IsOpen)
            {
                Cropper.Zoom = ZoomSlider.Value;
            }
        };
        Menu.Closed += OnMenuClosed;
    }

    /// <summary>Opens a screen by name: albums, grid, viewer or crop (the <c>--view</c> switch).</summary>
    public void ShowView(string name)
    {
        switch (name.ToLowerInvariant())
        {
            case "grid":
                OpenAlbum(_vm.Albums[0], immediate: true);
                break;
            case "viewer":
                OpenAlbum(_vm.Albums[0], immediate: true);
                OpenViewer(3, slideshow: false, immediate: true);
                break;
            case "crop":
                OpenAlbum(_vm.Albums[0], immediate: true);
                OpenViewer(3, slideshow: false, immediate: true);
                Dispatcher.UIThread.Post(OpenCrop, DispatcherPriority.Background);
                break;
            default:
                Pages.SelectViewByName("albums", immediate: true);
                break;
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (TopLevel.GetTopLevel(this) is { } top)
        {
            AppMenuService.Attach(top, Menu);
            _events = ApplicationEvents.Attach(top);
            _events.Deactivated += OnDeactivated;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_events is not null)
        {
            _events.Deactivated -= OnDeactivated;
        }

        SetSlideshow(false);
        base.OnDetachedFromVisualTree(e);
    }

    private void OnDeactivated(object? sender, EventArgs e) => SetSlideshow(false);

    private void OnAlbumClick(object? sender, RoutedEventArgs e)
    {
        if ((e.Source as Control)?.DataContext is AlbumItem album)
        {
            OpenAlbum(album);
        }
    }

    private void OpenAlbum(AlbumItem album, bool immediate = false)
    {
        _vm.OpenAlbum(album);
        PhotoGrid.Reset();
        Pages.SelectViewByName("grid", immediate);
    }

    private void OnPhotoClick(object? sender, RoutedEventArgs e)
    {
        if (_vm.IsSelecting || (e.Source as Control)?.DataContext is not PhotoItem photo)
        {
            return;
        }

        OpenViewer(photo.Position, slideshow: false);
    }

    private void OpenViewer(int position, bool slideshow, bool immediate = false)
    {
        Viewer.Images = _vm.PhotoUris;
        Viewer.SelectedIndex = Math.Clamp(position, 0, Math.Max(0, _vm.Photos.Count - 1));
        ViewerToolbar.IsVisible = true;
        Pages.SelectViewByName("viewer", immediate);
        UpdateCaption();
        SetSlideshow(slideshow);
    }

    private void UpdateCaption() =>
        ViewerCaption.Text = _vm.Photos.Count == 0 ? string.Empty : $"{Viewer.ViewIndex + 1} of {_vm.Photos.Count}";

    private void SetSlideshow(bool on)
    {
        ViewerSlideshow.IsChecked = on;
        if (on)
        {
            _slideshow.Start();
        }
        else
        {
            _slideshow.Stop();
        }
    }

    private void ApplySelectMode()
    {
        PhotoGrid.SelectedItems?.Clear();
        PhotoGrid.SelectionMode = _vm.IsSelecting ? SelectionMode.Multiple | SelectionMode.Toggle : SelectionMode.Single;
        PhotoGrid.SelectedIndex = -1;
        _vm.SelectedCount = 0;
    }

    private void OpenCrop()
    {
        SetSlideshow(false);
        Cropper.Source = Viewer.CenterSource ?? Viewer.CurrentImage?.Source;
        CropDialog.OpenAtCenter();
        Dispatcher.UIThread.Post(() =>
        {
            ZoomSlider.Minimum = Cropper.MinZoom;
            ZoomSlider.Maximum = Math.Max(Cropper.MinZoom * 3, Cropper.MaxZoom);
            ZoomSlider.Value = Cropper.Zoom;
        }, DispatcherPriority.Background);
    }

    private void OnCropClosed(object? sender, PopupClosedEventArgs e)
    {
        if (e.Reason != PopupCloseReason.Handle || !Equals(e.Value, "done") || Cropper.Crop() is not { } cropped)
        {
            return;
        }

        ResultImage.Source = cropped;
        ResultText.Text = $"{cropped.PixelSize.Width} × {cropped.PixelSize.Height} pixels";
        ResultDialog.OpenAtCenter();
    }

    private void OnMenuClosed(object? sender, PopupClosedEventArgs e)
    {
        switch (e.Value as string)
        {
            case "slideshow":
                OpenViewer(Pages.SelectedView == viewer ? Viewer.ViewIndex : 0, slideshow: true);
                break;
            case "select":
                if (Pages.SelectedView != grid)
                {
                    Pages.SelectViewByName("grid");
                }

                _vm.IsSelecting = true;
                break;
            case "about":
                AboutDialog.OpenAtCenter();
                break;
        }
    }
}
