using CinemaGo.Domain.DTOs;
namespace CinemaGo.Domain.Interfaces.External;

public interface IIdentityService
{
    Task<UserResponse?> FindByEmailAsync(string email, CancellationToken ct = default);
    Task<UserResponse?> FindByIdAsync(string id, CancellationToken ct = default);
    Task<bool> CheckPasswordAsync(string userId, string password, CancellationToken ct = default);   // counts failures, enforces lockout
    Task<UserResponse> CreateUserAsync(string userName, string email, string password, string role, bool activate, CancellationToken ct = default);
    Task<PagedResult<UserResponse>> GetUsersAsync(UserFilter filter, CancellationToken ct = default);
    Task<List<UserResponse>> GetByIdsAsync(IEnumerable<string> ids, CancellationToken ct = default);
    Task SetRoleAsync(string userId, string role, CancellationToken ct = default);
    Task SetActiveAsync(string userId, bool isActive, CancellationToken ct = default);   // activating also confirms the e-mail

    Task<string> GenerateEmailConfirmationCodeAsync(string userId, CancellationToken ct = default);
    Task ConfirmEmailAsync(string userId, string code, CancellationToken ct = default);
    Task<string> GeneratePasswordResetCodeAsync(string userId, CancellationToken ct = default);
    Task ResetPasswordAsync(string userId, string code, string newPassword, CancellationToken ct = default);
    Task ChangePasswordAsync(string userId, string currentPassword, string newPassword, CancellationToken ct = default);

    Task<string> GetSecurityStampAsync(string userId, CancellationToken ct = default);
    Task<bool> IsSessionValidAsync(string userId, string? securityStamp, CancellationToken ct = default);
    Task InvalidateSessionsAsync(string userId, CancellationToken ct = default);   // logout
    Task AnonymizeAsync(string userId, CancellationToken ct = default);            // delete account
}