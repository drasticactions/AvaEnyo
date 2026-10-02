using Android.App;
using Android.Content.PM;
using Avalonia.Android;

namespace AvaEnyo.Mail.Android;

[Activity(
    Label = "Mail",
    Theme = "@style/AvaEnyoTheme.NoActionBar",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
}
