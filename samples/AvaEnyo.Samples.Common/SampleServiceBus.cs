using System;
using System.Linq;
using System.Text.Json.Nodes;
using AvaEnyo.Services;

namespace AvaEnyo.Samples;

/// <summary>
/// A <see cref="MockServiceBus"/> preloaded with the demo routes, the way Enyo apps used <c>MockPalmService</c> in a
/// browser:
/// <list type="bullet">
/// <item><c>mail/folders</c>: <c>{ accounts: [{ name, address, folders: [{ id, name, unread, total }] }] }</c></item>
/// <item><c>mail/messages</c> with <c>{ folder, query? }</c>: <c>{ ids: [..] }</c>, newest first</item>
/// <item><c>mail/send</c> with <c>{ to, subject, body }</c>: <c>{ sent: true, id }</c> after <see cref="SendDelay"/></item>
/// <item><c>contacts/search</c> with <c>{ query }</c>: <c>{ contacts: [{ id, name, email }] }</c></item>
/// </list>
/// The JSON is built by hand, so nothing uses reflection serialization.
/// </summary>
public static class SampleServiceBus
{
    /// <summary>How long <c>mail/send</c> takes. Default 1.5 s.</summary>
    public static TimeSpan SendDelay { get; set; } = TimeSpan.FromMilliseconds(1500);

    public static MockServiceBus Create()
    {
        var bus = new MockServiceBus();
        bus.Register("mail/folders", _ => Folders());
        bus.Register("mail/messages", Messages);
        bus.RegisterDelayed("mail/send", SendDelay, p => new JsonObject
        {
            ["sent"] = true,
            ["id"] = Guid.NewGuid().ToString("N"),
            ["to"] = p?["to"]?.DeepClone(),
        });
        bus.Register("contacts/search", Contacts);
        return bus;
    }

    private static JsonNode Folders()
    {
        var accounts = new JsonArray();
        foreach (var a in SampleData.Accounts)
        {
            var folders = new JsonArray();
            foreach (var f in a.Folders)
            {
                folders.Add((JsonNode)new JsonObject { ["id"] = f.Id, ["name"] = f.Name, ["unread"] = f.Unread, ["total"] = f.Total });
            }

            accounts.Add((JsonNode)new JsonObject { ["name"] = a.Name, ["address"] = a.Address, ["folders"] = folders });
        }

        return new JsonObject { ["accounts"] = accounts };
    }

    private static JsonNode Messages(JsonNode? p)
    {
        var folder = (string?)p?["folder"] ?? "inbox";
        var query = ((string?)p?["query"])?.Trim();
        var ids = new JsonArray();
        foreach (var m in SampleData.MessagesIn(folder))
        {
            if (string.IsNullOrEmpty(query)
                || m.Subject.Contains(query, StringComparison.OrdinalIgnoreCase)
                || m.From.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                || m.Body.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                ids.Add((JsonNode)JsonValue.Create(m.Id));
            }
        }

        return new JsonObject { ["ids"] = ids };
    }

    private static JsonNode Contacts(JsonNode? p)
    {
        var query = ((string?)p?["query"])?.Trim() ?? string.Empty;
        var list = new JsonArray();
        foreach (var c in SampleData.Contacts.Where(c => query.Length > 0
            && (c.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || c.Email.Contains(query, StringComparison.OrdinalIgnoreCase))).Take(8))
        {
            list.Add((JsonNode)new JsonObject { ["id"] = c.Id, ["name"] = c.Name, ["email"] = c.Email });
        }

        return new JsonObject { ["contacts"] = list };
    }
}
