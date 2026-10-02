namespace AvaEnyo;

/// <summary>Registers the class handlers the theme needs, once per process.</summary>
internal static class ThemeBehaviors
{
    private static bool s_registered;

    public static void EnsureRegistered()
    {
        if (s_registered)
        {
            return;
        }

        s_registered = true;
        Animations.AnimationRunner.EnsureInitialized();
        SliderGestures.Register();
        CheckBoxPress.Register();
        ToggleSwitchGestures.Register();
        EnyoBack.EnsureRegistered();
        Progress.Register();
        ExpanderMotion.Register();
        ListSelectorPlacement.Register();
        NavigationBack.Register();
    }
}
