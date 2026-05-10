using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SportclubApp.Maui.Services;
using SportclubApp.Maui.Services.Api;
using SportclubApp.Maui.Services.Auth;
using SportclubApp.Maui.Services.Media;
using SportclubApp.Maui.Services.Navigation;
using SportclubApp.Maui.Services.Notifications;
using SportclubApp.Shared.Auth;
using SportclubApp.Shared.Dtos;
using SportclubApp.Shared.Enums;

namespace SportclubApp.Maui.ViewModels;

public sealed partial class ProfileViewModel(
    ISportclubApi api,
    IMediaPickerService mediaPicker,
    INavigationService navigation,
    ISecureTokenStore tokenStore,
    ISubscriptionExpiryScheduler expiryScheduler) : BaseViewModel
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InitialsFallback))]
    private string _firstName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InitialsFallback))]
    private string _lastName = string.Empty;

    [ObservableProperty]
    private bool _hasPhoto;

    [ObservableProperty]
    private ImageSource? _profilePhoto;

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

            await ApplyMemberAsync(await meTask);
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
            await ApplyMemberAsync(updated);
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
            await ApplyMemberAsync(updated);
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

    [RelayCommand]
    private async Task SignOutAsync()
    {
        IsBusy = true;
        try
        {
            var refreshToken = await tokenStore.GetRefreshTokenAsync();
            if (!string.IsNullOrEmpty(refreshToken))
            {
                try
                {
                    await api.LogoutAsync(new LogoutRequest(refreshToken));
                }
                catch
                {
                    // Ignore network errors on logout — clear local tokens regardless.
                }
            }
        }
        finally
        {
            await tokenStore.ClearAsync();
            UserContext.Current.Clear();
            await expiryScheduler.CancelAsync();
            IsBusy = false;
            await navigation.GoToAsync("//login");
        }
    }

    private async Task ApplyMemberAsync(MemberDto member)
    {
        FirstName = member.FirstName;
        LastName = member.LastName;
        Email = member.Email;
        HasDateOfBirth = member.DateOfBirth is not null;
        if (member.DateOfBirth is { } dob)
        {
            DateOfBirth = dob.ToDateTime(TimeOnly.MinValue);
        }

        HasPhoto = member.HasPhoto;
        if (!member.HasPhoto)
        {
            ProfilePhoto = null;
            return;
        }

        try
        {
            await using var stream = await api.GetMyPhotoAsync();
            if (stream is null)
            {
                HasPhoto = false;
                ProfilePhoto = null;
                return;
            }

            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory);
            var bytes = memory.ToArray();
            ProfilePhoto = ImageSource.FromStream(() => new MemoryStream(bytes));
        }
        catch
        {
            HasPhoto = false;
            ProfilePhoto = null;
        }
    }
}
