using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaEnyo.Mail.ViewModels;

/// <summary>A folder as the folder list shows it.</summary>
public sealed partial class FolderRow : ObservableObject
{
    public FolderRow(string accountName, string id, string name, int unread, int total)
    {
        AccountName = accountName;
        Id = id;
        Name = name;
        Unread = unread;
        Total = total;
    }

    public string AccountName { get; }

    public string Id { get; }

    public string Name { get; }

    public int Total { get; }

    [ObservableProperty]
    public partial int Unread { get; set; }

    public bool HasUnread => Unread > 0;

    partial void OnUnreadChanged(int value) => OnPropertyChanged(nameof(HasUnread));
}
