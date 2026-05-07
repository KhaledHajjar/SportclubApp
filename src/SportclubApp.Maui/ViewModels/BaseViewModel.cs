using CommunityToolkit.Mvvm.ComponentModel;

namespace SportclubApp.Maui.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    public bool IsNotBusy => !IsBusy;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    protected void ClearError() => ErrorMessage = null;
}
