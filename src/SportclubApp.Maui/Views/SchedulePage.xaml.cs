using SportclubApp.Maui.ViewModels;

namespace SportclubApp.Maui.Views;

public partial class SchedulePage : ContentPage
{
    private readonly ScheduleViewModel _viewModel;

    public SchedulePage(ScheduleViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel.ConfigureTitle();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel.Groups.Count == 0)
        {
            await _viewModel.LoadAsync();
        }
    }
}
