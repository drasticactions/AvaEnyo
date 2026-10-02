using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media;

namespace AvaEnyo.Services;

/// <summary>
/// A webOS dashboard notification: an icon, a title and a line of text. A dashboard can stack layers (new mail
/// after new mail); the newest layer shows on top and a badge counts them.
/// </summary>
public sealed class Notification : INotifyPropertyChanged
{
    private IImage? _icon;
    private string? _title;
    private string? _text;
    private object? _tag;

    /// <summary>Initializes a new notification with a fresh <see cref="Id"/>.</summary>
    public Notification()
        : this(Guid.NewGuid().ToString("N"))
    {
    }

    /// <summary>Initializes a new notification with <paramref name="id"/>.</summary>
    public Notification(string id)
    {
        Id = id;
        Layers.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(LayerCount));
            OnPropertyChanged(nameof(DisplayTitle));
            OnPropertyChanged(nameof(DisplayText));
        };
    }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>The identity used by <see cref="INotificationService.Update"/> and <see cref="INotificationService.Dismiss"/>.</summary>
    public string Id { get; }

    /// <summary>The icon at the left of the dashboard.</summary>
    public IImage? Icon
    {
        get => _icon;
        set => Set(ref _icon, value);
    }

    /// <summary>The title, in white.</summary>
    public string? Title
    {
        get => _title;
        set
        {
            Set(ref _title, value);
            OnPropertyChanged(nameof(DisplayTitle));
        }
    }

    /// <summary>The text under the title.</summary>
    public string? Text
    {
        get => _text;
        set
        {
            Set(ref _text, value);
            OnPropertyChanged(nameof(DisplayText));
        }
    }

    /// <summary>App data carried with the notification.</summary>
    public object? Tag
    {
        get => _tag;
        set => Set(ref _tag, value);
    }

    /// <summary>The stacked layers, oldest first. The last one shows.</summary>
    public ObservableCollection<NotificationLayer> Layers { get; } = new();

    /// <summary>The number of layers counted by the badge: at least 1.</summary>
    public int LayerCount => Math.Max(1, Layers.Count);

    /// <summary>The title shown: the newest layer's, else <see cref="Title"/>.</summary>
    public string? DisplayTitle => Layers.Count > 0 ? Layers[^1].Title ?? Title : Title;

    /// <summary>The text shown: the newest layer's, else <see cref="Text"/>.</summary>
    public string? DisplayText => Layers.Count > 0 ? Layers[^1].Text ?? Text : Text;

    /// <summary>Adds a layer on top.</summary>
    public void AddLayer(string? title, string? text) => Layers.Add(new NotificationLayer(title, text));

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (!Equals(field, value))
        {
            field = value;
            OnPropertyChanged(name);
        }
    }

    private void OnPropertyChanged(string? name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
