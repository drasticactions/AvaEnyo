using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using AvaEnyo.Samples;
using AvaEnyo.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaEnyo.Mail.ViewModels;

/// <summary>
/// The mail app state: accounts and folders, the messages of the selected folder (filtered by the search text),
/// the open message, and the saved draft. Everything is read through the service bus, as an Enyo app read
/// <c>palm://</c> services.
/// </summary>
public sealed partial class MailViewModel : ObservableObject
{
    private readonly IServiceBus _bus;
    private CancellationTokenSource? _load;

    public MailViewModel(IServiceBus bus)
    {
        _bus = bus;
    }

    public ObservableCollection<AccountRow> Accounts { get; } = new();

    public ObservableCollection<MailMessage> Messages { get; } = new();

    public PreferencesViewModel Preferences { get; } = new();

    public IServiceBus Bus => _bus;

    [ObservableProperty]
    public partial FolderRow? SelectedFolder { get; set; }

    [ObservableProperty]
    public partial MailMessage? SelectedMessage { get; set; }

    [ObservableProperty]
    public partial string? SearchText { get; set; }

    [ObservableProperty]
    public partial bool IsMultiSelect { get; set; }

    [ObservableProperty]
    public partial Draft? SavedDraft { get; set; }

    public string FolderTitle => SelectedFolder is { } f ? $"{f.AccountName} · {f.Name}" : "Mail";

    public bool HasMessage => SelectedMessage is not null;

    public async Task LoadAsync()
    {
        var result = await _bus.CallAsync("mail/folders", null);
        Accounts.Clear();
        foreach (var a in result?["accounts"]?.AsArray() ?? new JsonArray())
        {
            var name = (string?)a?["name"] ?? string.Empty;
            var folders = (a?["folders"]?.AsArray() ?? new JsonArray())
                .Select(f => new FolderRow(name, (string?)f?["id"] ?? string.Empty, (string?)f?["name"] ?? string.Empty, (int?)f?["unread"] ?? 0, (int?)f?["total"] ?? 0))
                .ToList();
            Accounts.Add(new AccountRow(name, (string?)a?["address"] ?? string.Empty, folders));
        }

        SelectedFolder ??= Accounts.FirstOrDefault()?.Folders.FirstOrDefault();
        await LoadMessagesAsync();
    }

    public async Task LoadMessagesAsync()
    {
        if (SelectedFolder is not { } folder)
        {
            return;
        }

        _load?.Cancel();
        var cts = new CancellationTokenSource();
        _load = cts;
        var parameters = new JsonObject { ["folder"] = folder.Id, ["query"] = SearchText ?? string.Empty };
        var result = await _bus.CallAsync("mail/messages", parameters, cts.Token);
        if (cts.IsCancellationRequested)
        {
            return;
        }

        var ids = new HashSet<int>((result?["ids"]?.AsArray() ?? new JsonArray()).Select(n => (int?)n ?? -1));
        Messages.Clear();
        foreach (var m in SampleData.Messages.Where(m => ids.Contains(m.Id)))
        {
            Messages.Add(m);
        }
    }

    public async Task<IReadOnlyList<Contact>> SearchContactsAsync(string query)
    {
        var result = await _bus.CallAsync("contacts/search", new JsonObject { ["query"] = query });
        var ids = (result?["contacts"]?.AsArray() ?? new JsonArray()).Select(c => (int?)c?["id"] ?? -1).ToHashSet();
        return SampleData.Contacts.Where(c => ids.Contains(c.Id)).ToList();
    }

    public async Task<bool> SendAsync(Draft draft)
    {
        var to = new JsonArray();
        foreach (var t in draft.To.Concat(draft.Cc))
        {
            to.Add((JsonNode)JsonValue.Create(t is Contact c ? c.Email : t.ToString() ?? string.Empty));
        }

        var result = await _bus.CallAsync("mail/send", new JsonObject { ["to"] = to, ["subject"] = draft.Subject, ["body"] = draft.Body });
        return (bool?)result?["sent"] == true;
    }

    public void Delete(MailMessage message)
    {
        if (SelectedMessage == message)
        {
            SelectedMessage = null;
        }

        Messages.Remove(message);
    }

    public void MarkRead(MailMessage message)
    {
        if (!message.IsUnread)
        {
            return;
        }

        message.IsUnread = false;
        foreach (var folder in Accounts.SelectMany(a => a.Folders).Where(f => f.Id == message.FolderId && f.Unread > 0))
        {
            folder.Unread--;
        }
    }

    partial void OnSelectedFolderChanged(FolderRow? value)
    {
        OnPropertyChanged(nameof(FolderTitle));
        _ = LoadMessagesAsync();
    }

    partial void OnSearchTextChanged(string? value) => _ = LoadMessagesAsync();

    partial void OnSelectedMessageChanged(MailMessage? value)
    {
        OnPropertyChanged(nameof(HasMessage));
        if (value is not null)
        {
            MarkRead(value);
        }
    }
}
