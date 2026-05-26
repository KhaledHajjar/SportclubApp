using SportclubApp.Maui.ViewModels;

namespace SportclubApp.Maui.Views;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(RegisterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel.ConfigureTitle();
    }
}
