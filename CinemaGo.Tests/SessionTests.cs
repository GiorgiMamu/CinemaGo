using CinemaGo.Application.Common;
using CinemaGo.Application.Services;
using CinemaGo.Domain.Constants;
using CinemaGo.Domain.DTOs;
using CinemaGo.Domain.Entities;
using CinemaGo.Domain.Enums;
using CinemaGo.Domain.Exceptions;
using CinemaGo.Domain.Interfaces.External;
using CinemaGo.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
namespace CinemaGo.Tests;

public class SessionTimeRulesTests
{
    private static readonly DateTime T = new(2030, 1, 1, 18, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Overlapping_Sessions_Are_Detected()
        => Assert.True(SessionTimeRules.Overlaps(T, T.AddHours(2), T.AddHours(1), T.AddHours(3)));

    [Fact]
    public void BackToBack_Sessions_Do_Not_Overlap()
        => Assert.False(SessionTimeRules.Overlaps(T, T.AddHours(2), T.AddHours(2), T.AddHours(4)));

    [Fact]
    public void End_Includes_Duration_And_CleaningBuffer()
        => Assert.Equal(T.AddMinutes(120 + 15), SessionTimeRules.GetEnd(T, 120));
}

public class SessionServiceTests
{
    private readonly ISessionRepository _sessions = Substitute.For<ISessionRepository>();
    private readonly IMovieRepository _movies = Substitute.For<IMovieRepository>();
    private readonly IHallRepository _halls = Substitute.For<IHallRepository>();
    private readonly IBookingRepository _bookings = Substitute.For<IBookingRepository>();
    private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
    private readonly IEmailSender _email = Substitute.For<IEmailSender>();
    private readonly SessionService _sut;

    public SessionServiceTests()
    {
        _sut = new SessionService(_sessions, _movies, _halls, Substitute.For<ISeatRepository>(), _bookings,
            Substitute.For<IUnitOfWork>(), _identity, _email, TestData.Mapper(), NullLogger<SessionService>.Instance);
    }

    [Fact]
    public async Task Create_OverlappingSessionInSameHall_ThrowsConflict()
    {
        var start = DateTime.UtcNow.AddDays(2);
        _movies.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(new Movie { Id = 1, DurationMinutes = 120 });
        _halls.ExistsAsync(1, Arg.Any<CancellationToken>()).Returns(true);
        _sessions.GetByHallInWindowAsync(1, Arg.Any<DateTime>(), Arg.Any<DateTime>(), null, Arg.Any<CancellationToken>())
            .Returns(new List<Session> { TestData.NewSession(id: 9, startsAt: start.AddMinutes(30)) });

        var ex = await Assert.ThrowsAsync<ConflictException>(() => _sut.CreateAsync(
            new SessionRequest { MovieId = 1, HallId = 1, StartsAt = start, StandardPrice = 10, VipPrice = 20 }, TestContext.Current.CancellationToken));
        Assert.Equal(Errors.Session.Overlap.Code, ex.ErrorCode);
    }

    [Fact]
    public async Task Cancel_CancelsActiveBookingsAndEmailsEachCustomerOnce()
    {
        var session = TestData.NewSession();
        _sessions.GetWithDetailsAsync(1, Arg.Any<CancellationToken>()).Returns(session);
        var bookings = new List<Booking>
        {
            new() { Id = 1, UserId = "a" }, new() { Id = 2, UserId = "a" }, new() { Id = 3, UserId = "b" }
        };
        _bookings.GetActiveBySessionAsync(1, Arg.Any<CancellationToken>()).Returns(bookings);
        _identity.GetByIdsAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>()).Returns(new List<UserResponse>
        {
            new() { Id = "a", UserName = "A", Email = "a@x.com" }, new() { Id = "b", UserName = "B", Email = "b@x.com" }
        });

        await _sut.CancelAsync(1, TestContext.Current.CancellationToken);

        Assert.Equal(SessionStatus.Cancelled, session.Status);
        Assert.All(bookings, b => Assert.Equal(BookingStatus.Cancelled, b.Status));
        await _email.Received(2).SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}