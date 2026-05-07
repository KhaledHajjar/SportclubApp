using SportclubApp.Maui.ViewModels;

namespace SportclubApp.Maui.Views;

public partial class HomePage : ContentPage
{
    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel.ConfigureTitle();
    }
}
