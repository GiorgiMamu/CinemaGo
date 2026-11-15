using CinemaGo.Application.Interfaces;
using CinemaGo.Application.Services;
using CinemaGo.Domain.DTOs;
using CinemaGo.Domain.Entities;
using CinemaGo.Domain.Enums;
using CinemaGo.Domain.Exceptions;
using CinemaGo.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
namespace CinemaGo.Tests;

public class BookingServiceTests
{
    private readonly IBookingRepository _bookings = Substitute.For<IBookingRepository>();
    private readonly ISessionRepository _sessions = Substitute.For<ISessionRepository>();
    private readonly ISeatRepository _seats = Substitute.For<ISeatRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _user = Substitute.For<ICurrentUserService>();
    private readonly List<Booking> _added = new();
    private readonly BookingService _sut;

    public BookingServiceTests()
    {
        _user.UserId.Returns("user-1");
        _bookings.GetActiveSeatIdsAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(new List<int>());
        _bookings.When(b => b.AddAsync(Arg.Any<Booking>(), Arg.Any<CancellationToken>()))
                 .Do(ci => _added.Add(ci.Arg<Booking>()));
        _sut = new BookingService(_bookings, _sessions, _seats, _uow, _user, TestData.Mapper(),
            NullLogger<BookingService>.Instance);
    }

    private void Arrange(Session session, params Seat[] seats)
    {
        _sessions.GetWithDetailsAsync(session.Id, Arg.Any<CancellationToken>()).Returns(session);
        _seats.GetByIdsAsync(Arg.Any<IEnumerable<int>>(), Arg.Any<CancellationToken>()).Returns(seats.ToList());
    }

    [Fact]
    public async Task Create_VipAndStandardSeats_UseTheirOwnPrices()
    {
        Arrange(TestData.NewSession(), TestData.NewSeat(1, vip: true), TestData.NewSeat(2));

        await _sut.CreateAsync(new CreateBookingRequest { SessionId = 1, SeatIds = { 1, 2 } });

        Assert.Equal(25m, _added.Single(b => b.Seat.Id == 1).Ticket!.FinalPrice);
        Assert.Equal(10m, _added.Single(b => b.Seat.Id == 2).Ticket!.FinalPrice);
    }

    [Fact]
    public async Task Create_MultipleSeats_CreatesOneTicketPerBookingWithUniqueCodes()
    {
        Arrange(TestData.NewSession(), TestData.NewSeat(1), TestData.NewSeat(2), TestData.NewSeat(3));

        var result = await _sut.CreateAsync(new CreateBookingRequest { SessionId = 1, SeatIds = { 1, 2, 3 } });

        Assert.Equal(3, result.Count);
        Assert.Equal(3, _added.Select(b => b.Ticket!.TicketCode).Distinct().Count());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());   // one transaction
    }

    [Fact]
    public async Task Create_SeatAlreadyBooked_ThrowsConflict()
    {
        Arrange(TestData.NewSession(), TestData.NewSeat(1));
        _bookings.GetActiveSeatIdsAsync(1, Arg.Any<CancellationToken>()).Returns(new List<int> { 1 });

        await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.CreateAsync(new CreateBookingRequest { SessionId = 1, SeatIds = { 1 } }));
    }

    [Fact]
    public async Task Create_SeatFromAnotherHall_ThrowsBadRequest()
    {
        Arrange(TestData.NewSession(hallId: 1), TestData.NewSeat(61, hallId: 2));

        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.CreateAsync(new CreateBookingRequest { SessionId = 1, SeatIds = { 61 } }));
    }

    [Fact]
    public async Task Create_SessionAlreadyStarted_ThrowsConflict()
    {
        Arrange(TestData.NewSession(startsAt: DateTime.UtcNow.AddMinutes(-5)), TestData.NewSeat(1));

        await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.CreateAsync(new CreateBookingRequest { SessionId = 1, SeatIds = { 1 } }));
    }

    [Fact]
    public async Task Create_ConcurrentDuplicateOnSave_ThrowsSeatConflict()
    {
        Arrange(TestData.NewSession(), TestData.NewSeat(1));
        _uow.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns<Task<int>>(_ => throw new DuplicateKeyException("duplicate"));

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.CreateAsync(new CreateBookingRequest { SessionId = 1, SeatIds = { 1 } }));
        Assert.Equal("seat_taken", ex.ErrorCode);
    }

    [Fact]
    public async Task Cancel_SomeoneElsesBooking_ThrowsForbidden()
    {
        _bookings.GetWithDetailsAsync(5, Arg.Any<CancellationToken>()).Returns(
            new Booking { Id = 5, UserId = "other-user", Session = TestData.NewSession() });

        await Assert.ThrowsAsync<ForbiddenException>(() => _sut.CancelAsync(5));
    }

    [Fact]
    public async Task Cancel_AlreadyCancelled_ThrowsConflict()
    {
        _bookings.GetWithDetailsAsync(5, Arg.Any<CancellationToken>()).Returns(
            new Booking { Id = 5, UserId = "user-1", Status = BookingStatus.Cancelled, Session = TestData.NewSession() });

        await Assert.ThrowsAsync<ConflictException>(() => _sut.CancelAsync(5));
    }

    [Fact]
    public async Task Cancel_OwnActiveBooking_SetsStatusAndTimestamp()
    {
        var booking = new Booking { Id = 5, UserId = "user-1", Session = TestData.NewSession() };
        _bookings.GetWithDetailsAsync(5, Arg.Any<CancellationToken>()).Returns(booking);

        await _sut.CancelAsync(5);

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        Assert.NotNull(booking.CancelledAt);
    }
}