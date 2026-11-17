using CinemaGo.Application.Interfaces;
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

public class UserServiceTests
{
    private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
    private readonly IBookingRepository _bookings = Substitute.For<IBookingRepository>();
    private readonly ICurrentUserService _current = Substitute.For<ICurrentUserService>();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _current.UserId.Returns("actor");
        _bookings.GetActiveFutureByUserAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new List<Booking>());
        _sut = new UserService(_identity, _bookings, Substitute.For<IUnitOfWork>(), _current, Substitute.For<IEmailSender>(), NullLogger<UserService>.Instance);
    }

    private void Target(string id, params string[] roles)
        => _identity.FindByIdAsync(id, Arg.Any<CancellationToken>()).Returns(new UserResponse { Id = id, Roles = roles.ToList() });

    [Fact]
    public async Task Admin_CannotDeleteAnotherAdmin()
    {
        _current.IsInRole(RoleNames.SuperAdmin).Returns(false);
        Target("t", RoleNames.Admin);
        await Assert.ThrowsAsync<ForbiddenException>(() => _sut.DeleteAsync("t", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Admin_CanDeleteCustomer_AndFutureBookingsAndTicketsAreCancelled()
    {
        _current.IsInRole(RoleNames.SuperAdmin).Returns(false);
        Target("t", RoleNames.Customer);
        var booking = new Booking { Id = 1, Ticket = new Ticket() };
        _bookings.GetActiveFutureByUserAsync("t", Arg.Any<CancellationToken>()).Returns(new List<Booking> { booking });

        await _sut.DeleteAsync("t", TestContext.Current.CancellationToken);

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        Assert.Equal(TicketStatus.Cancelled, booking.Ticket!.Status);
        await _identity.Received(1).AnonymizeAsync("t", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SuperAdmin_CanDeleteAdmin()
    {
        _current.IsInRole(RoleNames.SuperAdmin).Returns(true);
        Target("t", RoleNames.Admin);
        await _sut.DeleteAsync("t", TestContext.Current.CancellationToken);
        await _identity.Received(1).AnonymizeAsync("t", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Nobody_CanDeleteTheSuperAdmin()
    {
        _current.IsInRole(RoleNames.SuperAdmin).Returns(true);
        Target("t", RoleNames.SuperAdmin);
        await Assert.ThrowsAsync<ForbiddenException>(() => _sut.DeleteAsync("t", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DeletingYourselfThroughTheAdminEndpoint_IsRejected()
        => await Assert.ThrowsAsync<BadRequestException>(() => _sut.DeleteAsync("actor", TestContext.Current.CancellationToken));

    [Fact]
    public async Task DeleteMyAccount_WrongPassword_ThrowsUnauthorized()
    {
        Target("actor", RoleNames.Customer);
        _identity.CheckPasswordAsync("actor", "bad", Arg.Any<CancellationToken>()).Returns(false);
        await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.DeleteMyAccountAsync(new DeleteAccountRequest { Password = "bad" }, TestContext.Current.CancellationToken));
    }
}