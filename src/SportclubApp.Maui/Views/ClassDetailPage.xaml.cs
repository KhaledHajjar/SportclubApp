using SportclubApp.Maui.ViewModels;

namespace SportclubApp.Maui.Views;

public partial class ClassDetailPage : ContentPage
{
    private readonly ClassDetailViewModel _viewModel;

    public ClassDetailPage(ClassDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel.ConfigureTitle();
        BindingContext = _viewModel;
        _viewModel.ReservationConfirmed += OnReservationConfirmed;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (Content is { } content)
        {
            content.Opacity = 0;
            content.TranslationY = 8;
            _ = Task.WhenAll(
                content.FadeToAsync(1, 220, Easing.CubicOut),
                content.TranslateToAsync(0, 0, 220, Easing.CubicOut));
        }

        if (_viewModel.ClassId != Guid.Empty)
        {
            _viewModel.LoadCommand.Execute(null);
        }
    }

    private async void OnReservationConfirmed(object? sender, EventArgs e)
    {
        await PlaySuccessOverlayAsync();
    }

    private async Task PlaySuccessOverlayAsync()
    {
        SuccessOverlay.IsVisible = true;
        SuccessOverlay.Opacity = 0;
        SuccessIcon.Scale = 0;

        // Dim fades in while the check springs up to 1.1x.
        await Task.WhenAll(
            SuccessOverlay.FadeToAsync(1, 150, Easing.Linear),
            SuccessIcon.ScaleToAsync(1.1, 280, Easing.SpringOut));

        // Settle back to 1.0.
        await SuccessIcon.ScaleToAsync(1, 100, Easing.CubicOut);

        // Hold so the user reads it.
        await Task.Delay(650);

        // Fade out and reset.
        await SuccessOverlay.FadeToAsync(0, 220, Easing.CubicIn);
        SuccessOverlay.IsVisible = false;
    }
}
