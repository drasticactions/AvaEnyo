using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using AvaEnyo.Samples;

namespace AvaEnyo.Photos.ViewModels;

/// <summary>The state of the Photos demo: albums, the open album and its select mode.</summary>
public sealed partial class PhotosViewModel : ObservableObject
{
    public PhotosViewModel()
    {
        Albums = SampleData.Albums.Select(a => new AlbumItem(a)).ToList();
        OpenAlbum(Albums[0]);
    }

    public IReadOnlyList<AlbumItem> Albums { get; }

    public ObservableCollection<PhotoItem> Photos { get; } = new();

    [ObservableProperty]
    public partial AlbumItem? CurrentAlbum { get; set; }

    [ObservableProperty]
    public partial bool IsSelecting { get; set; }

    [ObservableProperty]
    public partial int SelectedCount { get; set; }

    public string SelectionText => IsSelecting
        ? (SelectedCount == 1 ? "1 selected" : $"{SelectedCount} selected")
        : CurrentAlbum?.CountText ?? string.Empty;

    /// <summary>The full-size photo URIs of the open album, for the viewer.</summary>
    public List<Uri> PhotoUris { get; private set; } = new();

    public void OpenAlbum(AlbumItem album)
    {
        CurrentAlbum = album;
        Photos.Clear();
        var i = 0;
        foreach (var index in album.Album.Photos)
        {
            Photos.Add(new PhotoItem(index, i++));
        }

        PhotoUris = Photos.Select(p => p.Uri).ToList();
        IsSelecting = false;
        SelectedCount = 0;
        OnPropertyChanged(nameof(PhotoUris));
        OnPropertyChanged(nameof(SelectionText));
    }

    partial void OnIsSelectingChanged(bool value) => OnPropertyChanged(nameof(SelectionText));

    partial void OnSelectedCountChanged(int value) => OnPropertyChanged(nameof(SelectionText));
}
