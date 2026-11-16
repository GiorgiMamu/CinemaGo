using CinemaGo.Application.Interfaces;
using CinemaGo.Application.Services;
using CinemaGo.Domain.DTOs;
using CinemaGo.Domain.Entities;
using CinemaGo.Domain.Enums;
using CinemaGo.Domain.Exceptions;
using CinemaGo.Domain.Interfaces.External;
using CinemaGo.Domain.Interfaces.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
namespace CinemaGo.Tests;

public class CancellationAndMaintenanceTests
{
    [Fact]
    public async Task CancellingABooking_AlsoCancelsItsTicket()
    {
        var bookings = Substitute.For<IBookingRepository>();
        var user = Substitute.For<ICurrentUserService>();
        user.UserId.Returns("u1");
        var booking = new Booking { Id = 5, UserId = "u1", Seat = TestData.NewSeat(1), Session = TestData.NewSession(), Ticket = new Ticket() };
        bookings.GetWithDetailsAsync(5, Arg.Any<CancellationToken>()).Returns(booking);
        var sut = new BookingService(bookings, Substitute.For<ISessionRepository>(), Substitute.For<ISeatRepository>(),
    Substitute.For<IUnitOfWork>(), user, TestData.Mapper(), NullLogger<BookingService>.Instance,
    Substitute.For<IIdentityService>(), Substitute.For<IEmailSender>());

        await sut.CancelAsync(5, TestContext.Current.CancellationToken);

        Assert.Equal(TicketStatus.Cancelled, booking.Ticket!.Status);
    }

    [Fact]
    public async Task CancellingASession_KeepsIt_SetsCancelledAt_AndCancelsTickets()
    {
        var sessions = Substitute.For<ISessionRepository>();
        var bookings = Substitute.For<IBookingRepository>();
        var identity = Substitute.For<IIdentityService>();
        identity.GetByIdsAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>()).Returns(new List<UserResponse>());
        var session = TestData.NewSession();
        sessions.GetWithDetailsAsync(1, Arg.Any<CancellationToken>()).Returns(session);
        var booking = new Booking { Id = 1, UserId = "a", Ticket = new Ticket() };
        bookings.GetActiveBySessionAsync(1, Arg.Any<CancellationToken>()).Returns(new List<Booking> { booking });
        var sut = new SessionService(sessions, Substitute.For<IMovieRepository>(), Substitute.For<IHallRepository>(),
            Substitute.For<ISeatRepository>(), bookings, Substitute.For<IUnitOfWork>(), identity,
            Substitute.For<IEmailSender>(), TestData.Mapper(), NullLogger<SessionService>.Instance);

        await sut.CancelAsync(1, TestContext.Current.CancellationToken);

        Assert.Equal(SessionStatus.Cancelled, session.Status);
        Assert.NotNull(session.CancelledAt);
        Assert.Equal(TicketStatus.Cancelled, booking.Ticket!.Status);
    }

    [Fact]
    public async Task FinishPastSessions_FinishesOnlySessionsThatHaveEnded()
    {
        var sessions = Substitute.For<ISessionRepository>();
        var ended = TestData.NewSession(id: 1, startsAt: DateTime.UtcNow.AddHours(-3), duration: 120);
        var running = TestData.NewSession(id: 2, startsAt: DateTime.UtcNow.AddMinutes(-30), duration: 120);
        sessions.GetStartedScheduledAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(new List<Session> { ended, running });
        var sut = new SessionService(sessions, Substitute.For<IMovieRepository>(), Substitute.For<IHallRepository>(),
            Substitute.For<ISeatRepository>(), Substitute.For<IBookingRepository>(), Substitute.For<IUnitOfWork>(),
            Substitute.For<IIdentityService>(), Substitute.For<IEmailSender>(), TestData.Mapper(), NullLogger<SessionService>.Instance);

        var count = await sut.FinishPastSessionsAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, count);
        Assert.Equal(SessionStatus.Finished, ended.Status);
        Assert.Equal(SessionStatus.Scheduled, running.Status);
    }

    [Fact]
    public async Task DeletingAPosterThatDoesNotExist_ThrowsNotFound()
    {
        var movies = Substitute.For<IMovieRepository>();
        movies.GetWithGenresAsync(5, Arg.Any<CancellationToken>()).Returns(new Movie { Id = 5, PosterPath = null });
        var sut = new MovieService(movies, Substitute.For<IGenreRepository>(), Substitute.For<IFileStorage>(),
            Substitute.For<IValidator<FileUpload>>(), Substitute.For<IUnitOfWork>(), TestData.Mapper(), NullLogger<MovieService>.Instance);

        await Assert.ThrowsAsync<NotFoundException>(() => sut.DeletePosterAsync(5, TestContext.Current.CancellationToken));
    }

}