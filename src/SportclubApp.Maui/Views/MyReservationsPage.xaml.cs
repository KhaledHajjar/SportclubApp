using SportclubApp.Maui.ViewModels;

namespace SportclubApp.Maui.Views;

public partial class MyReservationsPage : ContentPage
{
    private readonly MyReservationsViewModel _viewModel;

    public MyReservationsPage(MyReservationsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel.ConfigureTitle();
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadCommand.Execute(null);
    }
}
