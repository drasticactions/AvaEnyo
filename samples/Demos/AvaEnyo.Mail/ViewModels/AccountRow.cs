using System.Collections.Generic;

namespace AvaEnyo.Mail.ViewModels;

/// <summary>An account and its folders, one Group in the folder list.</summary>
public sealed record AccountRow(string Name, string Address, IReadOnlyList<FolderRow> Folders);
