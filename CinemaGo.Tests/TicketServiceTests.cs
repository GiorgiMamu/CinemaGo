using CinemaGo.Application.Interfaces;
using CinemaGo.Application.Services;
using CinemaGo.Domain.Constants;
using CinemaGo.Domain.Entities;
using CinemaGo.Domain.Enums;
using CinemaGo.Domain.Exceptions;
using CinemaGo.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
namespace CinemaGo.Tests;

public class TicketServiceTests
{
    private readonly ITicketRepository _tickets = Substitute.For<ITicketRepository>();
    private readonly TicketService _sut;

    public TicketServiceTests()
        => _sut = new TicketService(_tickets, Substitute.For<ICurrentUserService>(), Substitute.For<IUnitOfWork>(),
            TestData.Mapper(), NullLogger<TicketService>.Instance);

    private Ticket Arrange(TicketStatus status, DateTime startsAt)
    {
        var ticket = new Ticket
        {
            Id = 1,
            TicketCode = "CODE",
            Status = status,
            Booking = new Booking { UserId = "u", Seat = TestData.NewSeat(1), Session = TestData.NewSession(startsAt: startsAt) }
        };
        _tickets.GetByCodeTrackedAsync("CODE", Arg.Any<CancellationToken>()).Returns(ticket);
        return ticket;
    }

    [Fact]
    public async Task CheckIn_InsideTheWindow_MarksTicketUsed()
    {
        var ticket = Arrange(TicketStatus.Valid, DateTime.UtcNow.AddMinutes(30));
        await _sut.CheckInAsync("CODE", TestContext.Current.CancellationToken);
        Assert.Equal(TicketStatus.Used, ticket.Status);
        Assert.NotNull(ticket.UsedAt);
    }

    [Fact]
    public async Task CheckIn_CancelledTicket_ThrowsConflict()
    {
        Arrange(TicketStatus.Cancelled, DateTime.UtcNow.AddMinutes(30));
        var ex = await Assert.ThrowsAsync<ConflictException>(() => _sut.CheckInAsync("CODE", TestContext.Current.CancellationToken));
        Assert.Equal(Errors.Ticket.Cancelled.Code, ex.ErrorCode);
    }

    [Fact]
    public async Task CheckIn_AlreadyUsed_ThrowsConflict()
    {
        Arrange(TicketStatus.Used, DateTime.UtcNow.AddMinutes(30));
        var ex = await Assert.ThrowsAsync<ConflictException>(() => _sut.CheckInAsync("CODE", TestContext.Current.CancellationToken));
        Assert.Equal(Errors.Ticket.AlreadyUsed.Code, ex.ErrorCode);
    }

    [Fact]
    public async Task CheckIn_TooEarly_ThrowsConflict()
    {
        Arrange(TicketStatus.Valid, DateTime.UtcNow.AddHours(5));
        var ex = await Assert.ThrowsAsync<ConflictException>(() => _sut.CheckInAsync("CODE", TestContext.Current.CancellationToken));
        Assert.Equal(Errors.Ticket.NotValidNow.Code, ex.ErrorCode);
    }

    [Fact]
    public async Task CheckIn_UnknownCode_ThrowsNotFound()
    {
        _tickets.GetByCodeTrackedAsync("NOPE", Arg.Any<CancellationToken>()).Returns((Ticket?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.CheckInAsync("NOPE", TestContext.Current.CancellationToken));
    }
}