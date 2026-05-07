using SportclubApp.Maui.ViewModels;

namespace SportclubApp.Maui.Views;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _viewModel;

    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel.ConfigureTitle();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.TryAutoLoginAsync();
    }
}
