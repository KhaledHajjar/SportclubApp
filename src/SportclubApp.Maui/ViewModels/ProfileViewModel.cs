using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SportclubApp.Maui.Services.Api;
using SportclubApp.Maui.Services.Media;
using SportclubApp.Maui.Services.Navigation;
using SportclubApp.Shared.Dtos;
using SportclubApp.Shared.Enums;

namespace SportclubApp.Maui.ViewModels;

public sealed partial class ProfileViewModel(
    ISportclubApi api,
    IMediaPickerService mediaPicker,
    INavigationService navigation) : BaseViewModel
{
    [ObservableProperty]
    private string _firstName = string.Empty;

    [ObservableProperty]
    private string _lastName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPhoto))]
    [NotifyPropertyChangedFor(nameof(InitialsFallback))]
    private string? _profilePhotoUrl;

    [ObservableProperty]
    private DateTime _dateOfBirth = DateTime.UtcNow.AddYears(-25);

    [ObservableProperty]
    private bool _hasDateOfBirth;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSubscription))]
    [NotifyPropertyChangedFor(nameof(SubscriptionTypeText))]
    [NotifyPropertyChangedFor(nameof(SubscriptionEndsText))]
    [NotifyPropertyChangedFor(nameof(RemainingVisitsText))]
    [NotifyPropertyChangedFor(nameof(HasRemainingVisits))]
    private SubscriptionDto? _subscription;

    public bool HasPhoto => !string.IsNullOrWhiteSpace(ProfilePhotoUrl);

    public string InitialsFallback => string.Concat(
        FirstName.Length > 0 ? FirstName[0].ToString() : string.Empty,
        LastName.Length > 0 ? LastName[0].ToString() : string.Empty).ToUpperInvariant();

    public bool HasSubscription => Subscription is not null;

    public string SubscriptionTypeText => Subscription?.Type switch
    {
        SubscriptionType.TwicePerWeek => "Twice per week",
        SubscriptionType.Yearly => "Yearly",
        SubscriptionType.Unlimited => "Unlimited",
        _ => "—",
    };

    public string SubscriptionEndsText =>
        Subscription is null ? "—" : $"Ends {Subscription.EndUtc.LocalDateTime:d}";

    public string RemainingVisitsText =>
        Subscription?.RemainingWeeklyVisits is { } remaining
            ? $"{remaining} of 2 visits left this week"
            : string.Empty;

    public bool HasRemainingVisits => Subscription?.RemainingWeeklyVisits is not null;

    public ProfileViewModel ConfigureTitle()
    {
        Title = "Profile";
        return this;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        ClearError();
        try
        {
            var meTask = api.GetMeAsync();
            var subTask = api.GetMySubscriptionAsync();
            await Task.WhenAll(meTask, subTask);

            ApplyMember(await meTask);
            Subscription = await subTask;
        }
        catch (Exception)
        {
            ErrorMessage = "Could not load your profile. Pull down to retry.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
        {
            ErrorMessage = "First and last name are required.";
            return;
        }

        IsBusy = true;
        ClearError();
        try
        {
            DateOnly? dob = HasDateOfBirth ? DateOnly.FromDateTime(DateOfBirth) : null;
            var updated = await api.UpdateMeAsync(new UpdateMemberRequest(FirstName.Trim(), LastName.Trim(), dob));
            ApplyMember(updated);
            await navigation.DisplayAlertAsync("Profile saved", "Your changes have been saved.");
        }
        catch (Exception)
        {
            ErrorMessage = "Could not save your profile. Please try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ChangePhotoAsync()
    {
        var pickFromCamera = await navigation.DisplayConfirmAsync(
            "Change profile photo",
            "Choose a source.",
            accept: "Camera",
            cancel: "Gallery");

        var source = pickFromCamera ? PhotoSource.Camera : PhotoSource.Gallery;
        await using var pick = await mediaPicker.PickAsync(source);
        if (pick is null)
        {
            return;
        }

        IsBusy = true;
        ClearError();
        try
        {
            var updated = await api.UploadProfilePhotoAsync(pick.Content, pick.FileName, pick.ContentType);
            ApplyMember(updated);
        }
        catch (Exception)
        {
            ErrorMessage = "Could not upload the photo. Please try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyMember(MemberDto member)
    {
        FirstName = member.FirstName;
        LastName = member.LastName;
        Email = member.Email;
        HasDateOfBirth = member.DateOfBirth is not null;
        if (member.DateOfBirth is { } dob)
        {
            DateOfBirth = dob.ToDateTime(TimeOnly.MinValue);
        }
        ProfilePhotoUrl = string.IsNullOrWhiteSpace(member.ProfilePhotoUrl)
            ? null
            : new Uri(new Uri(AppConstants.ApiBaseUrl), member.ProfilePhotoUrl).ToString();
    }
}
