using CinemaGo.Application.Interfaces;
using CinemaGo.Application.Interfaces.Services;
using CinemaGo.Domain.Constants;
using CinemaGo.Domain.DTOs;
using CinemaGo.Domain.Exceptions;
using CinemaGo.Domain.Interfaces.External;
using CinemaGo.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
namespace CinemaGo.Application.Services;

public class UserService : IUserService
{
    private readonly IIdentityService _identity;
    private readonly IBookingRepository _bookings;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UserService> _logger;

    public UserService(IIdentityService identity, IBookingRepository bookings, IUnitOfWork uow,
        ICurrentUserService currentUser, ILogger<UserService> logger)
    { _identity = identity; _bookings = bookings; _uow = uow; _currentUser = currentUser; _logger = logger; }

    public Task<PagedResult<UserResponse>> GetPagedAsync(UserFilter filter, CancellationToken ct = default)
        => _identity.GetUsersAsync(filter, ct);

    public async Task<UserResponse> CreateAdminAsync(CreateAdminRequest r, CancellationToken ct = default)
    {
        var admin = await _identity.CreateUserAsync(r.UserName, r.Email, r.Password, RoleNames.Admin, activate: true, ct);
        _logger.LogInformation("Admin account created: {UserId}", admin.Id);
        return admin;
    }

    public async Task<UserResponse> ChangeRoleAsync(string userId, ChangeRoleRequest r, CancellationToken ct = default)
    {
        await EnsureCanManageAsync(userId, ct);
        await _identity.SetRoleAsync(userId, r.Role, ct);
        _logger.LogInformation("Role of user {UserId} changed to {Role}", userId, r.Role);
        return (await _identity.FindByIdAsync(userId, ct))!;
    }

    public async Task<UserResponse> SetActiveAsync(string userId, SetActiveRequest r, CancellationToken ct = default)
    {
        await EnsureCanManageAsync(userId, ct);
        await _identity.SetActiveAsync(userId, r.IsActive, ct);
        _logger.LogInformation("User {UserId} active status set to {IsActive}", userId, r.IsActive);
        return (await _identity.FindByIdAsync(userId, ct))!;
    }

    public async Task DeleteAsync(string userId, CancellationToken ct = default)
    {
        var actorId = _currentUser.UserId ?? throw new UnauthorizedException();
        if (userId == actorId) throw new BadRequestException(Errors.User.CannotDeleteViaAdmin);
        await EnsureCanManageAsync(userId, ct);
        await DeleteCoreAsync(userId, ct);
    }

    public async Task DeleteMyAccountAsync(DeleteAccountRequest r, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException();
        var me = await _identity.FindByIdAsync(userId, ct) ?? throw new NotFoundException("User", userId);
        if (me.Roles.Contains(RoleNames.SuperAdmin)) throw new ForbiddenException(Errors.User.SuperAdminProtected);
        if (!await _identity.CheckPasswordAsync(userId, r.Password, ct)) throw new UnauthorizedException(Errors.Auth.InvalidCredentials);
        await DeleteCoreAsync(userId, ct);
    }

    // SuperAdmin: never touched. Admin: customers only. SuperAdmin: admins and customers.
    private async Task EnsureCanManageAsync(string targetId, CancellationToken ct)
    {
        var target = await _identity.FindByIdAsync(targetId, ct) ?? throw new NotFoundException("User", targetId);
        if (target.Roles.Contains(RoleNames.SuperAdmin)) throw new ForbiddenException(Errors.User.SuperAdminProtected);
        if (!_currentUser.IsInRole(RoleNames.SuperAdmin) && !target.Roles.All(role => role == RoleNames.Customer))
            throw new ForbiddenException(Errors.User.AdminCannotManageStaff);
    }

    private async Task DeleteCoreAsync(string userId, CancellationToken ct)
    {
        var bookings = await _bookings.GetActiveFutureByUserAsync(userId, ct);
        var now = DateTime.UtcNow;
        foreach (var booking in bookings) booking.Cancel(now);   // booking + ticket
        if (bookings.Count > 0) await _uow.SaveChangesAsync(ct);

        await _identity.AnonymizeAsync(userId, ct);
        _logger.LogInformation("User {UserId} deleted by {ActorId}; {Count} future booking(s) cancelled",
            userId, _currentUser.UserId, bookings.Count);
    }
}