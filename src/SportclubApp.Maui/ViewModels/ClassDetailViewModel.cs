using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SportclubApp.Maui.Services.Api;
using SportclubApp.Maui.Services.Navigation;
using SportclubApp.Maui.Services.Notifications;
using SportclubApp.Shared.Dtos;

namespace SportclubApp.Maui.ViewModels;

[QueryProperty(nameof(ClassId), "classId")]
public sealed partial class ClassDetailViewModel(
    ISportclubApi api,
    INavigationService navigation,
    INotificationsBadgeService badge) : BaseViewModel
{
    [ObservableProperty]
    private Guid _classId;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSession))]
    [NotifyPropertyChangedFor(nameof(WorkoutName))]
    [NotifyPropertyChangedFor(nameof(WorkoutDescription))]
    [NotifyPropertyChangedFor(nameof(InstructorName))]
    [NotifyPropertyChangedFor(nameof(LocationName))]
    [NotifyPropertyChangedFor(nameof(StartText))]
    [NotifyPropertyChangedFor(nameof(CapacityText))]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyPropertyChangedFor(nameof(IsFull))]
    [NotifyPropertyChangedFor(nameof(CanReserve))]
    [NotifyPropertyChangedFor(nameof(CanJoinWaitingList))]
    [NotifyCanExecuteChangedFor(nameof(ReserveCommand))]
    [NotifyCanExecuteChangedFor(nameof(JoinWaitingListCommand))]
    private ClassSessionDto? _session;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanReserve))]
    [NotifyPropertyChangedFor(nameof(CanJoinWaitingList))]
    [NotifyCanExecuteChangedFor(nameof(ReserveCommand))]
    [NotifyCanExecuteChangedFor(nameof(JoinWaitingListCommand))]
    private bool _hasActiveReservation;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOnWaitingList))]
    [NotifyPropertyChangedFor(nameof(CanJoinWaitingList))]
    [NotifyCanExecuteChangedFor(nameof(JoinWaitingListCommand))]
    [NotifyCanExecuteChangedFor(nameof(LeaveWaitingListCommand))]
    private Guid? _waitingListEntryId;

    public bool HasSession => Session is not null;

    public bool IsFull => Session?.IsFull ?? false;

    public bool CanReserve => Session is not null && !Session.IsFull && !HasActiveReservation;

    public bool IsOnWaitingList => WaitingListEntryId.HasValue;

    public bool CanJoinWaitingList =>
        Session is { IsFull: true } && !HasActiveReservation && !IsOnWaitingList;

    public string WorkoutName => Session?.Workout.Name ?? string.Empty;

    public string WorkoutDescription => Session?.Workout.Description ?? string.Empty;

    public string InstructorName =>
        Session is null ? string.Empty : $"{Session.Instructor.FirstName} {Session.Instructor.LastName}";

    public string LocationName => Session?.Location.Name ?? string.Empty;

    public string StartText =>
        Session is null ? string.Empty : Session.StartUtc.LocalDateTime.ToString("dddd, MMM d • HH:mm");

    public string CapacityText =>
        Session is null ? string.Empty : $"{Session.ReservedCount} / {Session.Capacity} reserved";

    public string StatusText => Session switch
    {
        null => string.Empty,
        { IsFull: true } => $"Full — {Session.WaitingListCount} on waiting list",
        _ => $"{Session.FreeSpots} spots available",
    };

    partial void OnClassIdChanged(Guid value)
    {
        if (value != Guid.Empty)
        {
            _ = LoadAsync();
        }
    }

    public ClassDetailViewModel ConfigureTitle()
    {
        Title = "Class detail";
        return this;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (ClassId == Guid.Empty)
        {
            return;
        }

        IsBusy = true;
        ClearError();
        try
        {
            var sessionTask = api.GetClassAsync(ClassId);
            var reservationsTask = api.GetMyReservationsAsync();
            var waitingTask = api.GetMyWaitingListAsync();
            await Task.WhenAll(sessionTask, reservationsTask, waitingTask);

            Session = await sessionTask;
            if (Session is null)
            {
                ErrorMessage = "Class not found.";
                return;
            }

            var mine = await reservationsTask;
            HasActiveReservation = mine.Any(r =>
                r.ClassSessionId == ClassId
                && r.Status == Shared.Enums.ReservationStatus.Active);

            var entry = (await waitingTask).FirstOrDefault(w => w.ClassSessionId == ClassId);
            WaitingListEntryId = entry?.Id;
        }
        catch (Exception)
        {
            ErrorMessage = "Could not load this class. Pull down to retry.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanReserve))]
    private async Task ReserveAsync()
    {
        if (Session is null)
        {
            return;
        }

        IsBusy = true;
        ClearError();
        try
        {
            await api.ReserveAsync(Session.Id);
            HasActiveReservation = true;
            await LoadAsync();
            await badge.RefreshAsync();
            await navigation.DisplayAlertAsync("Reserved", "You're booked into this class.");
        }
        catch (ApiException ex)
        {
            ErrorMessage = ReservationErrorMessages.ForReserve(ex);
        }
        catch (Exception)
        {
            ErrorMessage = "Could not reach the server.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanJoinWaitingList))]
    private async Task JoinWaitingListAsync()
    {
        if (Session is null)
        {
            return;
        }

        IsBusy = true;
        ClearError();
        try
        {
            var entry = await api.JoinWaitingListAsync(Session.Id);
            WaitingListEntryId = entry.Id;
            await LoadAsync();
            await navigation.DisplayAlertAsync("On the list", "We'll notify you if a spot opens.");
        }
        catch (ApiException ex)
        {
            ErrorMessage = WaitingListErrorMessages.ForJoin(ex);
        }
        catch (Exception)
        {
            ErrorMessage = "Could not reach the server.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LeaveWaitingListAsync()
    {
        if (WaitingListEntryId is not { } entryId)
        {
            return;
        }

        IsBusy = true;
        ClearError();
        try
        {
            await api.LeaveWaitingListAsync(entryId);
            WaitingListEntryId = null;
            await LoadAsync();
        }
        catch (ApiException ex)
        {
            ErrorMessage = WaitingListErrorMessages.ForLeave(ex);
        }
        catch (Exception)
        {
            ErrorMessage = "Could not reach the server.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
