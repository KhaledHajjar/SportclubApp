using NSubstitute;
using SportclubApp.Maui.Services.Api;
using SportclubApp.Maui.Services.Navigation;
using SportclubApp.Maui.Services.Notifications;
using SportclubApp.Maui.ViewModels;
using SportclubApp.Shared.Dtos;

namespace SportclubApp.Maui.Tests.ViewModels;

// Defends: "Tapping Reserve twice quickly must not create two reservations.
// The button should be disabled the moment the request goes out, and the user
// shouldn't be able to fire it again until the request resolves."
public sealed class ClassDetailViewModelReserveTests
{
    [Fact]
    public async Task ReserveCommand_is_disabled_while_request_is_in_flight()
    {
        var api = Substitute.For<ISportclubApi>();
        var navigation = Substitute.For<INavigationService>();
        var badge = Substitute.For<INotificationsBadgeService>();

        var session = CreateSession();
        var inFlight = new TaskCompletionSource<ReservationDto>();
        api.ReserveAsync(session.Id, Arg.Any<CancellationToken>())
            .Returns(_ => inFlight.Task);

        var sut = new ClassDetailViewModel(api, navigation, badge) { Session = session };

        Assert.True(sut.ReserveCommand.CanExecute(null));

        var execution = sut.ReserveCommand.ExecuteAsync(null);

        // The button must already be disabled before the user can tap again.
        Assert.False(sut.ReserveCommand.CanExecute(null));

        inFlight.SetResult(new ReservationDto(
            Id: Guid.NewGuid(),
            ClassSessionId: session.Id,
            ClassStartUtc: session.StartUtc,
            CreatedUtc: DateTimeOffset.UtcNow,
            Status: Shared.Enums.ReservationStatus.Active,
            WorkoutName: session.Workout.Name,
            LocationName: session.Location.Name));

        await execution;

        // Exactly one HTTP call regardless of how many spurious CanExecute hits we did.
        await api.Received(1).ReserveAsync(session.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReserveCommand_re_enables_for_retry_when_request_fails()
    {
        var api = Substitute.For<ISportclubApi>();
        var navigation = Substitute.For<INavigationService>();
        var badge = Substitute.For<INotificationsBadgeService>();

        var session = CreateSession();
        api.ReserveAsync(session.Id, Arg.Any<CancellationToken>())
            .Returns<Task<ReservationDto>>(_ => throw new ApiException(
                System.Net.HttpStatusCode.InternalServerError, null, "boom", null));

        var sut = new ClassDetailViewModel(api, navigation, badge) { Session = session };

        await sut.ReserveCommand.ExecuteAsync(null);

        Assert.False(sut.IsBusy);
        Assert.True(sut.ReserveCommand.CanExecute(null));
        Assert.True(sut.HasError);
    }

    private static ClassSessionDto CreateSession() => new(
        Id: Guid.NewGuid(),
        StartUtc: DateTimeOffset.UtcNow.AddHours(3),
        Capacity: 10,
        ReservedCount: 2,
        WaitingListCount: 0,
        FreeSpots: 8,
        IsFull: false,
        Workout: new WorkoutDto(Guid.NewGuid(), "Yoga", "Stretchy.", 60),
        Instructor: new InstructorDto(Guid.NewGuid(), "Diana", "Janssen", null),
        Location: new LocationDto(Guid.NewGuid(), "Studio A", null));
}
