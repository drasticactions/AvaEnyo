using Android.App;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;

namespace AvaEnyo.Gallery.Android;

[Application]
public class Application : AvaloniaAndroidApplication<App>
{
    protected Application(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
    {
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) =>
        base.CustomizeAppBuilder(builder).WithAvaEnyoFonts();
}
