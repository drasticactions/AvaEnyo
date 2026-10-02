using Android.App;
using Android.Content.PM;
using Avalonia.Android;

namespace AvaEnyo.Gallery.Android;

[Activity(
    Label = "AvaEnyo Gallery",
    Theme = "@style/AvaEnyoTheme.NoActionBar",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
}
