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

        // Subtle entry animation: page content fades and lifts into place.
        if (Content is { } content)
        {
            content.Opacity = 0;
            content.TranslationY = 8;
            _ = Task.WhenAll(
                content.FadeToAsync(1, 220, Easing.CubicOut),
                content.TranslateToAsync(0, 0, 220, Easing.CubicOut));
        }

        await _viewModel.LoadAsync();
    }
}
