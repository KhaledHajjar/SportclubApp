using SportclubApp.Maui.ViewModels;

namespace SportclubApp.Maui.Views;

public partial class ClassDetailPage : ContentPage
{
    public ClassDetailPage(ClassDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel.ConfigureTitle();
    }
}
