using Android.App;
using Android.Content.PM;
using Avalonia.Android;

namespace AvaEnyo.Photos.Android;

[Activity(
    Label = "Photos",
    Theme = "@style/AvaEnyoTheme.NoActionBar",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
}
