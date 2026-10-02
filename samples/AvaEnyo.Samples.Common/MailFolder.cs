namespace AvaEnyo.Samples;

/// <summary>A fake mail folder.</summary>
public sealed record MailFolder(string Id, string Name, int Unread, int Total)
{
    public bool HasUnread => Unread > 0;
}
