using Avalonia.Controls;
using AvaEnyo.Controls;

namespace AvaEnyo.Gallery.Pages.Widgets;

public partial class NumericKeypadPage : UserControl
{
    public NumericKeypadPage()
    {
        InitializeComponent();
        Phone.KeyPressed += (_, e) => PhoneDisplay.Text = (PhoneDisplay.Text ?? string.Empty).Trim() + e.Key;
        Pin.KeyPressed += (_, e) =>
        {
            var text = (PinDisplay.Text ?? string.Empty).Trim();
            if (e.Key == NumericKeypad.BackspaceKey)
            {
                PinDisplay.Text = text.Length > 0 ? text[..^1] : " ";
            }
            else if (text.Length < 4)
            {
                PinDisplay.Text = text + "●";
            }
        };
    }
}
