using System.Collections.Generic;

namespace AvaEnyo.Samples;

/// <summary>A fake mail account with its folders.</summary>
public sealed record MailAccount(string Name, string Address, IReadOnlyList<MailFolder> Folders);
