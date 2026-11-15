using CinemaGo.Domain.Constants;
using CinemaGo.Domain.DTOs;
using CinemaGo.Domain.Exceptions;
using CinemaGo.Domain.Interfaces.External;
using CinemaGo.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace CinemaGo.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private const string ConfirmPurpose = "ConfirmEmail";
    private const string ResetPurpose = "ResetPassword";

    private readonly UserManager<ApplicationUser> _users;
    private readonly CinemaDbContext _db;

    public IdentityService(UserManager<ApplicationUser> users, CinemaDbContext db) { _users = users; _db = db; }

    // deleted (anonymized) accounts behave as if they do not exist
    private async Task<ApplicationUser> GetAsync(string id)
    {
        var user = await _users.FindByIdAsync(id);
        return user is { DeletedAt: null } ? user : throw new NotFoundException("User", id);
    }

    public async Task<UserResponse?> FindByEmailAsync(string email, CancellationToken ct = default)
    {
        var user = await _users.FindByEmailAsync(email);
        return user is not { DeletedAt: null } ? null : Map(user, await _users.GetRolesAsync(user));
    }

    public async Task<UserResponse?> FindByIdAsync(string id, CancellationToken ct = default)
    {
        var user = await _users.FindByIdAsync(id);
        return user is not { DeletedAt: null } ? null : Map(user, await _users.GetRolesAsync(user));
    }

    public async Task<bool> CheckPasswordAsync(string userId, string password, CancellationToken ct = default)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user is null) return false;
        if (await _users.IsLockedOutAsync(user)) throw new ForbiddenException(ErrorMessages.AccountLocked, ErrorCodes.AccountLocked);

        if (await _users.CheckPasswordAsync(user, password))
        {
            await _users.ResetAccessFailedCountAsync(user);
            return true;
        }
        await _users.AccessFailedAsync(user);   // 5 failures -> locked for 15 minutes (see DependencyInjection)
        return false;
    }

    public async Task<UserResponse> CreateUserAsync(string userName, string email, string password, string role, bool activate, CancellationToken ct = default)
    {
        if (await _users.FindByEmailAsync(email) is not null) throw new ConflictException(ErrorMessages.EmailTaken);
        if (await _users.FindByNameAsync(userName) is not null) throw new ConflictException(ErrorMessages.UserNameTaken);

        var user = new ApplicationUser { UserName = userName, Email = email, IsActive = activate, EmailConfirmed = activate };
        var created = await _users.CreateAsync(user, password);   // Identity hashes the password and applies the policy
        if (!created.Succeeded) throw new ValidationFailedException(ToErrors(created));

        var inRole = await _users.AddToRoleAsync(user, role);
        if (!inRole.Succeeded)
        {
            await _users.DeleteAsync(user);   // never leave a user without a role
            throw new ValidationFailedException(ToErrors(inRole));
        }
        return Map(user, new[] { role });
    }

    public async Task<PagedResult<UserResponse>> GetUsersAsync(UserFilter f, CancellationToken ct = default)
    {
        var q = _users.Users.AsNoTracking().Where(u => u.DeletedAt == null);
        if (!string.IsNullOrWhiteSpace(f.Search))
            q = q.Where(u => u.UserName!.Contains(f.Search) || u.Email!.Contains(f.Search));
        if (!string.IsNullOrWhiteSpace(f.Role))
        {
            var roleId = await _db.Roles.Where(r => r.Name == f.Role).Select(r => r.Id).FirstOrDefaultAsync(ct);
            q = q.Where(u => _db.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == roleId));
        }

        var total = await q.CountAsync(ct);
        var page = await q.OrderBy(u => u.UserName).Skip((f.Page - 1) * f.PageSize).Take(f.PageSize).ToListAsync(ct);
        return new PagedResult<UserResponse> { Items = await MapManyAsync(page, ct), Page = f.Page, PageSize = f.PageSize, TotalCount = total };
    }

    public async Task<List<UserResponse>> GetByIdsAsync(IEnumerable<string> ids, CancellationToken ct = default)
    {
        var idList = ids.ToList();
        var users = await _users.Users.AsNoTracking().Where(u => idList.Contains(u.Id) && u.DeletedAt == null).ToListAsync(ct);
        return await MapManyAsync(users, ct);
    }

    public async Task SetRoleAsync(string userId, string role, CancellationToken ct = default)
    {
        var user = await GetAsync(userId);
        await _users.RemoveFromRolesAsync(user, await _users.GetRolesAsync(user));
        var result = await _users.AddToRoleAsync(user, role);
        if (!result.Succeeded) throw new ValidationFailedException(ToErrors(result));
        await _users.UpdateSecurityStampAsync(user);   // role is inside the token, so old tokens must die
    }

    public async Task SetActiveAsync(string userId, bool isActive, CancellationToken ct = default)
    {
        var user = await GetAsync(userId);
        user.IsActive = isActive;
        if (isActive) user.EmailConfirmed = true;   // an admin activating an account replaces the e-mail step
        var result = await _users.UpdateAsync(user);
        if (!result.Succeeded) throw new ValidationFailedException(ToErrors(result));
    }

    // ---------- 6-digit codes (TOTP derived from the security stamp: no table, ~10 minutes, single use) ----------
    public async Task<string> GenerateEmailConfirmationCodeAsync(string userId, CancellationToken ct = default)
        => await _users.GenerateUserTokenAsync(await GetAsync(userId), TokenOptions.DefaultEmailProvider, ConfirmPurpose);

    public async Task ConfirmEmailAsync(string userId, string code, CancellationToken ct = default)
    {
        var user = await GetAsync(userId);
        if (user.EmailConfirmed) return;   // idempotent; never re-activates an account an admin disabled
        if (!await _users.VerifyUserTokenAsync(user, TokenOptions.DefaultEmailProvider, ConfirmPurpose, code))
            throw new BadRequestException(ErrorMessages.InvalidCode, ErrorCodes.InvalidCode);

        user.EmailConfirmed = true;
        user.IsActive = true;
        await _users.UpdateAsync(user);
        await _users.UpdateSecurityStampAsync(user);   // makes the code single-use
    }

    public async Task<string> GeneratePasswordResetCodeAsync(string userId, CancellationToken ct = default)
        => await _users.GenerateUserTokenAsync(await GetAsync(userId), TokenOptions.DefaultEmailProvider, ResetPurpose);

    public async Task ResetPasswordAsync(string userId, string code, string newPassword, CancellationToken ct = default)
    {
        var user = await GetAsync(userId);
        if (!await _users.VerifyUserTokenAsync(user, TokenOptions.DefaultEmailProvider, ResetPurpose, code))
            throw new BadRequestException(ErrorMessages.InvalidCode, ErrorCodes.InvalidCode);

        // Identity's own reset token applies the password policy and rotates the stamp (code becomes unusable, old sessions die)
        var token = await _users.GeneratePasswordResetTokenAsync(user);
        var result = await _users.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded) throw new ValidationFailedException(ToErrors(result));

        await _users.SetLockoutEndDateAsync(user, null);
        await _users.ResetAccessFailedCountAsync(user);
    }

    public async Task ChangePasswordAsync(string userId, string currentPassword, string newPassword, CancellationToken ct = default)
    {
        var user = await GetAsync(userId);
        var result = await _users.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded) throw new ValidationFailedException(ToErrors(result));
    }

    // ---------- sessions (logout / validation of every request) ----------
    public async Task<string> GetSecurityStampAsync(string userId, CancellationToken ct = default)
        => (await GetAsync(userId)).SecurityStamp ?? string.Empty;

    public async Task<bool> IsSessionValidAsync(string userId, string? securityStamp, CancellationToken ct = default)
    {
        var user = await _users.FindByIdAsync(userId);
        return user is { DeletedAt: null, IsActive: true } && securityStamp is not null && user.SecurityStamp == securityStamp;
    }

    public async Task InvalidateSessionsAsync(string userId, CancellationToken ct = default)
        => await _users.UpdateSecurityStampAsync(await GetAsync(userId));

    // delete account = anonymize: history (bookings, revenue) stays, personal data goes
    public async Task AnonymizeAsync(string userId, CancellationToken ct = default)
    {
        var user = await GetAsync(userId);
        var marker = Guid.NewGuid().ToString("N");

        await _users.RemoveFromRolesAsync(user, await _users.GetRolesAsync(user));
        if (await _users.HasPasswordAsync(user)) await _users.RemovePasswordAsync(user);
        await _users.SetUserNameAsync(user, $"deleted_{marker}");
        await _users.SetEmailAsync(user, $"deleted_{marker}@deleted.invalid");

        user.IsActive = false;
        user.EmailConfirmed = false;
        user.DeletedAt = DateTime.UtcNow;
        await _users.UpdateAsync(user);
        await _users.UpdateSecurityStampAsync(user);
    }

    // ---------- helpers ----------
    private async Task<List<UserResponse>> MapManyAsync(List<ApplicationUser> users, CancellationToken ct)
    {
        var ids = users.Select(u => u.Id).ToList();
        var pairs = await (from ur in _db.UserRoles
                           join r in _db.Roles on ur.RoleId equals r.Id
                           where ids.Contains(ur.UserId)
                           select new { ur.UserId, r.Name }).ToListAsync(ct);
        var byUser = pairs.GroupBy(p => p.UserId).ToDictionary(g => g.Key, g => g.Select(p => p.Name!).ToList());
        return users.Select(u => Map(u, byUser.GetValueOrDefault(u.Id) ?? new List<string>())).ToList();
    }

    private static UserResponse Map(ApplicationUser u, IEnumerable<string> roles) => new()
    {
        Id = u.Id,
        UserName = u.UserName ?? string.Empty,
        Email = u.Email ?? string.Empty,
        IsActive = u.IsActive,
        EmailConfirmed = u.EmailConfirmed,
        CreatedAt = u.CreatedAt,
        Roles = roles.ToList()
    };

    private static Dictionary<string, string[]> ToErrors(IdentityResult result)
        => result.Errors
            .GroupBy(e => e.Code.StartsWith("Password") ? "password" : "user")
            .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
}