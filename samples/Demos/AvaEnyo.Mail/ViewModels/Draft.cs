using System.Collections.Generic;

namespace AvaEnyo.Mail.ViewModels;

/// <summary>An unsent message, kept in memory when the app is deactivated.</summary>
public sealed record Draft(IReadOnlyList<object> To, IReadOnlyList<object> Cc, string? Subject, string? Body)
{
    public bool IsEmpty => To.Count == 0 && Cc.Count == 0 && string.IsNullOrEmpty(Subject) && string.IsNullOrEmpty(Body);
}
