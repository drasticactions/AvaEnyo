using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaEnyo.Mail.ViewModels;

/// <summary>The Preferences view: notifications, reading and account options.</summary>
public sealed partial class PreferencesViewModel : ObservableObject
{
    public static IReadOnlyList<string> SyncChoices { get; } = ["As items arrive", "Every 15 minutes", "Every hour", "Manually"];

    public static IReadOnlyList<string> SignatureChoices { get; } = ["None", "Sent from my TouchPad", "Cheers"];

    public static IReadOnlyList<int> MinuteChoices { get; } = [1, 2, 3, 4, 5, 6, 7, 8, 9];

    [ObservableProperty]
    public partial bool NewMailNotifications { get; set; } = true;

    [ObservableProperty]
    public partial bool ShowPreview { get; set; } = true;

    [ObservableProperty]
    public partial bool ConfirmDelete { get; set; } = true;

    [ObservableProperty]
    public partial bool DarkTheme { get; set; }

    [ObservableProperty]
    public partial int SyncIndex { get; set; }

    [ObservableProperty]
    public partial int SignatureIndex { get; set; } = 1;

    [ObservableProperty]
    public partial object? PreviewLines { get; set; } = 2;
}
