using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SportclubApp.Maui.Services.Api;
using SportclubApp.Maui.Services.Navigation;
using SportclubApp.Shared.Dtos;

namespace SportclubApp.Maui.ViewModels;

[QueryProperty(nameof(ClassId), "classId")]
public sealed partial class ClassDetailViewModel(
    ISportclubApi api,
    INavigationService navigation) : BaseViewModel
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
    [NotifyCanExecuteChangedFor(nameof(ReserveCommand))]
    private ClassSessionDto? _session;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanReserve))]
    [NotifyCanExecuteChangedFor(nameof(ReserveCommand))]
    private bool _hasActiveReservation;

    public bool HasSession => Session is not null;

    public bool IsFull => Session?.IsFull ?? false;

    public bool CanReserve => Session is not null && !Session.IsFull && !HasActiveReservation;

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
            await Task.WhenAll(sessionTask, reservationsTask);

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
}
