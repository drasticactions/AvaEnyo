using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace AvaEnyo;

/// <summary>
/// Makes a <see cref="CheckBox"/> toggle on press, as Enyo's check box toggled on mousedown, unless
/// <see cref="ThemeSettings.CheckBoxTogglesOnPress"/> is false or the app set <c>ClickMode</c> itself.
/// </summary>
internal static class CheckBoxPress
{
    public static void Register()
    {
        TemplatedControl.TemplateAppliedEvent.AddClassHandler<CheckBox>((box, _) => Apply(box));
    }

    private static void Apply(CheckBox box)
    {
        if (box.IsSet(Button.ClickModeProperty))
        {
            return;
        }

        box.SetValue(Button.ClickModeProperty, ThemeSettings.CheckBoxTogglesOnPress ? ClickMode.Press : ClickMode.Release, BindingPriority.Style);
    }
}
