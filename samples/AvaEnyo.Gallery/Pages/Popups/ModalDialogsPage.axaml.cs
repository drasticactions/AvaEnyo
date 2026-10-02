using Avalonia.Controls;

namespace AvaEnyo.Gallery.Pages.Popups;

public partial class ModalDialogsPage : UserControl
{
    public ModalDialogsPage()
    {
        InitializeComponent();
        OpenDialogButton.Click += async (_, _) =>
        {
            var value = await SignInDialog.ShowAsync();
            DialogResult.Text = "Result: " + (value ?? "(closed)");
        };
        OpenPromptButton.Click += async (_, _) =>
        {
            var accepted = await DeletePrompt.ShowAsync();
            PromptResult.Text = "Result: " + (accepted ? "accepted" : "cancelled");
        };
    }
}
