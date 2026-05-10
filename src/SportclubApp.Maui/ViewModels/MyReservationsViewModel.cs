using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SportclubApp.Maui.Services.Api;
using SportclubApp.Maui.Services.Navigation;
using SportclubApp.Maui.Services.Notifications;
using SportclubApp.Shared.Dtos;
using SportclubApp.Shared.Enums;

namespace SportclubApp.Maui.ViewModels;

public sealed partial class MyReservationsViewModel(
    ISportclubApi api,
    INavigationService navigation,
    INotificationsBadgeService badge) : BaseViewModel
{
    public ObservableCollection<ReservationItemViewModel> Reservations { get; } = [];

    public MyReservationsViewModel ConfigureTitle()
    {
        Title = "My classes";
        return this;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        ClearError();
        try
        {
            var rows = await api.GetMyReservationsAsync();
            Reservations.Clear();
            foreach (var r in rows.OrderBy(x => x.ClassStartUtc))
            {
                Reservations.Add(new ReservationItemViewModel(r));
            }
        }
        catch (Exception)
        {
            ErrorMessage = "Could not load your classes. Pull down to retry.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task OpenAsync(ReservationItemViewModel item)
    {
        await navigation.GoToAsync("classDetail", new Dictionary<string, object> { ["classId"] = item.ClassSessionId });
    }

    [RelayCommand]
    private async Task CancelAsync(ReservationItemViewModel item)
    {
        var confirm = await navigation.DisplayConfirmAsync(
            "Cancel reservation",
            "Are you sure you want to cancel this reservation?",
            accept: "Cancel reservation",
            cancel: "Keep");

        if (!confirm)
        {
            return;
        }

        IsBusy = true;
        try
        {
            await api.CancelReservationAsync(item.Id);
            await LoadAsync();
            await badge.RefreshAsync();
        }
        catch (ApiException ex)
        {
            await navigation.DisplayAlertAsync("Cannot cancel", ReservationErrorMessages.ForCancel(ex));
        }
        catch (Exception)
        {
            await navigation.DisplayAlertAsync("Cannot cancel", "Could not reach the server.");
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public sealed class ReservationItemViewModel(ReservationDto dto)
{
    public Guid Id { get; } = dto.Id;
    public Guid ClassSessionId { get; } = dto.ClassSessionId;
    public DateTimeOffset ClassStartUtc { get; } = dto.ClassStartUtc;
    public ReservationStatus Status { get; } = dto.Status;

    public string StartText => ClassStartUtc.LocalDateTime.ToString("dddd, MMM d • HH:mm");

    public string StatusText => Status switch
    {
        ReservationStatus.Active => "Active",
        ReservationStatus.Cancelled => "Cancelled",
        ReservationStatus.Attended => "Attended",
        ReservationStatus.NoShow => "No-show",
        _ => Status.ToString(),
    };

    public bool IsActive => Status == ReservationStatus.Active;

    public bool CanCancel => IsActive && ClassStartUtc > DateTimeOffset.UtcNow.AddHours(1);
}
