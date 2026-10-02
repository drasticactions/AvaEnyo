using Avalonia.Controls;

namespace AvaEnyo.Gallery.Pages.StockControls;

public partial class ValidationPage : UserControl
{
    public ValidationPage()
    {
        InitializeComponent();
        DataValidationErrors.SetError(Email, new System.Exception("Enter a valid email address"));
    }
}
